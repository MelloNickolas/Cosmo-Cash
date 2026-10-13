using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Auth;

public class LoginResponse
{
  public string Token { get; set; } = string.Empty;
  public DateTime ExpiraEm { get; set; }
  public int UsuarioID { get; set; }
  public string Nome { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public TipoUsuario Tipo { get; set; }
}
