using Npgsql;
using NpgsqlTypes;

public class Database
{
  public NpgsqlConnection _conn;
  public Database(NpgsqlConnection conn)
  {
    _conn = conn;
  }
  public async Task<Cliente?> GetClientById(int id)
  {
    using var command = _conn.CreateCommand();
    command.CommandText = "SELECT id, balance, account_limit FROM customers where id = @id;";
    command.Parameters.AddWithValue("id", id);
    await command.PrepareAsync();
    using var reader = await command.ExecuteReaderAsync();

    while (reader.Read())
    {
      int clientId = reader.GetInt32(0);
      int saldo = reader.GetInt32(1);
      int limite = reader.GetInt32(2);
      return new Cliente { Id = clientId, Saldo = saldo, Limite = limite };
    }
    return null;
  }


  public async Task<Estatement?> GetEstatement(int clientId)
  {

    //select balance as value, account_limit, 'type' as type, 'account_limit' as description, now() as operation_date from customers where id = 1) union all (select value, 0 as account_limit, type, description, operation_date from transactions where customer_id = 1 limit 10);
    List<ReadTransactions> transactions = new List<ReadTransactions>();
    using var command = _conn.CreateCommand();
    // command.CommandText = "SELECT value, description, type, operation_date FROM transactions WHERE customer_id = @id ORDER BY operation_date DESC LIMIT 10";
    command.CommandText = @"(select balance as value, account_limit, 'type' as type, 'account_limit' as description, now() as operation_date from customers
                              where id = @id) 
                              union all 
                              (select value, 0 as account_limit, type, description, operation_date from transactions where customer_id = @id order by operation_date desc limit 10);";

    command.Parameters.AddWithValue("id", clientId);
    await command.PrepareAsync();
    using var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();
    int saldo = reader.GetInt32(0);
    int limite = reader.GetInt32(1);
    var dataExtrato = reader.GetDateTime(4);
    while (await reader.ReadAsync())
    {
      int value = reader.GetInt32(0);
      string description = reader.GetString(3);
      string type = reader.GetString(2);
      DateTime date = reader.GetDateTime(4);

      transactions.Add(new ReadTransactions { Descricao = description, Tipo = type, Valor = value, RealizadaEm = date });
    }

    var saldoC = new ClientBalance
    {
      Limite = limite,
      Total = saldo,
      DataExtrato = dataExtrato
    };
    var estatements = new Estatement
    {
      Saldo = saldoC,
      UltimasTransacoes = transactions,

    };
    return estatements;
  }
  public async Task<bool> InsertTransaction(Transacao newTransaction, int clientId)
  {
    using var command = _conn.CreateCommand();
    command.CommandText = "INSERT INTO transactions (value, description, type, customer_id, operation_date) VALUES (@value, @description,@type, @customer_id, @date)";
    command.Parameters.AddWithValue("@value", newTransaction.Valor!);
    command.Parameters.AddWithValue("@description", newTransaction.Descricao!);
    command.Parameters.AddWithValue("@type", newTransaction.Tipo!);
    command.Parameters.AddWithValue("@customer_id", clientId);
    command.Parameters.AddWithValue("@date", DateTime.Now);
    await command.PrepareAsync();
    int rowsAffected = await command.ExecuteNonQueryAsync();
    command.Parameters.Clear();
    return rowsAffected > 0;
  }
  public async Task<(bool, int, int)> UpdateClient(int clienteId, int valor)
  {
    using var command = _conn.CreateCommand();
    command.CommandText = "UPDATE customers SET balance = @valor + balance WHERE id = @id and (balance + @valor + account_limit)>=0  RETURNING balance, account_limit ;";
    command.Parameters.AddWithValue("valor", valor);
    command.Parameters.AddWithValue("id", clienteId);
    await command.PrepareAsync();
    using var reader = await command.ExecuteReaderAsync();
    while (reader.Read())
    {
      int saldo = reader.GetInt32(0);
      int limit = reader.GetInt32(1);
      return (true, saldo, limit);
    }
    return (false, 0, 0);
  }

  public async Task<(TransactionResult, bool)> ProcessTransaction(int clientId, Transacao newTransaction)
  {

    var newT = new TransactionResult();

    var valor = newTransaction.GetValue();

    var (ok, saldo, limit) = await UpdateClient(clientId, valor);

    if (!ok)
    {
      return (newT, false);
    }
    await InsertTransaction(newTransaction, clientId);

    newT.Limite = limit;
    newT.Saldo = saldo;
    return (newT, true);
  }

  public bool ClientExists(int id)
  {
    return id >= 1 && id <= 5;
  }
}