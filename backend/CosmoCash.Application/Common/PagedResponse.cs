namespace CosmoCash.Application.Common;

public class PagedResponse<T>
{
  public IEnumerable<T> Itens { get; set; } = [];
  public int Pagina { get; set; }
  public int TamanhoPagina { get; set; }
  public int Total { get; set; }
  public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
}
