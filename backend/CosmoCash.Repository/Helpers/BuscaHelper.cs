namespace CosmoCash.Repository.Helpers;

public static class BuscaHelper
{
  // Caractere de escape do ILIKE. Precisa ser passado explicitamente em EF.Functions.ILike:
  // sem ele o Npgsql gera ESCAPE '' (nenhum) e o escape abaixo não funciona.
  public const string Escape = "\\";

  // Monta o padrão "%termo%" tratando %, _ e \ do que o usuário digitou como texto literal.
  public static string PadraoContem(string termo)
  {
    var seguro = termo.Trim()
      .Replace("\\", "\\\\")
      .Replace("%", "\\%")
      .Replace("_", "\\_");

    return $"%{seguro}%";
  }
}
