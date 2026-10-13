namespace CosmoCash.Application.Common;

public static class Documentos
{
  public static string ApenasDigitos(string? texto)
    => new string((texto ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

  // CPF válido: 11 dígitos, não todos iguais, e dígitos verificadores corretos.
  public static bool CpfValido(string cpf)
  {
    if (cpf.Length != 11 || !cpf.All(char.IsAsciiDigit)) return false;
    if (cpf.Distinct().Count() == 1) return false;

    return cpf[9] - '0' == DigitoVerificador(cpf, 9)
      && cpf[10] - '0' == DigitoVerificador(cpf, 10);
  }

  private static int DigitoVerificador(string cpf, int quantidade)
  {
    var soma = 0;
    for (var i = 0; i < quantidade; i++)
      soma += (cpf[i] - '0') * (quantidade + 1 - i);

    var resto = soma * 10 % 11;
    return resto == 10 ? 0 : resto;
  }

  // 12345678909 -> ***.456.789-**  (LGPD: mostra só o miolo na listagem)
  public static string? MascararCpf(string? cpf)
    => string.IsNullOrEmpty(cpf) || cpf.Length != 11
      ? null
      : $"***.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-**";
}
