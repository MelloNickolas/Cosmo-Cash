using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Vendas;
using CosmoCash.Domain.Enums;

namespace CosmoCash.Application.Interfaces;

public interface IVendaApplication
{
  Task<PagedResponse<VendaResumoResponse>> ListarAsync(
    int pagina, int tamanhoPagina, DateOnly? dataInicio, DateOnly? dataFim,
    StatusVenda? status, FormaPagamento? formaPagamento, int? clienteID, int? usuarioID);

  Task<VendaResponse> ObterPorIdAsync(int id);
  Task<VendaResponse> CriarAsync(VendaRequest request, int usuarioLogadoID);
  Task<VendaResponse> AtualizarAsync(int id, VendaRequest request);
  Task CancelarAsync(int id);
  Task<VendaResponse> ReativarAsync(int id);
}
