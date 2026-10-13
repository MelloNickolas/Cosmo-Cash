namespace CosmoCash.Application.DTOs.Usuarios;

// O AdminTotal redefine a senha de outro usuário (sem precisar da atual).
public class AlterarSenhaRequest
{
  public string NovaSenha { get; set; } = string.Empty;
}
