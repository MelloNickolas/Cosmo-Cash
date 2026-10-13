using CosmoCash.API.Extensions;
using CosmoCash.Application.DTOs.Auth;
using CosmoCash.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmoCash.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthApplication _authApplication;

  public AuthController(IAuthApplication authApplication) => _authApplication = authApplication;

  [AllowAnonymous]
  [EnableRateLimiting("login")]
  [HttpPost]
  [Route("Login")]
  public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    => Ok(await _authApplication.LoginAsync(request));

  // Qualquer usuário logado (AdminTotal ou Socio) troca a própria senha.
  [Authorize]
  [HttpPut]
  [Route("AlterarMinhaSenha")]
  public async Task<ActionResult> AlterarMinhaSenha([FromBody] AlterarMinhaSenhaRequest request)
  {
    await _authApplication.AlterarMinhaSenhaAsync(User.ObterUsuarioID(), request);
    return NoContent();
  }

  // Devolve quem está logado (o front usa para restaurar a sessão).
  [Authorize]
  [HttpGet]
  [Route("Me")]
  public ActionResult Me() => Ok(new
  {
    usuarioID = User.FindFirst("sub")?.Value,
    nome = User.FindFirst("name")?.Value,
    email = User.FindFirst("email")?.Value,
    tipo = User.FindFirst("role")?.Value
  });
}
