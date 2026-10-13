using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Context;
using CosmoCash.Repository.Helpers;
using CosmoCash.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CosmoCash.Repository.Repositories;

public class ProdutoRepository : BaseRepository, IProdutoRepository
{
  public ProdutoRepository(CosmoCashDbContext context) : base(context) { }

  public async Task<int> SalvarAsync(Produto produto)
  {
    _context.Produtos.Add(produto);
    await _context.SaveChangesAsync();
    return produto.ID;
  }

  public async Task AtualizarAsync(Produto produto)
  {
    _context.Produtos.Update(produto);
    await _context.SaveChangesAsync();
  }

  public async Task<Produto?> ObterPorIdAsync(int id)
    => await _context.Produtos.FirstOrDefaultAsync(produto => produto.ID == id);

  public async Task<IEnumerable<Produto>> ListarPorIdsAsync(IEnumerable<int> ids)
  {
    var lista = ids.Distinct().ToList();
    return await _context.Produtos.AsNoTracking().Where(produto => lista.Contains(produto.ID)).ToListAsync();
  }

  public async Task<Produto?> ObterDuplicadoAsync(string marca, string descricao, int volumeMl, string variacao)
  {
    var marcaMin = marca.ToLower();
    var descricaoMin = descricao.ToLower();
    var variacaoMin = variacao.ToLower();

    return await _context.Produtos.FirstOrDefaultAsync(produto =>
      produto.VolumeMl == volumeMl
      && produto.Marca.ToLower() == marcaMin
      && produto.Descricao.ToLower() == descricaoMin
      && produto.Variacao.ToLower() == variacaoMin);
  }

  public async Task<(IEnumerable<Produto> Itens, int Total)> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, bool? ativo, string? marca, string? variacao)
  {
    var consulta = _context.Produtos.AsNoTracking().AsQueryable();

    if (ativo.HasValue)
      consulta = consulta.Where(produto => produto.Ativo == ativo.Value);

    if (!string.IsNullOrWhiteSpace(marca))
    {
      var marcaMin = marca.Trim().ToLower();
      consulta = consulta.Where(produto => produto.Marca.ToLower() == marcaMin);
    }

    if (!string.IsNullOrWhiteSpace(variacao))
    {
      var variacaoMin = variacao.Trim().ToLower();
      consulta = consulta.Where(produto => produto.Variacao.ToLower() == variacaoMin);
    }

    if (!string.IsNullOrWhiteSpace(busca))
    {
      var padrao = BuscaHelper.PadraoContem(busca);
      consulta = consulta.Where(produto =>
        EF.Functions.ILike(produto.Marca, padrao, BuscaHelper.Escape)
        || EF.Functions.ILike(produto.Descricao, padrao, BuscaHelper.Escape));
    }

    var total = await consulta.CountAsync();
    var itens = await consulta
      .OrderBy(produto => produto.Marca)
      .ThenBy(produto => produto.Descricao)
      .ThenBy(produto => produto.VolumeMl)
      .ThenBy(produto => produto.ID)
      .Skip((pagina - 1) * tamanhoPagina)
      .Take(tamanhoPagina)
      .ToListAsync();

    return (itens, total);
  }

  public async Task<IEnumerable<string>> ListarMarcasAsync()
    => await _context.Produtos.AsNoTracking()
      .Select(produto => produto.Marca)
      .Distinct()
      .OrderBy(marca => marca)
      .ToListAsync();

  public async Task<IEnumerable<string>> ListarVariacoesAsync()
    => await _context.Produtos.AsNoTracking()
      .Select(produto => produto.Variacao)
      .Distinct()
      .OrderBy(variacao => variacao)
      .ToListAsync();
}
