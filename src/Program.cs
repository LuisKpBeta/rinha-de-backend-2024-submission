using Npgsql;
using NpgsqlTypes;

var builder = WebApplication.CreateBuilder(args);
string dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST")!;
if (dbHost == null)
{
  dbHost = "localhost";
}

string connectionString = $"Host={dbHost};Port=5432;Username=postgres;Password=postgres;Database=rinhadb;Pooling=true;Minimum Pool Size=20;Maximum Pool Size=2000;Enlist=false;";
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddScoped<Database>();

var app = builder.Build();

app.MapGet("/clientes/{id}/extrato", async (Database db, int id) =>
{
  await using (db._conn)
  {
    await db._conn.OpenAsync();
    var estatements = await db.GetEstatement(id);
    if (estatements == null)
    {
      return Results.NotFound();
    }
    return Results.Ok(estatements);
  }
});

app.MapPost("/clientes/{id}/transacoes", async (Database db, int id, NewTransacaoRequest newTransaction) =>
{

  if (!newTransaction.IsValid())
  {
    return Results.UnprocessableEntity();
  }

  await using (db._conn)
  {
    await db._conn.OpenAsync();
    var t = new Transacao
    {
      Valor = newTransaction.Valor,
      Descricao = newTransaction.Descricao,
      Tipo = newTransaction.Tipo
    };
    var operationResult = await db.ProcessTransaction(id, t);
    if (operationResult.ClientDoesNotExists)
    {
      return Results.NotFound();
    }

    if (!operationResult.Sucesso)
    {
      return Results.UnprocessableEntity();
    }

    return Results.Ok(operationResult);
  }
});


app.Run();
