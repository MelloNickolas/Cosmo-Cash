using CosmoCash.Domain.Entities;
using CosmoCash.Domain.Enums;
using CosmoCash.Repository.Context;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmoCash.Repository.Repositories;

public class DashboardRepository : BaseRepository, IDashboardRepository
{
  // O banco guarda UTC; o "dia" do dashboard é o dia de São Paulo. O Npgsql traduz
  // TimeZoneInfo.ConvertTimeBySystemTimeZoneId para AT TIME ZONE, então a conversão roda no Postgres.
  private const string FusoSaoPaulo = "America/Sao_Paulo";

  public DashboardRepository(CosmoCashDbContext context) : base(context) { }

  private IQueryable<Venda> VendasConcluidas(DateTime inicioUtc, DateTime fimUtcExclusivo)
    => _context.Vendas.AsNoTracking().Where(venda =>
      venda.Status == StatusVenda.Concluida
      && venda.DataVenda >= inicioUtc
      && venda.DataVenda < fimUtcExclusivo);

  private IQueryable<ItemVenda> ItensConcluidos(DateTime inicioUtc, DateTime fimUtcExclusivo)
    => _context.ItensVenda.AsNoTracking().Where(item =>
      item.Venda.Status == StatusVenda.Concluida
      && item.Venda.DataVenda >= inicioUtc
      && item.Venda.DataVenda < fimUtcExclusivo);

  public async Task<DashboardTotaisModel> ObterTotaisAsync(DateTime inicioUtc, DateTime fimUtcExclusivo)
  {
    var vendas = await VendasConcluidas(inicioUtc, fimUtcExclusivo).CountAsync();

    var somas = await ItensConcluidos(inicioUtc, fimUtcExclusivo)
      .GroupBy(_ => 1)
      .Select(grupo => new
      {
        Itens = grupo.Sum(item => item.Quantidade),
        Faturamento = grupo.Sum(item => item.PrecoVenda * item.Quantidade),
        Custo = grupo.Sum(item => item.PrecoCusto * item.Quantidade)
      })
      .FirstOrDefaultAsync();

    return new DashboardTotaisModel
    {
      Vendas = vendas,
      Itens = somas?.Itens ?? 0,
      Faturamento = somas?.Faturamento ?? 0,
      Custo = somas?.Custo ?? 0
    };
  }

  public async Task<List<DashboardSerieModel>> ListarSeriePorDiaAsync(DateTime inicioUtc, DateTime fimUtcExclusivo)
    => await ItensConcluidos(inicioUtc, fimUtcExclusivo)
      .GroupBy(item => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(item.Venda.DataVenda, FusoSaoPaulo).Date)
      .Select(grupo => new DashboardSerieModel
      {
        Dia = grupo.Key,
        Vendas = grupo.Select(item => item.VendaID).Distinct().Count(),
        Faturamento = grupo.Sum(item => item.PrecoVenda * item.Quantidade),
        Custo = grupo.Sum(item => item.PrecoCusto * item.Quantidade)
      })
      .OrderBy(ponto => ponto.Dia)
      .ToListAsync();

  public async Task<List<DashboardSerieModel>> ListarSeriePorMesAsync(DateTime inicioUtc, DateTime fimUtcExclusivo)
  {
    var linhas = await ItensConcluidos(inicioUtc, fimUtcExclusivo)
      .GroupBy(item => new
      {
        Ano = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(item.Venda.DataVenda, FusoSaoPaulo).Year,
        Mes = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(item.Venda.DataVenda, FusoSaoPaulo).Month
      })
      .Select(grupo => new
      {
        grupo.Key.Ano,
        grupo.Key.Mes,
        Vendas = grupo.Select(item => item.VendaID).Distinct().Count(),
        Faturamento = grupo.Sum(item => item.PrecoVenda * item.Quantidade),
        Custo = grupo.Sum(item => item.PrecoCusto * item.Quantidade)
      })
      .OrderBy(linha => linha.Ano).ThenBy(linha => linha.Mes)
      .ToListAsync();

    return linhas.Select(linha => new DashboardSerieModel
    {
      Dia = new DateTime(linha.Ano, linha.Mes, 1),
      Vendas = linha.Vendas,
      Faturamento = linha.Faturamento,
      Custo = linha.Custo
    }).ToList();
  }

