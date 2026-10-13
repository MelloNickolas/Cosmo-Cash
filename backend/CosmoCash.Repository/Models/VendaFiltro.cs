using CosmoCash.Domain.Enums;

namespace CosmoCash.Repository.Models;

public class VendaFiltro
{
  public int Pagina { get; set; } = 1;
  public int TamanhoPagina { get; set; } = 10;

  // Intervalo em UTC: início inclusivo, fim EXclusivo.
  public DateTime? InicioUtc { get; set; }
  public DateTime? FimUtcExclusivo { get; set; }

  public StatusVenda? Status { get; set; }
  public FormaPagamento? FormaPagamento { get; set; }
  public int? ClienteID { get; set; }
  public int? UsuarioID { get; set; }
}
