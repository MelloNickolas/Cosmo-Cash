using System.Text.RegularExpressions;
using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Clientes;
using CosmoCash.Application.Exceptions;
using CosmoCash.Application.Interfaces;
using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Interfaces;

namespace CosmoCash.Application.Applications;

public class ClienteApplication : IClienteApplication
{
  private const int TamanhoPaginaMaximo = 100;

  private readonly IClienteRepository _clienteRepository;

  public ClienteApplication(IClienteRepository clienteRepository) => _clienteRepository = clienteRepository;

  public async Task<PagedResponse<ClienteResumoResponse>> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, bool? ativo)
  {
    pagina = Math.Max(pagina, 1);
    tamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoPaginaMaximo);

    var (itens, total) = await _clienteRepository.ListarAsync(
      pagina, tamanhoPagina, busca, Documentos.ApenasDigitos(busca), ativo);

    return new PagedResponse<ClienteResumoResponse>
    {
      Itens = itens.Select(MapearResumo).ToList(),
      Pagina = pagina,
      TamanhoPagina = tamanhoPagina,
      Total = total
    };
  }

  public async Task<ClienteResponse> ObterPorIdAsync(int id)
    => MapearResponse(await ValidarExistentePorId(id));

  public async Task<ClienteResponse> CriarAsync(ClienteRequest request)
  {
    if (request == null) throw new ValidationException("Os dados do cliente não podem ser vazios.");

    var cliente = new Cliente { Ativo = true };
    PreencherEValidar(cliente, request);

    await ValidarCpfDisponivel(cliente.Cpf, null);
    await _clienteRepository.SalvarAsync(cliente);
    return MapearResponse(cliente);
  }

  public async Task<ClienteResponse> AtualizarAsync(int id, ClienteRequest request)
  {
    if (request == null) throw new ValidationException("Os dados do cliente não podem ser vazios.");

    var cliente = await ValidarExistentePorId(id);
    PreencherEValidar(cliente, request);

    await ValidarCpfDisponivel(cliente.Cpf, id);
    await _clienteRepository.AtualizarAsync(cliente);
    return MapearResponse(cliente);
  }

  // Soft delete: o cliente some das novas vendas, mas as vendas antigas continuam ligadas a ele.
  public async Task DesativarAsync(int id)
  {
    var cliente = await ValidarExistentePorId(id);
    if (!cliente.Ativo) return;

    cliente.Ativo = false;
    await _clienteRepository.AtualizarAsync(cliente);
  }

  public async Task<ClienteResponse> ReativarAsync(int id)
  {
    var cliente = await ValidarExistentePorId(id);

    if (!cliente.Ativo)
    {
      cliente.Ativo = true;
      await _clienteRepository.AtualizarAsync(cliente);
    }

    return MapearResponse(cliente);
  }

  #region Úteis
  private static ClienteResponse MapearResponse(Cliente cliente) => new()
  {
    ID = cliente.ID,
    Nome = cliente.Nome,
    Cpf = cliente.Cpf,
    Telefone = cliente.Telefone,
    Cep = cliente.Cep,
    Endereco = cliente.Endereco,
    Ativo = cliente.Ativo,
    CriadoEm = cliente.CriadoEm
  };

  private static ClienteResumoResponse MapearResumo(Cliente cliente) => new()
  {
    ID = cliente.ID,
    Nome = cliente.Nome,
    CpfMascarado = Documentos.MascararCpf(cliente.Cpf),
    Telefone = cliente.Telefone,
    Ativo = cliente.Ativo
  };

  private async Task<Cliente> ValidarExistentePorId(int id)
  {
    var cliente = await _clienteRepository.ObterPorIdAsync(id);
    if (cliente == null) throw new NotFoundException("Cliente não localizado.");
    return cliente;
  }

  // Valida e normaliza tudo antes de tocar na entidade (se algo falhar, nada é alterado pela metade).
  private static void PreencherEValidar(Cliente cliente, ClienteRequest request)
  {
    var nome = Regex.Replace(request.Nome?.Trim() ?? string.Empty, @"\s+", " ");
    if (nome.Length == 0) throw new ValidationException("O nome não pode ser vazio.");
    if (nome.Length > 120) throw new ValidationException("O nome pode ter no máximo 120 caracteres.");

    var cpf = Documentos.ApenasDigitos(request.Cpf);
    if (cpf.Length > 0 && !Documentos.CpfValido(cpf))
      throw new ValidationException("Informe um CPF válido.");

    var telefone = Documentos.ApenasDigitos(request.Telefone);
    if (telefone.Length > 0 && (telefone.Length < 10 || telefone.Length > 13))
      throw new ValidationException("Informe um telefone válido, com DDD.");

    var cep = Documentos.ApenasDigitos(request.Cep);
    if (cep.Length > 0 && cep.Length != 8)
      throw new ValidationException("Informe um CEP válido (8 dígitos).");

    var endereco = Regex.Replace(request.Endereco?.Trim() ?? string.Empty, @"\s+", " ");
    if (endereco.Length > 300)
      throw new ValidationException("O endereço pode ter no máximo 300 caracteres.");

    cliente.Nome = nome;
    cliente.Cpf = cpf.Length == 0 ? null : cpf;
    cliente.Telefone = telefone.Length == 0 ? null : telefone;
    cliente.Cep = cep.Length == 0 ? null : cep;
    cliente.Endereco = endereco.Length == 0 ? null : endereco;
  }

  private async Task ValidarCpfDisponivel(string? cpf, int? idIgnorado)
  {
    if (cpf == null) return;

    var existente = await _clienteRepository.ObterPorCpfAsync(cpf);
    if (existente == null || existente.ID == idIgnorado) return;

    throw new ValidationException(existente.Ativo
      ? "Já existe um cliente com este CPF."
      : "Já existe um cliente com este CPF, mas ele está desativado. Reative-o em vez de cadastrar de novo.");
  }
  #endregion
}
