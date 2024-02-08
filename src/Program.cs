using Npgsql;
using NpgsqlTypes;

var builder = WebApplication.CreateBuilder(args);
string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=rinhadb;";
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddScoped<Database>();

var app = builder.Build();

app.MapGet("/clientes/{id}/extrato", (int id) =>
{
  var message = $"Hello World! {id}";
  return message;
});

app.MapPost("/clientes/{id}/transacoes", async (Database db, int id, Transacao newTransaction) =>
{
  if (!newTransaction.IsValid())
  {
    return Results.BadRequest();
  }
  await db.Connect();

  var exists = await db.ClientExists(id);
  if (!exists)
  {
    await db.EndConn();
    return Results.NotFound();
  }
  var operationResult = await db.ProcessTransaction(id, newTransaction);
  await db.EndConn();

  if (!operationResult.HasSuccess())
  {
    return Results.UnprocessableEntity();
  }

  return Results.Ok(operationResult);
});


app.Run();
