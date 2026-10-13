using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Usuarios;

// Edição: não mexe na senha (há endpoints próprios para isso).
public class UsuarioAtualizarRequest
{
  public string Nome { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public TipoUsuario Tipo { get; set; }
}
