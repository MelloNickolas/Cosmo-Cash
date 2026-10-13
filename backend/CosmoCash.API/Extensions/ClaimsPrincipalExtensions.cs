using System.Security.Claims;
using CosmoCash.Application.Exceptions;

namespace CosmoCash.API.Extensions;

public static class ClaimsPrincipalExtensions
{
  // ID do usuário logado, a partir da claim "sub" do JWT.
  public static int ObterUsuarioID(this ClaimsPrincipal usuario)
  {
    var valor = usuario.FindFirst("sub")?.Value;
    if (!int.TryParse(valor, out var id))
      throw new UnauthorizedException("Sessão inválida.");
    return id;
  }
}
