public class NewTransacaoRequest
{
  public object? Valor { get; set; }
  public int IntValor { get; set; }
  public string? Descricao { get; set; }
  public string? Tipo { get; set; }
  public bool IsValid()
  {
    bool success = int.TryParse(Valor?.ToString(), out int convertedValue);
    if (!success)
    {
      return false;
    }
    IntValor = convertedValue;
    var valueOk = IntValor >= 0;
    var descriptionOk = !string.IsNullOrEmpty(Descricao) && Descricao.Length >= 1 && Descricao.Length <= 10;
    var typeOk = !string.IsNullOrEmpty(Tipo) && Tipo == "c" || Tipo == "d";

    return valueOk && descriptionOk & typeOk;
  }
}

public class Transacao
{
  public Cliente? Client { get; set; }

  public int Valor { get; set; }
  public string? Descricao { get; set; }
  public string? Tipo { get; set; }

  public DateTime RealizadaEm { get; set; }

  public int GetValue()
  {
    if (Tipo == "c")
    {
      return Valor!;
    }

    return Valor! * -1;
  }

}

public class Cliente
{
  public int Id { get; set; }
  public int Saldo { get; set; }
  public int Limite { get; set; }

}
