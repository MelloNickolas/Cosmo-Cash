namespace CosmoCash.Application.DTOs.Produtos;

// Sem preço: o preço de venda e o de custo são informados a cada venda, por item.
public class ProdutoRequest
{
  public string Marca { get; set; } = string.Empty;
  public string Descricao { get; set; } = string.Empty;
  public int VolumeMl { get; set; }
  public string Variacao { get; set; } = string.Empty;
}
