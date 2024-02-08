using Npgsql;
using NpgsqlTypes;

public class Database
{
  private NpgsqlConnection _conn;

  public Database(NpgsqlConnection conn)
  {
    _conn = conn;
  }

  public async Task Connect()
  {
    await _conn.OpenAsync();
  }
  public async Task EndConn()
  {
    await _conn.CloseAsync();
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
    InsertTransaction(newTransaction);
    newT.Limite = newTransaction.Client.Limite;
    newT.Saldo = newTransaction.Client.Saldo;
    return newT;
  }
}