using CosmoCash.Domain.Enums;

namespace CosmoCash.Repository.Models;

public enum OrdenacaoRanking
{
  Quantidade = 1,
  Faturamento = 2,
  Lucro = 3
}

public class DashboardTotaisModel
{
  public int Vendas { get; set; }
  public int Itens { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
}

// Um ponto da série: o dia (ou o 1º dia do mês, no agrupamento mensal), no calendário de São Paulo.
public class DashboardSerieModel
{
  public DateTime Dia { get; set; }
  public int Vendas { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
}

public class DashboardFormaPagamentoModel
{
  public FormaPagamento FormaPagamento { get; set; }
  public int Vendas { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
}

public class DashboardProdutoModel
{
  public int ProdutoID { get; set; }
  public string Marca { get; set; } = string.Empty;
  public string Descricao { get; set; } = string.Empty;
  public int VolumeMl { get; set; }
  public string Variacao { get; set; } = string.Empty;
  public int Quantidade { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
}

public class DashboardMarcaModel
{
  public string Marca { get; set; } = string.Empty;
  public int Quantidade { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
}
