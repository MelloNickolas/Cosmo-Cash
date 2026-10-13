using CosmoCash.Domain.Entities;
using CosmoCash.Domain.Enums;
using CosmoCash.Repository.Context;
using CosmoCash.Repository.Helpers;
using CosmoCash.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CosmoCash.Repository.Repositories;

public class UsuarioRepository : BaseRepository, IUsuarioRepository
{
  public UsuarioRepository(CosmoCashDbContext context) : base(context) { }

  public async Task<int> SalvarAsync(Usuario usuario)
  {
    _context.Usuarios.Add(usuario);
    await _context.SaveChangesAsync();
    return usuario.ID;
  }

  public async Task AtualizarAsync(Usuario usuario)
  {
    _context.Usuarios.Update(usuario);
    await _context.SaveChangesAsync();
  }

  public async Task<Usuario?> ObterPorIdAsync(int id)
    => await _context.Usuarios.FirstOrDefaultAsync(usuario => usuario.ID == id);

  public async Task<Usuario?> ObterPorEmailAsync(string email)
    => await _context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Email == email);

  public async Task<bool> ExisteTipoAsync(TipoUsuario tipo)
    => await _context.Usuarios.AnyAsync(usuario => usuario.Tipo == tipo);

  public async Task<int> ContarAtivosPorTipoAsync(TipoUsuario tipo)
    => await _context.Usuarios.CountAsync(usuario => usuario.Tipo == tipo && usuario.Ativo);

  public async Task<(IEnumerable<Usuario> Itens, int Total)> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, bool? ativo)
  {
    var consulta = _context.Usuarios.AsNoTracking().AsQueryable();

    if (ativo.HasValue)
      consulta = consulta.Where(usuario => usuario.Ativo == ativo.Value);

    if (!string.IsNullOrWhiteSpace(busca))
    {
      var padrao = BuscaHelper.PadraoContem(busca);
      consulta = consulta.Where(usuario =>
        EF.Functions.ILike(usuario.Nome, padrao, BuscaHelper.Escape)
        || EF.Functions.ILike(usuario.Email, padrao, BuscaHelper.Escape));
    }

    var total = await consulta.CountAsync();
    var itens = await consulta
      .OrderBy(usuario => usuario.Nome)
      .ThenBy(usuario => usuario.ID)
      .Skip((pagina - 1) * tamanhoPagina)
      .Take(tamanhoPagina)
      .ToListAsync();

    return (itens, total);
  }
}
