using CosmoCash.Application.DTOs.Produtos;
using CosmoCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmoCash.API.Controllers;

// AdminTotal e Socio têm o mesmo acesso a produtos.
[ApiController]
[Authorize]
[Route("[controller]")]
public class ProdutoController : ControllerBase
{
  private readonly IProdutoApplication _produtoApplication;

  public ProdutoController(IProdutoApplication produtoApplication) => _produtoApplication = produtoApplication;

  [HttpGet]
  [Route("ListarProdutos")]
  public async Task<ActionResult> ListarProdutos(
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanhoPagina = 10,
    [FromQuery] string? busca = null,
    [FromQuery] bool? ativo = null,
    [FromQuery] string? marca = null,
    [FromQuery] string? variacao = null)
    => Ok(await _produtoApplication.ListarAsync(pagina, tamanhoPagina, busca, ativo, marca, variacao));

  [HttpGet]
  [Route("ObterProduto/{id:int}")]
  public async Task<ActionResult> ObterProduto(int id)
    => Ok(await _produtoApplication.ObterPorIdAsync(id));

  // Sugestões para o formulário e para os filtros.
  [HttpGet]
  [Route("ListarMarcas")]
  public async Task<ActionResult> ListarMarcas()
    => Ok(await _produtoApplication.ListarMarcasAsync());

  [HttpGet]
  [Route("ListarVariacoes")]
  public async Task<ActionResult> ListarVariacoes()
    => Ok(await _produtoApplication.ListarVariacoesAsync());

  [HttpPost]
  [Route("CriarProduto")]
  public async Task<ActionResult> CriarProduto([FromBody] ProdutoRequest request)
  {
    var criado = await _produtoApplication.CriarAsync(request);
    return CreatedAtAction(nameof(ObterProduto), new { id = criado.ID }, criado);
  }

  [HttpPut]
  [Route("AtualizarProduto/{id:int}")]
  public async Task<ActionResult> AtualizarProduto(int id, [FromBody] ProdutoRequest request)
    => Ok(await _produtoApplication.AtualizarAsync(id, request));

  // Soft delete.
  [HttpDelete]
  [Route("DesativarProduto/{id:int}")]
  public async Task<ActionResult> DesativarProduto(int id)
  {
    await _produtoApplication.DesativarAsync(id);
    return NoContent();
  }

  [HttpPatch]
  [Route("ReativarProduto/{id:int}")]
  public async Task<ActionResult> ReativarProduto(int id)
    => Ok(await _produtoApplication.ReativarAsync(id));
}
