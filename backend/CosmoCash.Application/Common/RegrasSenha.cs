using CosmoCash.Application.Exceptions;

namespace CosmoCash.Application.Common;

public static class RegrasSenha
{
  public const int Minimo = 8;

  // O BCrypt só considera os primeiros 72 bytes; acima disso o resto seria ignorado em silêncio.
  public const int Maximo = 72;

  public static void Validar(string? senha)
  {
    if (string.IsNullOrEmpty(senha))
      throw new ValidationException("A senha não pode ser vazia.");

    if (senha.Length < Minimo)
      throw new ValidationException($"A senha deve ter pelo menos {Minimo} caracteres.");

    if (System.Text.Encoding.UTF8.GetByteCount(senha) > Maximo)
      throw new ValidationException($"A senha é longa demais (máximo {Maximo} bytes).");
  }
}
