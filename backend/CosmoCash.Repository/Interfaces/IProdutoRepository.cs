using CosmoCash.Domain.Entities;

namespace CosmoCash.Repository.Interfaces;

public interface IProdutoRepository
{
  Task<int> SalvarAsync(Produto produto);
  Task AtualizarAsync(Produto produto);
  Task<Produto?> ObterPorIdAsync(int id);
  Task<IEnumerable<Produto>> ListarPorIdsAsync(IEnumerable<int> ids);

  // Procura um produto idêntico (marca, descrição, volume e variação), ignorando maiúsculas.
  Task<Produto?> ObterDuplicadoAsync(string marca, string descricao, int volumeMl, string variacao);

  Task<(IEnumerable<Produto> Itens, int Total)> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, bool? ativo, string? marca, string? variacao);

  Task<IEnumerable<string>> ListarMarcasAsync();
  Task<IEnumerable<string>> ListarVariacoesAsync();
}
