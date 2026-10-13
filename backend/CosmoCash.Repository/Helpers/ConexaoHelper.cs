using Npgsql;

namespace CosmoCash.Repository.Helpers;

public static class ConexaoHelper
{
  // O Neon entrega a conexão em URI (postgresql://user:senha@host/db?sslmode=require),
  // formato que o Npgsql não interpreta. Aqui convertemos para key-value.
  public static string Normalizar(string? conexao)
  {
    if (string.IsNullOrWhiteSpace(conexao))
      throw new Exception("CONNECTION_STRING não configurada.");

    var valor = conexao.Trim();
    var ehUri = valor.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
      || valor.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

    if (!ehUri) return valor;

    var uri = new Uri(valor);
    var usuarioSenha = uri.UserInfo.Split(':', 2);

    // O builder cuida do escape de caracteres especiais (ex.: ';' na senha).
    var builder = new NpgsqlConnectionStringBuilder
    {
      Host = uri.Host,
      Port = uri.Port > 0 ? uri.Port : 5432,
      Database = uri.AbsolutePath.TrimStart('/'),
      Username = Uri.UnescapeDataString(usuarioSenha[0]),
      Password = usuarioSenha.Length > 1 ? Uri.UnescapeDataString(usuarioSenha[1]) : string.Empty,
      SslMode = SslMode.Require
    };

    return builder.ConnectionString;
  }
}
