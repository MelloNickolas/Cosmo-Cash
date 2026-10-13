namespace CosmoCash.Services.Interfaces;

public interface IHashService
{
  string HashFicticio { get; }
  string GerarHash(string senha);
  bool Verificar(string senha, string hash);
}
