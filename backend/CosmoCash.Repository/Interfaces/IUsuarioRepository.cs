using CosmoCash.Domain.Entities;
using CosmoCash.Domain.Enums;

namespace CosmoCash.Repository.Interfaces;

public interface IUsuarioRepository
{
  Task<int> SalvarAsync(Usuario usuario);
  Task AtualizarAsync(Usuario usuario);
  Task<Usuario?> ObterPorIdAsync(int id);
  Task<Usuario?> ObterPorEmailAsync(string email);
  Task<bool> ExisteTipoAsync(TipoUsuario tipo);
  Task<int> ContarAtivosPorTipoAsync(TipoUsuario tipo);
  Task<(IEnumerable<Usuario> Itens, int Total)> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo);
}
