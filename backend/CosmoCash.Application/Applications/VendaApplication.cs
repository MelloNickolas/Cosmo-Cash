using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Vendas;
using CosmoCash.Application.Exceptions;
using CosmoCash.Application.Interfaces;
using CosmoCash.Domain.Entities;
using CosmoCash.Domain.Enums;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Repository.Models;

namespace CosmoCash.Application.Applications;

public class VendaApplication : IVendaApplication
{
  private const int TamanhoPaginaMaximo = 100;
  private const int MaximoItens = 50;
  private const int QuantidadeMaxima = 999;
  private const decimal ValorMaximo = 9_999_999.99m;

  // Tolerância para relógios levemente adiantados no celular do usuário.
  private static readonly TimeSpan ToleranciaFuturo = TimeSpan.FromMinutes(5);

  private readonly IVendaRepository _vendaRepository;
  private readonly IProdutoRepository _produtoRepository;
  private readonly IClienteRepository _clienteRepository;

  public VendaApplication(
    IVendaRepository vendaRepository,
    IProdutoRepository produtoRepository,
    IClienteRepository clienteRepository)
  {
    _vendaRepository = vendaRepository;
    _produtoRepository = produtoRepository;
    _clienteRepository = clienteRepository;
  }

  public async Task<PagedResponse<VendaResumoResponse>> ListarAsync(
    int pagina, int tamanhoPagina, DateOnly? dataInicio, DateOnly? dataFim,
    StatusVenda? status, FormaPagamento? formaPagamento, int? clienteID, int? usuarioID)
  {
    if (dataInicio.HasValue && dataFim.HasValue && dataFim < dataInicio)
      throw new ValidationException("A data final não pode ser anterior à data inicial.");

    var filtro = new VendaFiltro
    {
      Pagina = Math.Max(pagina, 1),
      TamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoPaginaMaximo),
      // O dia é o dia de São Paulo: do início do dia inicial até o início do dia seguinte ao final.
      InicioUtc = dataInicio.HasValue ? FusoHorario.InicioDoDiaUtc(dataInicio.Value) : null,
      FimUtcExclusivo = dataFim.HasValue ? FusoHorario.InicioDoDiaUtc(dataFim.Value.AddDays(1)) : null,
      Status = status,
      FormaPagamento = formaPagamento,
      ClienteID = clienteID,
      UsuarioID = usuarioID
    };

    var (itens, total) = await _vendaRepository.ListarAsync(filtro);

