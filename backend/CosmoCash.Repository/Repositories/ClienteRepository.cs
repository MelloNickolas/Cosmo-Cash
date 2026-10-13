using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Context;
using CosmoCash.Repository.Helpers;
using CosmoCash.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CosmoCash.Repository.Repositories;

public class ClienteRepository : BaseRepository, IClienteRepository
{
  public ClienteRepository(CosmoCashDbContext context) : base(context) { }

  public async Task<int> SalvarAsync(Cliente cliente)
  {
    _context.Clientes.Add(cliente);
    await _context.SaveChangesAsync();
    return cliente.ID;
  }

  public async Task AtualizarAsync(Cliente cliente)
  {
    _context.Clientes.Update(cliente);
    await _context.SaveChangesAsync();
  }

  public async Task<Cliente?> ObterPorIdAsync(int id)
    => await _context.Clientes.FirstOrDefaultAsync(cliente => cliente.ID == id);

  public async Task<Cliente?> ObterPorCpfAsync(string cpf)
    => await _context.Clientes.FirstOrDefaultAsync(cliente => cliente.Cpf == cpf);

  public async Task<(IEnumerable<Cliente> Itens, int Total)> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, string? digitosBusca, bool? ativo)
  {
    var consulta = _context.Clientes.AsNoTracking().AsQueryable();

    if (ativo.HasValue)
      consulta = consulta.Where(cliente => cliente.Ativo == ativo.Value);

    if (!string.IsNullOrWhiteSpace(busca))
    {
      var padraoNome = BuscaHelper.PadraoContem(busca);

      // Com 3+ dígitos na busca, também procura em CPF e telefone (que são guardados só com dígitos).
      if (!string.IsNullOrEmpty(digitosBusca) && digitosBusca.Length >= 3)
      {
        var padraoDigitos = $"%{digitosBusca}%";
        consulta = consulta.Where(cliente =>
          EF.Functions.ILike(cliente.Nome, padraoNome, BuscaHelper.Escape)
          || (cliente.Cpf != null && EF.Functions.ILike(cliente.Cpf, padraoDigitos, BuscaHelper.Escape))
          || (cliente.Telefone != null && EF.Functions.ILike(cliente.Telefone, padraoDigitos, BuscaHelper.Escape)));
      }
      else
      {
        consulta = consulta.Where(cliente =>
          EF.Functions.ILike(cliente.Nome, padraoNome, BuscaHelper.Escape));
      }
    }

    var total = await consulta.CountAsync();
    var itens = await consulta
      .OrderBy(cliente => cliente.Nome)
      .ThenBy(cliente => cliente.ID)
      .Skip((pagina - 1) * tamanhoPagina)
      .Take(tamanhoPagina)
      .ToListAsync();

    return (itens, total);
  }
}
