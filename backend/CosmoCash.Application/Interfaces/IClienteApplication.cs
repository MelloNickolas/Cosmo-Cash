using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Clientes;

namespace CosmoCash.Application.Interfaces;

public interface IClienteApplication
{
  Task<PagedResponse<ClienteResumoResponse>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo);
  Task<ClienteResponse> ObterPorIdAsync(int id);
  Task<ClienteResponse> CriarAsync(ClienteRequest request);
  Task<ClienteResponse> AtualizarAsync(int id, ClienteRequest request);
  Task DesativarAsync(int id);
  Task<ClienteResponse> ReativarAsync(int id);
}
