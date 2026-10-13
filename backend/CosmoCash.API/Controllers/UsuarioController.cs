using CosmoCash.API.Extensions;
using CosmoCash.Application.DTOs.Usuarios;
using CosmoCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmoCash.API.Controllers;

// Gerenciar usuários é exclusivo do AdminTotal (o Socio não tem acesso a nada aqui).
[ApiController]
[Authorize(Roles = "AdminTotal")]
[Route("[controller]")]
public class UsuarioController : ControllerBase
{
  private readonly IUsuarioApplication _usuarioApplication;

  public UsuarioController(IUsuarioApplication usuarioApplication) => _usuarioApplication = usuarioApplication;

  [HttpGet]
  [Route("ListarUsuarios")]
  public async Task<ActionResult> ListarUsuarios(
    [FromQuery] int pagina = 1,
    [FromQuery] int tamanhoPagina = 10,
    [FromQuery] string? busca = null,
    [FromQuery] bool? ativo = null)
    => Ok(await _usuarioApplication.ListarAsync(pagina, tamanhoPagina, busca, ativo));

  [HttpGet]
  [Route("ObterUsuario/{id:int}")]
  public async Task<ActionResult> ObterUsuario(int id)
    => Ok(await _usuarioApplication.ObterPorIdAsync(id));

  [HttpPost]
  [Route("CriarUsuario")]
  public async Task<ActionResult> CriarUsuario([FromBody] UsuarioRequest request)
  {
    var criado = await _usuarioApplication.CriarAsync(request);
    return CreatedAtAction(nameof(ObterUsuario), new { id = criado.ID }, criado);
  }

  [HttpPut]
  [Route("AtualizarUsuario/{id:int}")]
  public async Task<ActionResult> AtualizarUsuario(int id, [FromBody] UsuarioAtualizarRequest request)
    => Ok(await _usuarioApplication.AtualizarAsync(id, request));

  [HttpPut]
  [Route("AlterarSenha/{id:int}")]
  public async Task<ActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaRequest request)
  {
    await _usuarioApplication.AlterarSenhaAsync(id, request);
    return NoContent();
  }

  // Soft delete: o usuário continua no banco (as vendas dele não se perdem).
  [HttpDelete]
  [Route("DesativarUsuario/{id:int}")]
  public async Task<ActionResult> DesativarUsuario(int id)
  {
    await _usuarioApplication.DesativarAsync(id, User.ObterUsuarioID());
    return NoContent();
  }

  [HttpPatch]
  [Route("ReativarUsuario/{id:int}")]
  public async Task<ActionResult> ReativarUsuario(int id)
    => Ok(await _usuarioApplication.ReativarAsync(id));
}