    return new PagedResponse<VendaResumoResponse>
    {
      Itens = itens.Select(MapearResumo).ToList(),
      Pagina = filtro.Pagina,
      TamanhoPagina = filtro.TamanhoPagina,
      Total = total
    };
  }

  public async Task<VendaResponse> ObterPorIdAsync(int id)
    => MapearResponse(await ValidarExistentePorId(id));

  public async Task<VendaResponse> CriarAsync(VendaRequest request, int usuarioLogadoID)
  {
    if (request == null) throw new ValidationException("Os dados da venda não podem ser vazios.");

    var dataVenda = ValidarData(request.DataVenda);
    ValidarFormaPagamento(request.FormaPagamento);
    var itens = await ValidarEMontarItens(request.Itens, produtosJaNaVenda: []);
    await ValidarCliente(request.ClienteID, clienteAtualID: null);

    var venda = new Venda
    {
      DataVenda = dataVenda,
      FormaPagamento = request.FormaPagamento,
      Status = StatusVenda.Concluida,
      ClienteID = request.ClienteID,
      UsuarioID = usuarioLogadoID,
      Itens = itens
    };

    var id = await _vendaRepository.SalvarAsync(venda);
    return MapearResponse(await ValidarExistentePorId(id));
  }

  public async Task<VendaResponse> AtualizarAsync(int id, VendaRequest request)
  {
    if (request == null) throw new ValidationException("Os dados da venda não podem ser vazios.");

    var venda = await ValidarExistentePorId(id);
    if (venda.Status == StatusVenda.Cancelada)
      throw new ValidationException("Uma venda cancelada não pode ser editada. Reative-a primeiro.");

    var dataVenda = ValidarData(request.DataVenda ?? venda.DataVenda);
    ValidarFormaPagamento(request.FormaPagamento);

    // Produto/cliente desativado só é barrado se for NOVO na venda: o que já estava nela pode ficar.
    var itens = await ValidarEMontarItens(request.Itens, produtosJaNaVenda: venda.Itens.Select(i => i.ProdutoID));
    await ValidarCliente(request.ClienteID, clienteAtualID: venda.ClienteID);

    venda.DataVenda = dataVenda;
    venda.FormaPagamento = request.FormaPagamento;
    venda.ClienteID = request.ClienteID;

    // Substitui os itens por inteiro: os antigos viram órfãos e são removidos junto no SaveChanges.
    venda.Itens.Clear();
    foreach (var item in itens) venda.Itens.Add(item);

    await _vendaRepository.SalvarAlteracoesAsync();
    return MapearResponse(await ValidarExistentePorId(id));
  }

  // Soft delete: a venda continua no banco (e no histórico), mas sai dos totais do dashboard.
  public async Task CancelarAsync(int id)
  {
    var venda = await ValidarExistentePorId(id);
    if (venda.Status == StatusVenda.Cancelada) return;

    venda.Status = StatusVenda.Cancelada;
    venda.CanceladaEm = DateTime.UtcNow;
    await _vendaRepository.SalvarAlteracoesAsync();
  }

  public async Task<VendaResponse> ReativarAsync(int id)
  {
    var venda = await ValidarExistentePorId(id);

    if (venda.Status == StatusVenda.Cancelada)
    {
      venda.Status = StatusVenda.Concluida;
      venda.CanceladaEm = null;
      await _vendaRepository.SalvarAlteracoesAsync();
    }

    return MapearResponse(venda);
  }

  #region Úteis
  private async Task<Venda> ValidarExistentePorId(int id)
  {
    var venda = await _vendaRepository.ObterPorIdAsync(id);
    if (venda == null) throw new NotFoundException("Venda não localizada.");
    return venda;
  }

  private static DateTime ValidarData(DateTime? informada)
  {
    if (informada == null) return DateTime.UtcNow;

    var utc = FusoHorario.ParaUtc(informada.Value);
    if (utc > DateTime.UtcNow.Add(ToleranciaFuturo))
      throw new ValidationException("A data da venda não pode ser no futuro.");

    return utc;
  }

  private static void ValidarFormaPagamento(FormaPagamento forma)
  {
    if (!Enum.IsDefined(forma))
      throw new ValidationException("Informe a forma de pagamento (Cartao ou Pix).");
  }

  private async Task ValidarCliente(int? clienteID, int? clienteAtualID)
  {
    if (clienteID == null) return;

    var cliente = await _clienteRepository.ObterPorIdAsync(clienteID.Value);
    if (cliente == null) throw new ValidationException("Cliente não localizado.");

    if (!cliente.Ativo && clienteID != clienteAtualID)
      throw new ValidationException("Este cliente está desativado e não pode ser usado em uma nova venda.");
  }

  private async Task<List<ItemVenda>> ValidarEMontarItens(
    IEnumerable<VendaItemRequest>? itensRequest, IEnumerable<int> produtosJaNaVenda)
  {
    var lista = itensRequest?.ToList() ?? [];

    if (lista.Count == 0) throw new ValidationException("A venda precisa ter pelo menos um item.");
    if (lista.Count > MaximoItens) throw new ValidationException($"A venda pode ter no máximo {MaximoItens} itens.");

    var produtos = (await _produtoRepository.ListarPorIdsAsync(lista.Select(i => i.ProdutoID)))
      .ToDictionary(produto => produto.ID);
    var jaNaVenda = produtosJaNaVenda.ToHashSet();

    var resultado = new List<ItemVenda>();

    for (var i = 0; i < lista.Count; i++)
    {
      var item = lista[i];
      var rotulo = $"Item {i + 1}";

      if (!produtos.TryGetValue(item.ProdutoID, out var produto))
        throw new ValidationException($"{rotulo}: produto não localizado.");

      if (!produto.Ativo && !jaNaVenda.Contains(produto.ID))
        throw new ValidationException(
          $"{rotulo}: o produto \"{produto.Marca} {produto.Descricao}\" está desativado e não pode ser usado em uma nova venda.");

      if (item.Quantidade < 1 || item.Quantidade > QuantidadeMaxima)
        throw new ValidationException($"{rotulo}: a quantidade deve estar entre 1 e {QuantidadeMaxima}.");

      ValidarValor(item.PrecoVenda, $"{rotulo}: o preço de venda", obrigatorioMaiorQueZero: true);
      ValidarValor(item.PrecoCusto, $"{rotulo}: o preço de custo", obrigatorioMaiorQueZero: false);

      resultado.Add(new ItemVenda
      {
        ProdutoID = item.ProdutoID,
        Quantidade = item.Quantidade,
        PrecoVenda = item.PrecoVenda,
        PrecoCusto = item.PrecoCusto
      });
    }

    return resultado;
  }

  private static void ValidarValor(decimal valor, string campo, bool obrigatorioMaiorQueZero)
  {
    if (obrigatorioMaiorQueZero ? valor <= 0 : valor < 0)
      throw new ValidationException(obrigatorioMaiorQueZero
        ? $"{campo} deve ser maior que zero."
        : $"{campo} não pode ser negativo.");

    if (valor > ValorMaximo)
      throw new ValidationException($"{campo} é alto demais.");

    if (decimal.Round(valor, 2) != valor)
      throw new ValidationException($"{campo} deve ter no máximo 2 casas decimais.");
  }

  private static VendaResponse MapearResponse(Venda venda)
  {
    var itens = venda.Itens
      .OrderBy(item => item.ID)
      .Select(item => new VendaItemResponse
      {
        ID = item.ID,
        ProdutoID = item.ProdutoID,
        Marca = item.Produto.Marca,
        Descricao = item.Produto.Descricao,
        VolumeMl = item.Produto.VolumeMl,
        Variacao = item.Produto.Variacao,
        Quantidade = item.Quantidade,
        PrecoVenda = item.PrecoVenda,
        PrecoCusto = item.PrecoCusto,
        Subtotal = item.PrecoVenda * item.Quantidade,
        Lucro = (item.PrecoVenda - item.PrecoCusto) * item.Quantidade
      })
      .ToList();

    var valorTotal = itens.Sum(item => item.Subtotal);
    var custoTotal = itens.Sum(item => item.PrecoCusto * item.Quantidade);

    return new VendaResponse
    {
      ID = venda.ID,
      DataVenda = venda.DataVenda,
      FormaPagamento = venda.FormaPagamento,
      Status = venda.Status,
      CanceladaEm = venda.CanceladaEm,
      Cliente = venda.Cliente == null
        ? null
        : new VendaPessoaResponse { ID = venda.Cliente.ID, Nome = venda.Cliente.Nome },
      Usuario = new VendaPessoaResponse { ID = venda.Usuario.ID, Nome = venda.Usuario.Nome },
      Itens = itens,
      ValorTotal = valorTotal,
      CustoTotal = custoTotal,
      Lucro = valorTotal - custoTotal
    };
  }

  private static VendaResumoResponse MapearResumo(VendaResumoModel modelo) => new()
  {
    ID = modelo.ID,
    DataVenda = modelo.DataVenda,
    FormaPagamento = modelo.FormaPagamento,
    Status = modelo.Status,
    ClienteID = modelo.ClienteID,
    ClienteNome = modelo.ClienteNome,
    UsuarioID = modelo.UsuarioID,
    UsuarioNome = modelo.UsuarioNome,
    QuantidadeItens = modelo.QuantidadeItens,
    ValorTotal = modelo.ValorTotal,
    CustoTotal = modelo.CustoTotal,
    Lucro = modelo.ValorTotal - modelo.CustoTotal
  };
  #endregion
}
