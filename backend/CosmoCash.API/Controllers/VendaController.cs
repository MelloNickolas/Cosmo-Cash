using CosmoCash.API.Extensions;
using CosmoCash.Application.DTOs.Vendas;
using CosmoCash.Application.Interfaces;
using CosmoCash.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmoCash.API.Controllers;

// AdminTotal e Socio têm o mesmo acesso a vendas (inclusive custo e lucro).
[ApiController]
[Authorize]
[Route("[controller]")]
public class VendaController : ControllerBase
{
  private readonly IVendaApplication _vendaApplication;

  public VendaController(IVendaApplication vendaApplication) => _vendaApplication = vendaApplication;

  // dataInicio/dataFim no formato yyyy-MM-dd, dias de São Paulo, ambos inclusivos.
  [HttpGet]
  [Route("ListarVendas")]
  public async Task<ActionResult> ListarVendas(
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanhoPagina = 10,
    [FromQuery] DateOnly? dataInicio = null,
    [FromQuery] DateOnly? dataFim = null,
    [FromQuery] StatusVenda? status = null,
    [FromQuery] FormaPagamento? formaPagamento = null,
    [FromQuery] int? clienteID = null,
    [FromQuery] int? usuarioID = null)
    => Ok(await _vendaApplication.ListarAsync(
      pagina, tamanhoPagina, dataInicio, dataFim, status, formaPagamento, clienteID, usuarioID));

  [HttpGet]
  [Route("ObterVenda/{id:int}")]
  public async Task<ActionResult> ObterVenda(int id)
    => Ok(await _vendaApplication.ObterPorIdAsync(id));

  [HttpPost]
  [Route("CriarVenda")]
  public async Task<ActionResult> CriarVenda([FromBody] VendaRequest request)
  {
    var criada = await _vendaApplication.CriarAsync(request, User.ObterUsuarioID());
    return CreatedAtAction(nameof(ObterVenda), new { id = criada.ID }, criada);
  }

  [HttpPut]
  [Route("AtualizarVenda/{id:int}")]
  public async Task<ActionResult> AtualizarVenda(int id, [FromBody] VendaRequest request)
    => Ok(await _vendaApplication.AtualizarAsync(id, request));

  // Soft delete: a venda fica no histórico, marcada como Cancelada.
  [HttpDelete]
  [Route("CancelarVenda/{id:int}")]
  public async Task<ActionResult> CancelarVenda(int id)
  {
    await _vendaApplication.CancelarAsync(id);
    return NoContent();
  }

  [HttpPatch]
  [Route("ReativarVenda/{id:int}")]
  public async Task<ActionResult> ReativarVenda(int id)
    => Ok(await _vendaApplication.ReativarAsync(id));
}
