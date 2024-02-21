public class NewTransacaoRequest
{
  public double? Valor { get; set; }
  public int IntValor { get; set; }
  public string? Descricao { get; set; }
  public string? Tipo { get; set; }
  public bool IsValid()
  {
    if ((int)Valor! != Valor!)
    {
      return false;
    }
    IntValor = (int)Valor;
    var valueOk = IntValor >= 0;
    var descriptionOk = !string.IsNullOrEmpty(Descricao) && Descricao.Length >= 1 && Descricao.Length <= 10;
    var typeOk = !string.IsNullOrEmpty(Tipo) && Tipo == "c" || Tipo == "d";

    return valueOk && descriptionOk & typeOk;
  }
}

public class Transacao
{
  public Cliente? Client { get; set; }

  public int? Valor { get; set; }
  public string? Descricao { get; set; }
  public string? Tipo { get; set; }

  public DateTime RealizadaEm { get; set; }

  public bool IsValid()
  {
    var valueOk = Valor >= 0;
    var descriptionOk = !string.IsNullOrEmpty(Descricao) && Descricao.Length >= 1 && Descricao.Length <= 10;
    var typeOk = !string.IsNullOrEmpty(Tipo) && Tipo == "c" || Tipo == "d";

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
      Client!.Saldo += Valor ?? 0;
      return;
    }
    Client!.Saldo -= Valor ?? 0;
  }

}

public class Cliente
{
  public int Id { get; set; }
  public int Saldo { get; set; }
  public int Limite { get; set; }

}
