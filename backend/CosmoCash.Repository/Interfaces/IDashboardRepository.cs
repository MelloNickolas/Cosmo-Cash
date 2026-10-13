using CosmoCash.Repository.Models;

namespace CosmoCash.Repository.Interfaces;

// Todos os métodos consideram só vendas Concluídas e um intervalo UTC [inicio, fim).
public interface IDashboardRepository
{
  Task<DashboardTotaisModel> ObterTotaisAsync(DateTime inicioUtc, DateTime fimUtcExclusivo);
  Task<List<DashboardSerieModel>> ListarSeriePorDiaAsync(DateTime inicioUtc, DateTime fimUtcExclusivo);
  Task<List<DashboardSerieModel>> ListarSeriePorMesAsync(DateTime inicioUtc, DateTime fimUtcExclusivo);
  Task<List<DashboardFormaPagamentoModel>> ListarPorFormaPagamentoAsync(DateTime inicioUtc, DateTime fimUtcExclusivo);

  Task<List<DashboardProdutoModel>> ListarTopProdutosAsync(
    DateTime inicioUtc, DateTime fimUtcExclusivo, int limite, OrdenacaoRanking ordenarPor);

  Task<List<DashboardMarcaModel>> ListarTopMarcasAsync(
    DateTime inicioUtc, DateTime fimUtcExclusivo, int limite, OrdenacaoRanking ordenarPor);
}
