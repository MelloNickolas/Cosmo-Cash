using CosmoCash.Domain.Enums;

namespace CosmoCash.Domain.Entities;

public class Usuario
{
  public int ID { get; set; }
  public string Nome { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string SenhaHash { get; set; } = string.Empty;
  public TipoUsuario Tipo { get; set; } = TipoUsuario.Socio;
  public bool Ativo { get; set; } = true;
  public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
