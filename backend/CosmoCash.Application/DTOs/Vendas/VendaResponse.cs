using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Vendas;

public class VendaItemResponse
{
  public int ID { get; set; }
  public int ProdutoID { get; set; }
  public string Marca { get; set; } = string.Empty;
  public string Descricao { get; set; } = string.Empty;
  public int VolumeMl { get; set; }
  public string Variacao { get; set; } = string.Empty;
  public int Quantidade { get; set; }
  public decimal PrecoVenda { get; set; }
  public decimal PrecoCusto { get; set; }

  // PrecoVenda × Quantidade
  public decimal Subtotal { get; set; }

  // (PrecoVenda − PrecoCusto) × Quantidade
  public decimal Lucro { get; set; }
}

public class VendaPessoaResponse
{
  public int ID { get; set; }
  public string Nome { get; set; } = string.Empty;
}

public class VendaResponse
{
  public int ID { get; set; }
  public DateTime DataVenda { get; set; }
  public FormaPagamento FormaPagamento { get; set; }
  public StatusVenda Status { get; set; }
  public DateTime? CanceladaEm { get; set; }
  public VendaPessoaResponse? Cliente { get; set; }
  public VendaPessoaResponse Usuario { get; set; } = new();
  public List<VendaItemResponse> Itens { get; set; } = [];
  public decimal ValorTotal { get; set; }
  public decimal CustoTotal { get; set; }
  public decimal Lucro { get; set; }
}

// Linha da listagem (sem os itens).
public class VendaResumoResponse
{
  public int ID { get; set; }
  public DateTime DataVenda { get; set; }
  public FormaPagamento FormaPagamento { get; set; }
  public StatusVenda Status { get; set; }
  public int? ClienteID { get; set; }
  public string? ClienteNome { get; set; }
  public int UsuarioID { get; set; }
  public string UsuarioNome { get; set; } = string.Empty;
  public int QuantidadeItens { get; set; }
  public decimal ValorTotal { get; set; }
  public decimal CustoTotal { get; set; }
  public decimal Lucro { get; set; }
}
