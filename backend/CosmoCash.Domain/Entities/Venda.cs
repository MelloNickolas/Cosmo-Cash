using CosmoCash.Domain.Enums;

namespace CosmoCash.Domain.Entities;

public class Venda
{
  public int ID { get; set; }
  public DateTime DataVenda { get; set; } = DateTime.UtcNow;
  public FormaPagamento FormaPagamento { get; set; }
  public StatusVenda Status { get; set; } = StatusVenda.Concluida;
  public DateTime? CanceladaEm { get; set; }

  public int? ClienteID { get; set; }
  public int UsuarioID { get; set; }

  public Cliente? Cliente { get; set; }
  public Usuario Usuario { get; set; } = null!;
  public ICollection<ItemVenda> Itens { get; set; } = [];
}
