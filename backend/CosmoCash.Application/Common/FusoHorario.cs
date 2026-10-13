namespace CosmoCash.Application.Common;

// O banco guarda instantes em UTC, mas "o dia da venda" é o dia de São Paulo.
// Uma venda às 22h de São Paulo já é "amanhã" em UTC; por isso a conversão fica centralizada aqui.
public static class FusoHorario
{
  private static readonly TimeZoneInfo _saoPaulo = Localizar();

  private static TimeZoneInfo Localizar()
  {
    // Linux/Docker usa o id IANA; Windows antigo usa o id próprio. O Brasil não tem horário de verão
    // desde 2019, então o fallback fixo em -03:00 também é correto para datas atuais.
    foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
    {
      try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
      catch (TimeZoneNotFoundException) { }
      catch (InvalidTimeZoneException) { }
    }

    return TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "BRT", "BRT");
  }

  public static DateOnly HojeEmSaoPaulo()
    => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _saoPaulo));

  // Meia-noite (horário de São Paulo) do dia informado, convertida para UTC.
  public static DateTime InicioDoDiaUtc(DateOnly dia)
    => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dia.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), _saoPaulo);

  // Converte o que veio da API para UTC.
  //  - com fuso (Z ou +hh:mm): usa o instante informado;
  //  - só data ("2026-10-12"): se for hoje, vira "agora"; senão, meio-dia de São Paulo desse dia;
  //  - data e hora sem fuso: interpreta como horário de São Paulo.
  public static DateTime ParaUtc(DateTime valor)
  {
    if (valor.Kind == DateTimeKind.Utc) return valor;
    if (valor.Kind == DateTimeKind.Local) return valor.ToUniversalTime();

    if (valor.TimeOfDay == TimeSpan.Zero)
    {
      var dia = DateOnly.FromDateTime(valor);
      return dia == HojeEmSaoPaulo()
        ? DateTime.UtcNow
        : TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(valor.AddHours(12), DateTimeKind.Unspecified), _saoPaulo);
    }

    return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(valor, DateTimeKind.Unspecified), _saoPaulo);
  }
}
