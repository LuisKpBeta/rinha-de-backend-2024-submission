using Npgsql;
using NpgsqlTypes;

var builder = WebApplication.CreateBuilder(args);
string dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST")!;
if (dbHost == null)
{
  dbHost = "localhost";
}

string connectionString = $"Host=db;Port=5432;Username=postgres;Password=postgres;Database=rinhadb;Command Timeout=1";
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddSingleton<Database>(new Database(connectionString));

var app = builder.Build();

app.MapGet("/clientes/{id}/extrato", async (Database db, int id) =>
{
  var estatements = await db.GetEstatement(id);
  if (estatements == null)
  {
    return Results.NotFound();
  }
  return Results.Ok(estatements);
});

app.MapPost("/clientes/{id}/transacoes", async (Database db, int id, Transacao newTransaction) =>
{
  if (!newTransaction.IsValid())
  {
    return Results.UnprocessableEntity();
  }

  var exists = await db.ClientExists(id);
  if (!exists)
  {
    return Results.NotFound();
  }
  var operationResult = await db.ProcessTransaction(id, newTransaction);

  if (!operationResult.HasSuccess())
  {
    return Results.UnprocessableEntity();
  }

  return Results.Ok(operationResult);
});


app.Run();
