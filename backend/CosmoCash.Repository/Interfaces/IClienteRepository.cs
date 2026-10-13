using CosmoCash.Domain.Entities;

namespace CosmoCash.Repository.Interfaces;

public interface IClienteRepository
{
  Task<int> SalvarAsync(Cliente cliente);
  Task AtualizarAsync(Cliente cliente);
  Task<Cliente?> ObterPorIdAsync(int id);
  Task<Cliente?> ObterPorCpfAsync(string cpf);

  // 'digitosBusca' é a mesma busca só com dígitos, para achar por CPF/telefone ("123.456" -> "123456").
  Task<(IEnumerable<Cliente> Itens, int Total)> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, string? digitosBusca, bool? ativo);
}
