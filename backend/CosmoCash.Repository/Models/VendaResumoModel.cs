using CosmoCash.Domain.Enums;

namespace CosmoCash.Repository.Models;

// Linha da listagem de vendas, com os totais já calculados no banco.
public class VendaResumoModel
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
}
