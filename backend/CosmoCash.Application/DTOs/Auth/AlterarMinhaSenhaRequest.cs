namespace CosmoCash.Application.DTOs.Auth;

// O próprio usuário troca a sua senha, informando a atual.
public class AlterarMinhaSenhaRequest
{
  public string SenhaAtual { get; set; } = string.Empty;
  public string NovaSenha { get; set; } = string.Empty;
}
