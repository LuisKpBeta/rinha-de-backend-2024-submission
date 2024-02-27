using System.Text.Json.Serialization;

public record TransactionResult
{
  public int Limite { get; set; }
  public int Saldo { get; set; }
}

public record Estatement
{
  public required ClientBalance Saldo { get; set; }

  public List<ReadTransactions> UltimasTransacoes { get; set; } = new List<ReadTransactions>();
}
public record ClientBalance
{
  public int Total { get; set; }
  public int Limite { get; set; }
  public DateTime DataExtrato { get; set; } = DateTime.Now;
}
public record ReadTransactions
{
  public int Valor { get; set; }
  public required string Descricao { get; set; }
  public required string Tipo { get; set; }
  public DateTime RealizadaEm { get; set; }
}