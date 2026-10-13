namespace CosmoCash.Domain.Entities;

public class Produto
{
  public int ID { get; set; }
  public string Marca { get; set; } = string.Empty;
  public string Descricao { get; set; } = string.Empty;
  public int VolumeMl { get; set; }
  public string Variacao { get; set; } = string.Empty;
  public bool Ativo { get; set; } = true;
  public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
