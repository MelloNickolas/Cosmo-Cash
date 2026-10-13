namespace CosmoCash.Domain.Entities;

public class ItemVenda
{
  public int ID { get; set; }
  public int Quantidade { get; set; } = 1;

  // Snapshot do momento da venda: mudar o preço depois não altera vendas antigas.
  public decimal PrecoVenda { get; set; }
  public decimal PrecoCusto { get; set; }

  public int VendaID { get; set; }
  public int ProdutoID { get; set; }

  public Venda Venda { get; set; } = null!;
  public Produto Produto { get; set; } = null!;
}
