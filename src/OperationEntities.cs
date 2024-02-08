public class TransactionResult
{
  public int Limite { get; set; }
  public int Saldo { get; set; }
  private bool Sucesso = true;

  public void SetSucess(bool value)
  {
    Sucesso = value;
  }
  public bool HasSuccess()
  {
    return Sucesso;
  }
}

public class Estatement
{
  public required ClientBalance Saldo { get; set; }

  public List<ReadTransactions> UltimasTransacoes { get; set; } = new List<ReadTransactions>();
}
public class ClientBalance
{
  public int Total { get; set; }
  public int Limite { get; set; }
  public DateTime DataExtrato { get; } = DateTime.Now;
}
public class ReadTransactions
{
  public int Valor { get; set; }
  public required string Descricao { get; set; }
  public required string Tipo { get; set; }

  public DateTime RealizadaEm { get; set; }
}