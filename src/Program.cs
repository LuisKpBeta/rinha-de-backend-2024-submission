using Npgsql;
using NpgsqlTypes;

var builder = WebApplication.CreateBuilder(args);
string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=rinhadb;";
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddScoped<Database>();

var app = builder.Build();

app.MapGet("/clientes/{id}/extrato", async (Database db, int id) =>
{
  await db.Connect();
  var estatements = await db.GetEstatement(id);
  if (estatements == null)
  {
    await db.EndConn();
    return Results.NotFound();
  }
  return Results.Ok(estatements);
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
