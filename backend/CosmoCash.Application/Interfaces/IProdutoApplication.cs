using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Produtos;

namespace CosmoCash.Application.Interfaces;

public interface IProdutoApplication
{
  Task<PagedResponse<ProdutoResponse>> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, bool? ativo, string? marca, string? variacao);

  Task<ProdutoResponse> ObterPorIdAsync(int id);
  Task<ProdutoResponse> CriarAsync(ProdutoRequest request);
  Task<ProdutoResponse> AtualizarAsync(int id, ProdutoRequest request);
  Task DesativarAsync(int id);
  Task<ProdutoResponse> ReativarAsync(int id);
  Task<IEnumerable<string>> ListarMarcasAsync();
  Task<IEnumerable<string>> ListarVariacoesAsync();
}
