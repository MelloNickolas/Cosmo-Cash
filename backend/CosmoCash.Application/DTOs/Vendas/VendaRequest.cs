using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Vendas;

public class VendaItemRequest
{
  public int ProdutoID { get; set; }
  public int Quantidade { get; set; } = 1;

  // Preço unitário cobrado do cliente (deve ser maior que zero).
  public decimal PrecoVenda { get; set; }

  // Custo unitário. Pode ser zero.
  public decimal PrecoCusto { get; set; }
}

public class VendaRequest
{
  // Opcional: sem data, vale "agora". Não aceita data futura.
  public DateTime? DataVenda { get; set; }

  public FormaPagamento FormaPagamento { get; set; }

  // Opcional: venda sem cliente cadastrado.
  public int? ClienteID { get; set; }

  public List<VendaItemRequest> Itens { get; set; } = [];
}
