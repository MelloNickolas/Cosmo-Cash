using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Dashboard;
using CosmoCash.Application.Exceptions;
using CosmoCash.Application.Interfaces;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Repository.Models;

namespace CosmoCash.Application.Applications;

public class DashboardApplication : IDashboardApplication
{
  private const int MaximoDiasPersonalizado = 1830;   // ~5 anos
  private const int MaximoDiasAgrupadoPorDia = 400;
  private const int LimiteMaximoRanking = 50;

  private readonly IDashboardRepository _dashboardRepository;

  public DashboardApplication(IDashboardRepository dashboardRepository)
    => _dashboardRepository = dashboardRepository;

  public async Task<PainelResponse> ObterPainelAsync(
    PeriodoDashboard? periodo, DateOnly? dataInicio, DateOnly? dataFim,
    AgrupamentoSerie? agrupamento, int limite, OrdenacaoRanking ordenarPor)
  {
    limite = Math.Clamp(limite, 1, LimiteMaximoRanking);

    var janela = ResolverPeriodo(periodo, dataInicio, dataFim);
    var agrup = ResolverAgrupamento(janela, agrupamento);

    var inicioUtc = FusoHorario.InicioDoDiaUtc(janela.Inicio);
    var fimUtc = FusoHorario.InicioDoDiaUtc(janela.FimEfetivo.AddDays(1));
    var anteriorInicioUtc = FusoHorario.InicioDoDiaUtc(janela.AnteriorInicio);
    var anteriorFimUtc = FusoHorario.InicioDoDiaUtc(janela.AnteriorFim.AddDays(1));

    // Em sequência (e não em paralelo): um DbContext não aceita duas consultas ao mesmo tempo.
    var atual = await _dashboardRepository.ObterTotaisAsync(inicioUtc, fimUtc);
    var anterior = await _dashboardRepository.ObterTotaisAsync(anteriorInicioUtc, anteriorFimUtc);
    var serieBruta = agrup == AgrupamentoSerie.Dia
      ? await _dashboardRepository.ListarSeriePorDiaAsync(inicioUtc, fimUtc)
      : await _dashboardRepository.ListarSeriePorMesAsync(inicioUtc, fimUtc);
    var formas = await _dashboardRepository.ListarPorFormaPagamentoAsync(inicioUtc, fimUtc);
    var produtos = await _dashboardRepository.ListarTopProdutosAsync(inicioUtc, fimUtc, limite, ordenarPor);
    var marcas = await _dashboardRepository.ListarTopMarcasAsync(inicioUtc, fimUtc, limite, ordenarPor);

    var totaisAtual = MapearTotais(atual);
    var totaisAnterior = MapearTotais(anterior);

    return new PainelResponse
    {
      Periodo = janela,
      Agrupamento = agrup,
      Resumo = new ResumoResponse
      {
        Atual = totaisAtual,
        Anterior = totaisAnterior,
        Variacao = new VariacaoResponse
        {
          Vendas = Variacao(totaisAtual.Vendas, totaisAnterior.Vendas),
          Faturamento = Variacao(totaisAtual.Faturamento, totaisAnterior.Faturamento),
          Custo = Variacao(totaisAtual.Custo, totaisAnterior.Custo),
          Lucro = Variacao(totaisAtual.Lucro, totaisAnterior.Lucro),
          TicketMedio = Variacao(totaisAtual.TicketMedio, totaisAnterior.TicketMedio)
        }
      },
      Serie = PreencherSerie(janela, agrup, serieBruta),
      PorFormaPagamento = formas.Select(forma => new FormaPagamentoResumoResponse
      {
        FormaPagamento = forma.FormaPagamento,
        Vendas = forma.Vendas,
        Faturamento = forma.Faturamento,
        Lucro = forma.Faturamento - forma.Custo
      }).ToList(),
      TopProdutos = produtos.Select(produto => new TopProdutoResponse
      {
        ProdutoID = produto.ProdutoID,
        Marca = produto.Marca,
        Descricao = produto.Descricao,
        VolumeMl = produto.VolumeMl,
        Variacao = produto.Variacao,
        Quantidade = produto.Quantidade,
        Faturamento = produto.Faturamento,
        Lucro = produto.Faturamento - produto.Custo
      }).ToList(),
      TopMarcas = marcas.Select(marca => new TopMarcaResponse
      {
        Marca = marca.Marca,
        Quantidade = marca.Quantidade,
        Faturamento = marca.Faturamento,
        Lucro = marca.Faturamento - marca.Custo
      }).ToList()
    };
  }

