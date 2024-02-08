public class Transacao
{
  public Cliente? Client { get; set; }

  public int Valor { get; set; }
  public required string Descricao { get; set; }
  public required string Tipo { get; set; }

  public DateTime RealizadaEm { get; set; }

  public bool IsValid()
  {
    var valueOk = Valor >= 0;
    var descriptionOk = Descricao.Length >= 1 && Descricao.Length <= 10;
    var typeOk = Tipo == "c" || Tipo == "d";

    return valueOk && descriptionOk & typeOk;
  }
  public bool CanBeMade()
  {
    if (Tipo == "c")
    {
      return true;
    }
    var total = Client!.Saldo + Client!.Limite;
    return Valor < total;
  }
  public void DoOperation()
  {
    if (Tipo == "c")
    {
      Client!.Saldo += Valor;
      return;
    }
    Client!.Saldo -= Valor;
  }

}

public class Cliente
{
  public int Id { get; set; }
  public required string Nome { get; set; }
  public int Saldo { get; set; }
  public int Limite { get; set; }

}
