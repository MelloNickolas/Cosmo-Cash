using CosmoCash.Application.DTOs.Clientes;
using CosmoCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmoCash.API.Controllers;

// AdminTotal e Socio têm o mesmo acesso a clientes.
[ApiController]
[Authorize]
[Route("[controller]")]
public class ClienteController : ControllerBase
{
  private readonly IClienteApplication _clienteApplication;

  public ClienteController(IClienteApplication clienteApplication) => _clienteApplication = clienteApplication;

  // Listagem devolve o CPF mascarado; o completo só em ObterCliente.
  [HttpGet]
  [Route("ListarClientes")]
  public async Task<ActionResult> ListarClientes(
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanhoPagina = 10,
    [FromQuery] string? busca = null,
    [FromQuery] bool? ativo = null)
    => Ok(await _clienteApplication.ListarAsync(pagina, tamanhoPagina, busca, ativo));

  [HttpGet]
  [Route("ObterCliente/{id:int}")]
  public async Task<ActionResult> ObterCliente(int id)
    => Ok(await _clienteApplication.ObterPorIdAsync(id));

  [HttpPost]
  [Route("CriarCliente")]
  public async Task<ActionResult> CriarCliente([FromBody] ClienteRequest request)
  {
    var criado = await _clienteApplication.CriarAsync(request);
    return CreatedAtAction(nameof(ObterCliente), new { id = criado.ID }, criado);
  }

  [HttpPut]
  [Route("AtualizarCliente/{id:int}")]
  public async Task<ActionResult> AtualizarCliente(int id, [FromBody] ClienteRequest request)
    => Ok(await _clienteApplication.AtualizarAsync(id, request));

  // Soft delete.
  [HttpDelete]
  [Route("DesativarCliente/{id:int}")]
  public async Task<ActionResult> DesativarCliente(int id)
  {
    await _clienteApplication.DesativarAsync(id);
    return NoContent();
  }

  [HttpPatch]
  [Route("ReativarCliente/{id:int}")]
  public async Task<ActionResult> ReativarCliente(int id)
    => Ok(await _clienteApplication.ReativarAsync(id));
}
