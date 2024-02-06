var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/clientes/{id}/extrato", (int id) =>
{
  var message = $"Hello World! {id}";
  return message;
});

app.MapPost("/clientes/{id}/transacoes", (int id) => $"recebido para {id}");


app.Run();
