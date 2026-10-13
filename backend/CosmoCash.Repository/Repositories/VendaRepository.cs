using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Context;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace CosmoCash.Repository.Repositories;

public class VendaRepository : BaseRepository, IVendaRepository
{
  public VendaRepository(CosmoCashDbContext context) : base(context) { }

  public async Task<int> SalvarAsync(Venda venda)
  {
    _context.Vendas.Add(venda);
    await _context.SaveChangesAsync();
    return venda.ID;
  }

  public async Task SalvarAlteracoesAsync() => await _context.SaveChangesAsync();

  public async Task<Venda?> ObterPorIdAsync(int id)
    => await _context.Vendas
      .Include(venda => venda.Cliente)
      .Include(venda => venda.Usuario)
      .Include(venda => venda.Itens)
        .ThenInclude(item => item.Produto)
      .FirstOrDefaultAsync(venda => venda.ID == id);

  public async Task<(IEnumerable<VendaResumoModel> Itens, int Total)> ListarAsync(VendaFiltro filtro)
  {
    var consulta = _context.Vendas.AsNoTracking().AsQueryable();

    if (filtro.InicioUtc.HasValue)
      consulta = consulta.Where(venda => venda.DataVenda >= filtro.InicioUtc.Value);

    if (filtro.FimUtcExclusivo.HasValue)
      consulta = consulta.Where(venda => venda.DataVenda < filtro.FimUtcExclusivo.Value);

    if (filtro.Status.HasValue)
      consulta = consulta.Where(venda => venda.Status == filtro.Status.Value);

    if (filtro.FormaPagamento.HasValue)
      consulta = consulta.Where(venda => venda.FormaPagamento == filtro.FormaPagamento.Value);

    if (filtro.ClienteID.HasValue)
      consulta = consulta.Where(venda => venda.ClienteID == filtro.ClienteID.Value);

    if (filtro.UsuarioID.HasValue)
      consulta = consulta.Where(venda => venda.UsuarioID == filtro.UsuarioID.Value);

    var total = await consulta.CountAsync();

    var itens = await consulta
      .OrderByDescending(venda => venda.DataVenda)
      .ThenByDescending(venda => venda.ID)
      .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
      .Take(filtro.TamanhoPagina)
      .Select(venda => new VendaResumoModel
      {
        ID = venda.ID,
        DataVenda = venda.DataVenda,
        FormaPagamento = venda.FormaPagamento,
        Status = venda.Status,
        ClienteID = venda.ClienteID,
        ClienteNome = venda.Cliente != null ? venda.Cliente.Nome : null,
        UsuarioID = venda.UsuarioID,
        UsuarioNome = venda.Usuario.Nome,
        QuantidadeItens = venda.Itens.Sum(item => item.Quantidade),
        ValorTotal = venda.Itens.Sum(item => item.PrecoVenda * item.Quantidade),
        CustoTotal = venda.Itens.Sum(item => item.PrecoCusto * item.Quantidade)
      })
      .ToListAsync();

    return (itens, total);
  }
}
