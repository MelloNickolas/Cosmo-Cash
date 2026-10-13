using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CosmoCash.Domain.Entities;
using CosmoCash.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CosmoCash.Services.Token;

public class TokenService : ITokenService
{
  private readonly JwtOptions _options;

  public TokenService(JwtOptions options) => _options = options;

  public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
  {
    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, usuario.ID.ToString()),
      new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
      new Claim("name", usuario.Nome),
      // O Tipo vira Role: [Authorize(Roles = "AdminTotal")].
      // Nomes curtos fixos ("name"/"role") para não depender do mapeamento automático de claims.
      new Claim("role", usuario.Tipo.ToString())
    };

    var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Chave));
    var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
    var expiraEm = DateTime.UtcNow.AddHours(_options.HorasValidade);

    var token = new JwtSecurityToken(
      issuer: _options.Issuer,
      audience: _options.Audience,
      claims: claims,
      expires: expiraEm,
      signingCredentials: credenciais);

    return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
  }
}
