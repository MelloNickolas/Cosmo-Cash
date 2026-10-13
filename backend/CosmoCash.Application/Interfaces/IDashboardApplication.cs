using CosmoCash.Application.DTOs.Dashboard;
using CosmoCash.Repository.Models;

namespace CosmoCash.Application.Interfaces;

public interface IDashboardApplication
{
  Task<PainelResponse> ObterPainelAsync(
    PeriodoDashboard? periodo, DateOnly? dataInicio, DateOnly? dataFim,
    AgrupamentoSerie? agrupamento, int limite, OrdenacaoRanking ordenarPor);
}