  #region Úteis
  // Semana: segunda a domingo. Mês e ano: calendário. Tudo no calendário de São Paulo.
  private static PeriodoResponse ResolverPeriodo(PeriodoDashboard? periodo, DateOnly? dataInicio, DateOnly? dataFim)
  {
    var hoje = FusoHorario.HojeEmSaoPaulo();
    var tipo = periodo ?? (dataInicio.HasValue || dataFim.HasValue ? PeriodoDashboard.Personalizado : PeriodoDashboard.Mes);

    DateOnly inicio, fim;

    switch (tipo)
    {
      case PeriodoDashboard.Hoje:
        inicio = fim = hoje;
        break;

      case PeriodoDashboard.Semana:
        var diasDesdeSegunda = ((int)hoje.DayOfWeek + 6) % 7;
        inicio = hoje.AddDays(-diasDesdeSegunda);
        fim = inicio.AddDays(6);
        break;

      case PeriodoDashboard.Mes:
        inicio = new DateOnly(hoje.Year, hoje.Month, 1);
        fim = inicio.AddMonths(1).AddDays(-1);
        break;

      case PeriodoDashboard.Ano:
        inicio = new DateOnly(hoje.Year, 1, 1);
        fim = new DateOnly(hoje.Year, 12, 31);
        break;

      default:
        if (!dataInicio.HasValue || !dataFim.HasValue)
          throw new ValidationException("Informe a data inicial e a data final do período.");
        if (dataFim < dataInicio)
          throw new ValidationException("A data final não pode ser anterior à data inicial.");
        if (dataFim.Value.DayNumber - dataInicio.Value.DayNumber + 1 > MaximoDiasPersonalizado)
          throw new ValidationException("O período pode ter no máximo 5 anos.");

        inicio = dataInicio.Value;
        fim = dataFim.Value;
        break;
    }

    // Dias futuros não têm venda: as contas (e a comparação) param em hoje.
    var fimEfetivo = fim > hoje ? hoje : fim;
    if (fimEfetivo < inicio) fimEfetivo = inicio;

    // Período anterior, para comparar "igual com igual" e só até onde o atual chegou:
    //  Hoje -> ontem | Semana -> semana passada | Mês -> mês passado | Ano -> ano passado
    //  Personalizado -> mesma quantidade de dias, imediatamente antes do início.
    var dias = fimEfetivo.DayNumber - inicio.DayNumber + 1;

    var anteriorInicio = tipo switch
    {
      PeriodoDashboard.Hoje => inicio.AddDays(-1),
      PeriodoDashboard.Semana => inicio.AddDays(-7),
      PeriodoDashboard.Mes => inicio.AddMonths(-1),
      PeriodoDashboard.Ano => inicio.AddYears(-1),
      _ => inicio.AddDays(-dias)
    };

    var anteriorFim = anteriorInicio.AddDays(dias - 1);

    // Não deixa o anterior invadir o período atual (ex.: mês passado com menos dias).
    if (tipo != PeriodoDashboard.Personalizado && anteriorFim >= inicio)
      anteriorFim = inicio.AddDays(-1);

    return new PeriodoResponse
    {
      Tipo = tipo,
      Inicio = inicio,
      Fim = fim,
      FimEfetivo = fimEfetivo,
      AnteriorInicio = anteriorInicio,
      AnteriorFim = anteriorFim
    };
  }

  private static AgrupamentoSerie ResolverAgrupamento(PeriodoResponse janela, AgrupamentoSerie? pedido)
  {
    var dias = janela.FimEfetivo.DayNumber - janela.Inicio.DayNumber + 1;

    if (pedido.HasValue)
      return pedido == AgrupamentoSerie.Dia && dias > MaximoDiasAgrupadoPorDia ? AgrupamentoSerie.Mes : pedido.Value;

    return janela.Tipo == PeriodoDashboard.Ano || dias > 92 ? AgrupamentoSerie.Mes : AgrupamentoSerie.Dia;
  }

  private static List<SeriePontoResponse> PreencherSerie(
    PeriodoResponse janela, AgrupamentoSerie agrupamento, List<DashboardSerieModel> bruta)
  {
    var porChave = bruta.ToDictionary(ponto => ponto.Dia.Date);
    var serie = new List<SeriePontoResponse>();

    if (agrupamento == AgrupamentoSerie.Dia)
    {
      for (var dia = janela.Inicio; dia <= janela.FimEfetivo; dia = dia.AddDays(1))
      {
        porChave.TryGetValue(dia.ToDateTime(TimeOnly.MinValue), out var ponto);
        serie.Add(CriarPonto(dia.ToString("yyyy-MM-dd"), ponto));
      }
    }
    else
    {
      var mes = new DateOnly(janela.Inicio.Year, janela.Inicio.Month, 1);
      var ultimo = new DateOnly(janela.FimEfetivo.Year, janela.FimEfetivo.Month, 1);

      for (; mes <= ultimo; mes = mes.AddMonths(1))
      {
        porChave.TryGetValue(mes.ToDateTime(TimeOnly.MinValue), out var ponto);
        serie.Add(CriarPonto(mes.ToString("yyyy-MM"), ponto));
      }
    }

    return serie;
  }

  private static SeriePontoResponse CriarPonto(string periodo, DashboardSerieModel? ponto) => new()
  {
    Periodo = periodo,
    Vendas = ponto?.Vendas ?? 0,
    Faturamento = ponto?.Faturamento ?? 0,
    Custo = ponto?.Custo ?? 0,
    Lucro = (ponto?.Faturamento ?? 0) - (ponto?.Custo ?? 0)
  };

  private static TotaisResponse MapearTotais(DashboardTotaisModel modelo) => new()
  {
    Vendas = modelo.Vendas,
    Itens = modelo.Itens,
    Faturamento = modelo.Faturamento,
    Custo = modelo.Custo,
    Lucro = modelo.Faturamento - modelo.Custo,
    TicketMedio = modelo.Vendas == 0 ? 0 : Math.Round(modelo.Faturamento / modelo.Vendas, 2)
  };

  private static decimal? Variacao(decimal atual, decimal anterior)
    => anterior == 0 ? null : Math.Round((atual - anterior) / anterior * 100, 1);
  #endregion
}
