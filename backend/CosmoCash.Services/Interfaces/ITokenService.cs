using CosmoCash.Domain.Entities;

namespace CosmoCash.Services.Interfaces;

public interface ITokenService
{
  (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
