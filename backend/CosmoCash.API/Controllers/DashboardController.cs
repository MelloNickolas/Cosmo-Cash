using CosmoCash.Application.DTOs.Dashboard;
using CosmoCash.Application.Interfaces;
using CosmoCash.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmoCash.API.Controllers;

// AdminTotal e Socio veem o mesmo painel (inclusive custo e lucro).
[ApiController]
[Authorize]
[Route("[controller]")]
public class DashboardController : ControllerBase
{
  private readonly IDashboardApplication _dashboardApplication;

  public DashboardController(IDashboardApplication dashboardApplication)
    => _dashboardApplication = dashboardApplication;

  // Uma chamada devolve o painel inteiro (menos idas e voltas no celular).
  //  - periodo: Hoje | Semana (seg–dom) | Mes | Ano | Personalizado (padrão: Mes)
  //  - dataInicio/dataFim (yyyy-MM-dd): para Personalizado
  //  - agrupamento: Dia | Mes (padrão automático conforme o tamanho do período)
  //  - ordenarPor: Quantidade | Faturamento | Lucro (rankings de produtos e marcas)
  [HttpGet]
  [Route("Painel")]
  public async Task<ActionResult<PainelResponse>> Painel(
    [FromQuery] PeriodoDashboard? periodo = null,
    [FromQuery] DateOnly? dataInicio = null,
    [FromQuery] DateOnly? dataFim = null,
    [FromQuery] AgrupamentoSerie? agrupamento = null,
    [FromQuery] int limite = 5,
    [FromQuery] OrdenacaoRanking ordenarPor = OrdenacaoRanking.Quantidade)
    => Ok(await _dashboardApplication.ObterPainelAsync(periodo, dataInicio, dataFim, agrupamento, limite, ordenarPor));
}
