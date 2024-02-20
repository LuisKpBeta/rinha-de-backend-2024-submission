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
    var client = await GetClientById(clientId);
    if (client == null)
    {
      return null;
    }
    List<ReadTransactions> transactions = new List<ReadTransactions>();
    using var command = _conn.CreateCommand();
    command.CommandText = "SELECT value, description, type, operation_date FROM transactions WHERE customer_id = @id ORDER BY operation_date DESC LIMIT 10";
    command.Parameters.AddWithValue("id", clientId);
    await command.PrepareAsync();
    using var reader = await command.ExecuteReaderAsync();
    while (reader.Read())
    {
      int value = reader.GetInt32(0);
      string description = reader.GetString(1);
      string type = reader.GetString(2);
      DateTime date = reader.GetDateTime(3);

      transactions.Add(new ReadTransactions { Descricao = description, Tipo = type, Valor = value, RealizadaEm = date });
    }

    var saldo = new ClientBalance
    {
      Limite = client.Limite,
      Total = client.Saldo,
    };
    var estatements = new Estatement
    {
      Saldo = saldo,
      UltimasTransacoes = transactions
    };
    return estatements;
  }
  public async Task<bool> InsertTransaction(Transacao newTransaction, NpgsqlTransaction? t = null)
  {
    using var command = _conn.CreateCommand();
    command.CommandText = "INSERT INTO transactions (value, description, type, customer_id, operation_date) VALUES (@value, @description,@type, @customer_id, @date)";
    command.Parameters.AddWithValue("@value", newTransaction.Valor!);
    command.Parameters.AddWithValue("@description", newTransaction.Descricao!);
    command.Parameters.AddWithValue("@type", newTransaction.Tipo!);
    command.Parameters.AddWithValue("@customer_id", newTransaction.Client!.Id);
    command.Parameters.AddWithValue("@date", DateTime.Now);
    await command.PrepareAsync();
    int rowsAffected = await command.ExecuteNonQueryAsync();
    command.Parameters.Clear();
    return rowsAffected > 0;
  }
  public async Task<bool> UpdateClient(Cliente cliente)
  {
    using var command = _conn.CreateCommand();
    command.CommandText = "UPDATE customers SET balance = @novoSaldo WHERE id = @id;";
    command.Parameters.AddWithValue("novoSaldo", cliente.Saldo);
    command.Parameters.AddWithValue("id", cliente.Id);
    await command.PrepareAsync();
    int rowsAffected = await command.ExecuteNonQueryAsync();
    return rowsAffected > 0;
  }

  public async Task<TransactionResult> ProcessTransaction(int clientId, Transacao newTransaction)
  {
    // var command = new NpgsqlCommand("select pg_advisory_lock(@id)", _conn);
    // command.Parameters.AddWithValue("@id", clientId);
    // await command.ExecuteScalarAsync();
    await using var t = await _conn.BeginTransactionAsync();
    var newT = new TransactionResult();
    try
    {

      var client = await GetClientById(clientId)!;
      if (client == null)
      {
        newT.ClientDoesNotExists = true;
        return newT;
      }
      newTransaction.Client = client;
      var saldoOk = newTransaction.CanBeMade();
      if (!saldoOk)
      {
        newT.Sucesso = false;
        return newT;
      }
      newTransaction.DoOperation();
      await UpdateClient(newTransaction.Client);
      await InsertTransaction(newTransaction);

      // command = new NpgsqlCommand("select pg_advisory_unlock(@id)", _conn);
      // command.Parameters.AddWithValue("@id", clientId);
      // await command.ExecuteScalarAsync();

      await t.CommitAsync();
      newT.Limite = newTransaction.Client.Limite;
      newT.Saldo = newTransaction.Client.Saldo;
      return newT;
    }
    catch (Exception er)
    {
      Console.WriteLine(er);
      await t.RollbackAsync();
      newT.Sucesso = false;
      return newT;
    }
  }
}