  public async Task<List<DashboardFormaPagamentoModel>> ListarPorFormaPagamentoAsync(
    DateTime inicioUtc, DateTime fimUtcExclusivo)
    => await ItensConcluidos(inicioUtc, fimUtcExclusivo)
      .GroupBy(item => item.Venda.FormaPagamento)
      .Select(grupo => new DashboardFormaPagamentoModel
      {
        FormaPagamento = grupo.Key,
        Vendas = grupo.Select(item => item.VendaID).Distinct().Count(),
        Faturamento = grupo.Sum(item => item.PrecoVenda * item.Quantidade),
        Custo = grupo.Sum(item => item.PrecoCusto * item.Quantidade)
      })
      .OrderByDescending(linha => linha.Faturamento)
      .ToListAsync();

  public async Task<List<DashboardProdutoModel>> ListarTopProdutosAsync(
    DateTime inicioUtc, DateTime fimUtcExclusivo, int limite, OrdenacaoRanking ordenarPor)
  {
    var agrupado = ItensConcluidos(inicioUtc, fimUtcExclusivo)
      .GroupBy(item => new
      {
        item.ProdutoID,
        item.Produto.Marca,
        item.Produto.Descricao,
        item.Produto.VolumeMl,
        item.Produto.Variacao
      })
      .Select(grupo => new DashboardProdutoModel
      {
        ProdutoID = grupo.Key.ProdutoID,
        Marca = grupo.Key.Marca,
        Descricao = grupo.Key.Descricao,
        VolumeMl = grupo.Key.VolumeMl,
        Variacao = grupo.Key.Variacao,
        Quantidade = grupo.Sum(item => item.Quantidade),
        Faturamento = grupo.Sum(item => item.PrecoVenda * item.Quantidade),
        Custo = grupo.Sum(item => item.PrecoCusto * item.Quantidade)
      });

    var ordenado = ordenarPor switch
    {
      OrdenacaoRanking.Faturamento => agrupado.OrderByDescending(p => p.Faturamento),
      OrdenacaoRanking.Lucro => agrupado.OrderByDescending(p => p.Faturamento - p.Custo),
      _ => agrupado.OrderByDescending(p => p.Quantidade)
    };

    return await ordenado.ThenByDescending(p => p.Faturamento).Take(limite).ToListAsync();
  }

  public async Task<List<DashboardMarcaModel>> ListarTopMarcasAsync(
    DateTime inicioUtc, DateTime fimUtcExclusivo, int limite, OrdenacaoRanking ordenarPor)
  {
    var agrupado = ItensConcluidos(inicioUtc, fimUtcExclusivo)
      .GroupBy(item => item.Produto.Marca)
      .Select(grupo => new DashboardMarcaModel
      {
        Marca = grupo.Key,
        Quantidade = grupo.Sum(item => item.Quantidade),
        Faturamento = grupo.Sum(item => item.PrecoVenda * item.Quantidade),
        Custo = grupo.Sum(item => item.PrecoCusto * item.Quantidade)
      });

    var ordenado = ordenarPor switch
    {
      OrdenacaoRanking.Faturamento => agrupado.OrderByDescending(m => m.Faturamento),
      OrdenacaoRanking.Lucro => agrupado.OrderByDescending(m => m.Faturamento - m.Custo),
      _ => agrupado.OrderByDescending(m => m.Quantidade)
    };

    return await ordenado.ThenByDescending(m => m.Faturamento).Take(limite).ToListAsync();
  }
}
