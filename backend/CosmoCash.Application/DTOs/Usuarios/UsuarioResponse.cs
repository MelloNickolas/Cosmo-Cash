using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Usuarios;

// Nunca expõe SenhaHash.
public class UsuarioResponse
{
  public int ID { get; set; }
  public string Nome { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public TipoUsuario Tipo { get; set; }
  public bool Ativo { get; set; }
  public DateTime CriadoEm { get; set; }
}
