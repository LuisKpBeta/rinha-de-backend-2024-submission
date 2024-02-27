using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});
string dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST")!;
if (dbHost == null)
{
  dbHost = "localhost";
}

string connectionString = $"Host={dbHost};Port=5432;Username=postgres;Password=postgres;Database=rinhadb;Pooling=true;Minimum Pool Size=20;Maximum Pool Size=50;Enlist=false;";
builder.Services.AddNpgsqlDataSource(connectionString);
builder.Services.AddScoped<Database>();

var app = builder.Build();

app.MapGet("/clientes/{id}/extrato", async (Database db, int id) =>
{
  await using (db._conn)
  {
    if (!db.ClientExists(id))
    {
      return Results.NotFound();
    }
    await db._conn.OpenAsync();
    var estatements = await db.GetEstatement(id);
    return Results.Ok(estatements);
  }
});

app.MapPost("/clientes/{id}/transacoes", async (Database db, int id, NewTransacaoRequest newTransaction) =>
{
  if (!db.ClientExists(id))
  {
    return Results.NotFound();
  }

  if (!newTransaction.IsValid())
  {
    return Results.UnprocessableEntity();
  }

  await using (db._conn)
  {
    var t = new Transacao
    {
      Valor = newTransaction.IntValor,
      Descricao = newTransaction.Descricao,
      Tipo = newTransaction.Tipo
    };
    await db._conn.OpenAsync();
    var operationResult = await db.ProcessTransaction(id, t);

    if (!operationResult.Sucesso)
    {
      return Results.UnprocessableEntity();
    }

    return Results.Ok(operationResult);
  }
});


app.Run();
