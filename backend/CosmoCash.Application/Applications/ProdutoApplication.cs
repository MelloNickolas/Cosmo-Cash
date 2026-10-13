using System.Text.RegularExpressions;
using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Produtos;
using CosmoCash.Application.Exceptions;
using CosmoCash.Application.Interfaces;
using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Interfaces;

namespace CosmoCash.Application.Applications;

public class ProdutoApplication : IProdutoApplication
{
  private const int TamanhoPaginaMaximo = 100;
  private const int VolumeMaximoMl = 5000;

  private readonly IProdutoRepository _produtoRepository;

  public ProdutoApplication(IProdutoRepository produtoRepository) => _produtoRepository = produtoRepository;

  public async Task<PagedResponse<ProdutoResponse>> ListarAsync(
    int pagina, int tamanhoPagina, string? busca, bool? ativo, string? marca, string? variacao)
  {
    pagina = Math.Max(pagina, 1);
    tamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoPaginaMaximo);

    var (itens, total) = await _produtoRepository.ListarAsync(pagina, tamanhoPagina, busca, ativo, marca, variacao);

    return new PagedResponse<ProdutoResponse>
    {
      Itens = itens.Select(MapearResponse).ToList(),
      Pagina = pagina,
      TamanhoPagina = tamanhoPagina,
      Total = total
    };
  }

  public async Task<ProdutoResponse> ObterPorIdAsync(int id)
    => MapearResponse(await ValidarExistentePorId(id));

  public async Task<ProdutoResponse> CriarAsync(ProdutoRequest request)
  {
    if (request == null) throw new ValidationException("Os dados do produto não podem ser vazios.");

    var produto = new Produto
    {
      Marca = ValidarTexto(request.Marca, "marca", 80),
      Descricao = ValidarTexto(request.Descricao, "descrição", 200),
      VolumeMl = ValidarVolume(request.VolumeMl),
      Variacao = ValidarTexto(request.Variacao, "variação", 60),
      Ativo = true
    };

    await ValidarDuplicado(produto, null);
    await _produtoRepository.SalvarAsync(produto);
    return MapearResponse(produto);
  }

  public async Task<ProdutoResponse> AtualizarAsync(int id, ProdutoRequest request)
  {
    if (request == null) throw new ValidationException("Os dados do produto não podem ser vazios.");

    var produto = await ValidarExistentePorId(id);

    produto.Marca = ValidarTexto(request.Marca, "marca", 80);
    produto.Descricao = ValidarTexto(request.Descricao, "descrição", 200);
    produto.VolumeMl = ValidarVolume(request.VolumeMl);
    produto.Variacao = ValidarTexto(request.Variacao, "variação", 60);

    await ValidarDuplicado(produto, id);
    await _produtoRepository.AtualizarAsync(produto);
    return MapearResponse(produto);
  }

  // Soft delete: o produto some das novas vendas, mas as vendas antigas continuam intactas.
  public async Task DesativarAsync(int id)
  {
    var produto = await ValidarExistentePorId(id);
    if (!produto.Ativo) return;

    produto.Ativo = false;
    await _produtoRepository.AtualizarAsync(produto);
  }

  public async Task<ProdutoResponse> ReativarAsync(int id)
  {
    var produto = await ValidarExistentePorId(id);

    if (!produto.Ativo)
    {
      produto.Ativo = true;
      await _produtoRepository.AtualizarAsync(produto);
    }

    return MapearResponse(produto);
  }

  public async Task<IEnumerable<string>> ListarMarcasAsync() => await _produtoRepository.ListarMarcasAsync();

  public async Task<IEnumerable<string>> ListarVariacoesAsync() => await _produtoRepository.ListarVariacoesAsync();

  #region Úteis
  private static ProdutoResponse MapearResponse(Produto produto) => new()
  {
    ID = produto.ID,
    Marca = produto.Marca,
    Descricao = produto.Descricao,
    VolumeMl = produto.VolumeMl,
    Variacao = produto.Variacao,
    Ativo = produto.Ativo,
    CriadoEm = produto.CriadoEm
  };

  private async Task<Produto> ValidarExistentePorId(int id)
  {
    var produto = await _produtoRepository.ObterPorIdAsync(id);
    if (produto == null) throw new NotFoundException("Produto não localizado.");
    return produto;
  }

  // Remove espaços das pontas e colapsa espaços repetidos ("Eau  de   Parfum" -> "Eau de Parfum").
  private static string ValidarTexto(string? texto, string campo, int tamanhoMaximo)
  {
    var valor = Regex.Replace(texto?.Trim() ?? string.Empty, @"\s+", " ");

    if (valor.Length == 0)
      throw new ValidationException($"A {campo} não pode ser vazia.");

    if (valor.Length > tamanhoMaximo)
      throw new ValidationException($"A {campo} pode ter no máximo {tamanhoMaximo} caracteres.");

    return valor;
  }

  private static int ValidarVolume(int volumeMl)
  {
    if (volumeMl <= 0 || volumeMl > VolumeMaximoMl)
      throw new ValidationException($"O volume deve estar entre 1 e {VolumeMaximoMl} ml.");

    return volumeMl;
  }

  private async Task ValidarDuplicado(Produto produto, int? idIgnorado)
  {
    var duplicado = await _produtoRepository.ObterDuplicadoAsync(
      produto.Marca, produto.Descricao, produto.VolumeMl, produto.Variacao);

    if (duplicado == null || duplicado.ID == idIgnorado) return;

    throw new ValidationException(duplicado.Ativo
      ? "Já existe um produto igual cadastrado."
      : "Já existe um produto igual, mas ele está desativado. Reative-o em vez de cadastrar de novo.");
  }
  #endregion
}
