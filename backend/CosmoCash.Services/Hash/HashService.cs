using CosmoCash.Services.Interfaces;

namespace CosmoCash.Services.Hash;

public class HashService : IHashService
{
  // Hash real de uma senha aleatória, gerado uma vez por processo.
  // Serve para o login gastar o mesmo tempo mesmo quando o e-mail não existe.
  private static readonly string _hashFicticio = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

  public string HashFicticio => _hashFicticio;

  public string GerarHash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

  public bool Verificar(string senha, string hash)
  {
    try { return BCrypt.Net.BCrypt.Verify(senha, hash); }
    catch { return false; }
  }
}
