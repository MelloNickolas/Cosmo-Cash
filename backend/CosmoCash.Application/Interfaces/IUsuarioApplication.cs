using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Usuarios;

namespace CosmoCash.Application.Interfaces;

public interface IUsuarioApplication
{
  Task<PagedResponse<UsuarioResponse>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo);
  Task<UsuarioResponse> ObterPorIdAsync(int id);
  Task<UsuarioResponse> CriarAsync(UsuarioRequest request);
  Task<UsuarioResponse> AtualizarAsync(int id, UsuarioAtualizarRequest request);
  Task AlterarSenhaAsync(int id, AlterarSenhaRequest request);
  Task DesativarAsync(int id, int usuarioLogadoID);
  Task<UsuarioResponse> ReativarAsync(int id);
}
