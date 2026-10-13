namespace CosmoCash.Domain.Entities;

public class Cliente
{
  public int ID { get; set; }
  public string Nome { get; set; } = string.Empty;
  public string? Cpf { get; set; }
  public string? Telefone { get; set; }
  public string? Cep { get; set; }
  public string? Endereco { get; set; }
  public bool Ativo { get; set; } = true;
  public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
