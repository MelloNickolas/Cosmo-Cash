using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Usuarios;

// Criação: inclui a senha inicial.
public class UsuarioRequest
{
  public string Nome { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Senha { get; set; } = string.Empty;
  public TipoUsuario Tipo { get; set; } = TipoUsuario.Socio;
}
