using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Models;

namespace CosmoCash.Repository.Interfaces;

public interface IVendaRepository
{
  Task<int> SalvarAsync(Venda venda);

  // Persiste alterações de uma venda carregada por ObterPorIdAsync (entidade rastreada).
  Task SalvarAlteracoesAsync();

  // Venda com itens, produtos, cliente e usuário.
  Task<Venda?> ObterPorIdAsync(int id);

  Task<(IEnumerable<VendaResumoModel> Itens, int Total)> ListarAsync(VendaFiltro filtro);
}
