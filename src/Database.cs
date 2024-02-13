using Npgsql;
using NpgsqlTypes;

public class Database
{
  private NpgsqlConnection _conn;

  public Database(string connectionString)
  {
    Console.WriteLine(connectionString);
    _conn = new NpgsqlConnection(connectionString);
    _conn.Open();
  }

  public async Task Connect()
  {
    await _conn.OpenAsync();
  }
  public async Task<bool> ClientExists(int id)
  {
    string query = "SELECT id FROM customers where id = @id";

    var command = _conn.CreateCommand();
    command.CommandText = query;
    var idQuery = command.Parameters.Add($"id", NpgsqlDbType.Integer);
    idQuery.Value = id;
    command.Prepare();
    var result = await command.ExecuteScalarAsync();
    return result != null;
  }
  public Cliente? GetClientById(int id)
  {
    string query = "SELECT id, name, balance, account_limit FROM customers where id = @id";
    using var command = new NpgsqlCommand(query, _conn);
    command.Parameters.AddWithValue("@id", id);
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      int clientId = reader.GetInt32(0);
      string nome = reader.GetString(1);
      int saldo = reader.GetInt32(2);
      int limite = reader.GetInt32(3);
      return new Cliente { Id = clientId, Nome = nome, Saldo = saldo, Limite = limite };
    }
    return null;

  }
  public bool UpdateClient(Cliente cliente)
  {
    using var command = new NpgsqlCommand("UPDATE customers SET balance = @novoSaldo WHERE id = @id", _conn);
    command.Parameters.AddWithValue("@novoSaldo", cliente.Saldo);
    command.Parameters.AddWithValue("@id", cliente.Id);
    int rowsAffected = command.ExecuteNonQuery();
    return rowsAffected > 0;
  }

  public async Task<Estatement?> GetEstatement(int clientId)
  {
    var client = GetClientById(clientId);
    if (client == null)
    {
      return null;
    }
    List<ReadTransactions> transactions = new List<ReadTransactions>();

    string query = "SELECT value, description, type, operation_date FROM transactions WHERE customer_id = @id ORDER BY operation_date DESC LIMIT 10";

    using var command = new NpgsqlCommand(query, _conn);
    command.Parameters.AddWithValue("@id", clientId);
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
  public bool InsertTransaction(Transacao newTransaction)
  {
    string query = "INSERT INTO transactions (value, description, type, customer_id, operation_date) VALUES (@value, @description,@type, @customer_id, @date)";

    using var command = new NpgsqlCommand(query, _conn);
    command.Parameters.AddWithValue("@value", newTransaction.Valor);
    command.Parameters.AddWithValue("@description", newTransaction.Descricao);
    command.Parameters.AddWithValue("@type", newTransaction.Tipo);
    command.Parameters.AddWithValue("@customer_id", newTransaction.Client!.Id);
    command.Parameters.AddWithValue("@date", DateTime.Now);

    int rowsAffected = command.ExecuteNonQuery();
    return rowsAffected > 0;
  }

  public async Task<TransactionResult> ProcessTransaction(int clientId, Transacao newTransaction)
  {
    await Task.CompletedTask;
    // var command = new NpgsqlCommand("select pg_advisory_lock(@id)", _conn);
    // command.Parameters.AddWithValue("@id", clientId);
    // await command.ExecuteScalarAsync();
    var newT = new TransactionResult();
    var client = GetClientById(clientId)!;

    newTransaction.Client = client;
    var saldoOk = newTransaction.CanBeMade();
    if (!saldoOk)
    {
      newT.SetSucess(false);
      return newT;
    }
    newTransaction.DoOperation();
    UpdateClient(newTransaction.Client);

    // command = new NpgsqlCommand("select pg_advisory_unlock(@id)", _conn);
    // command.Parameters.AddWithValue("@id", clientId);
    // await command.ExecuteScalarAsync();

    InsertTransaction(newTransaction);
    newT.Limite = newTransaction.Client.Limite;
    newT.Saldo = newTransaction.Client.Saldo;
    return newT;
  }
}