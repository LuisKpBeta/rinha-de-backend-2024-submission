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