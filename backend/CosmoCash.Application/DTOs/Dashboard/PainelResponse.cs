using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.DTOs.Dashboard;

public enum PeriodoDashboard
{
  Hoje = 1,
  Semana = 2,
  Mes = 3,
  Ano = 4,
  Personalizado = 5
}

public enum AgrupamentoSerie
{
  Dia = 1,
  Mes = 2
}

// Só vendas Concluídas entram em qualquer número do painel.
public class TotaisResponse
{
  public int Vendas { get; set; }

  // Soma das quantidades (perfumes vendidos).
  public int Itens { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
  public decimal Lucro { get; set; }
  public decimal TicketMedio { get; set; }
}

// Variação percentual do período atual contra o anterior. Nulo quando o anterior foi zero.
public class VariacaoResponse
{
  public decimal? Vendas { get; set; }
  public decimal? Faturamento { get; set; }
  public decimal? Custo { get; set; }
  public decimal? Lucro { get; set; }
  public decimal? TicketMedio { get; set; }
}

public class PeriodoResponse
{
  public PeriodoDashboard Tipo { get; set; }
  public DateOnly Inicio { get; set; }
  public DateOnly Fim { get; set; }

  // Fim usado nas contas: não passa de hoje (dias futuros não têm vendas).
  public DateOnly FimEfetivo { get; set; }
  public DateOnly AnteriorInicio { get; set; }
  public DateOnly AnteriorFim { get; set; }
}

public class ResumoResponse
{
  public TotaisResponse Atual { get; set; } = new();
  public TotaisResponse Anterior { get; set; } = new();
  public VariacaoResponse Variacao { get; set; } = new();
}

public class SeriePontoResponse
{
  // "2026-10-12" (agrupamento por dia) ou "2026-10" (por mês).
  public string Periodo { get; set; } = string.Empty;
  public int Vendas { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Custo { get; set; }
  public decimal Lucro { get; set; }
}

public class FormaPagamentoResumoResponse
{
  public FormaPagamento FormaPagamento { get; set; }
  public int Vendas { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Lucro { get; set; }
}

public class TopProdutoResponse
{
  public int ProdutoID { get; set; }
  public string Marca { get; set; } = string.Empty;
  public string Descricao { get; set; } = string.Empty;
  public int VolumeMl { get; set; }
  public string Variacao { get; set; } = string.Empty;
  public int Quantidade { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Lucro { get; set; }
}

public class TopMarcaResponse
{
  public string Marca { get; set; } = string.Empty;
  public int Quantidade { get; set; }
  public decimal Faturamento { get; set; }
  public decimal Lucro { get; set; }
}

public class PainelResponse
{
  public PeriodoResponse Periodo { get; set; } = new();
  public AgrupamentoSerie Agrupamento { get; set; }
  public ResumoResponse Resumo { get; set; } = new();

  // Todos os períodos do intervalo, inclusive os sem venda (zerados), para o gráfico não ter buracos.
  public List<SeriePontoResponse> Serie { get; set; } = [];
  public List<FormaPagamentoResumoResponse> PorFormaPagamento { get; set; } = [];
  public List<TopProdutoResponse> TopProdutos { get; set; } = [];
  public List<TopMarcaResponse> TopMarcas { get; set; } = [];
}
