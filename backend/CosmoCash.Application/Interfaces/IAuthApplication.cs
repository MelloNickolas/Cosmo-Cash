using CosmoCash.Application.DTOs.Auth;

namespace CosmoCash.Application.Interfaces;

public interface IAuthApplication
{
  Task<LoginResponse> LoginAsync(LoginRequest request);
  Task AlterarMinhaSenhaAsync(int usuarioLogadoID, AlterarMinhaSenhaRequest request);
}
