using System.Net.Mail;
using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Usuarios;
using CosmoCash.Application.Exceptions;
using CosmoCash.Application.Interfaces;
using CosmoCash.Domain.Entities;
using CosmoCash.Domain.Enums;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Services.Interfaces;

namespace CosmoCash.Application.Applications;

public class UsuarioApplication : IUsuarioApplication
{
  private const int TamanhoPaginaMaximo = 100;

  private readonly IUsuarioRepository _usuarioRepository;
  private readonly IHashService _hashService;

  public UsuarioApplication(IUsuarioRepository usuarioRepository, IHashService hashService)
  {
    _usuarioRepository = usuarioRepository;
    _hashService = hashService;
  }

  public async Task<PagedResponse<UsuarioResponse>> ListarAsync(int pagina, int tamanhoPagina, string? busca, bool? ativo)
  {
    pagina = Math.Max(pagina, 1);
    tamanhoPagina = Math.Clamp(tamanhoPagina, 1, TamanhoPaginaMaximo);

    var (itens, total) = await _usuarioRepository.ListarAsync(pagina, tamanhoPagina, busca, ativo);

    return new PagedResponse<UsuarioResponse>
    {
      Itens = itens.Select(MapearResponse).ToList(),
      Pagina = pagina,
      TamanhoPagina = tamanhoPagina,
      Total = total
    };
  }

  public async Task<UsuarioResponse> ObterPorIdAsync(int id)
    => MapearResponse(await ValidarExistentePorId(id));

  public async Task<UsuarioResponse> CriarAsync(UsuarioRequest request)
  {
    if (request == null) throw new ValidationException("Os dados do usuário não podem ser vazios.");

    var nome = ValidarNome(request.Nome);
    var email = ValidarEmail(request.Email);
    ValidarTipo(request.Tipo);
    RegrasSenha.Validar(request.Senha);
    await ValidarEmailDisponivel(email, null);

    var usuario = new Usuario
    {
      Nome = nome,
      Email = email,
      SenhaHash = _hashService.GerarHash(request.Senha),
      Tipo = request.Tipo,
      Ativo = true
    };

    await _usuarioRepository.SalvarAsync(usuario);
    return MapearResponse(usuario);
  }

  public async Task<UsuarioResponse> AtualizarAsync(int id, UsuarioAtualizarRequest request)
  {
    if (request == null) throw new ValidationException("Os dados do usuário não podem ser vazios.");

    var usuario = await ValidarExistentePorId(id);
    var nome = ValidarNome(request.Nome);
    var email = ValidarEmail(request.Email);
    ValidarTipo(request.Tipo);
    await ValidarEmailDisponivel(email, id);

    // Rebaixar o último AdminTotal ativo deixaria o sistema sem ninguém para gerenciar usuários.
    var seraRebaixado = usuario.Tipo == TipoUsuario.AdminTotal && request.Tipo != TipoUsuario.AdminTotal;
    if (seraRebaixado && usuario.Ativo)
      await GarantirQueNaoEOUltimoAdmin(usuario, "rebaixar");

    usuario.Nome = nome;
    usuario.Email = email;
    usuario.Tipo = request.Tipo;

    await _usuarioRepository.AtualizarAsync(usuario);
    return MapearResponse(usuario);
  }

  public async Task AlterarSenhaAsync(int id, AlterarSenhaRequest request)
  {
    if (request == null) throw new ValidationException("Informe a nova senha.");

    var usuario = await ValidarExistentePorId(id);
    RegrasSenha.Validar(request.NovaSenha);

    usuario.SenhaHash = _hashService.GerarHash(request.NovaSenha);
    await _usuarioRepository.AtualizarAsync(usuario);
  }

  public async Task DesativarAsync(int id, int usuarioLogadoID)
  {
    var usuario = await ValidarExistentePorId(id);

    if (usuario.ID == usuarioLogadoID)
      throw new ValidationException("Você não pode desativar o seu próprio usuário.");

    if (!usuario.Ativo) return;

    if (usuario.Tipo == TipoUsuario.AdminTotal)
      await GarantirQueNaoEOUltimoAdmin(usuario, "desativar");

    usuario.Ativo = false;
    await _usuarioRepository.AtualizarAsync(usuario);
  }

  public async Task<UsuarioResponse> ReativarAsync(int id)
  {
    var usuario = await ValidarExistentePorId(id);

    if (!usuario.Ativo)
    {
      usuario.Ativo = true;
      await _usuarioRepository.AtualizarAsync(usuario);
    }

    return MapearResponse(usuario);
  }

  #region Úteis
  private static UsuarioResponse MapearResponse(Usuario usuario) => new()
  {
    ID = usuario.ID,
    Nome = usuario.Nome,
    Email = usuario.Email,
    Tipo = usuario.Tipo,
    Ativo = usuario.Ativo,
    CriadoEm = usuario.CriadoEm
  };

  private async Task<Usuario> ValidarExistentePorId(int id)
  {
    var usuario = await _usuarioRepository.ObterPorIdAsync(id);
    if (usuario == null) throw new NotFoundException("Usuário não localizado.");
    return usuario;
  }

  private static string ValidarNome(string? nome)
  {
    var valor = nome?.Trim() ?? string.Empty;
    if (valor.Length == 0) throw new ValidationException("O nome não pode ser vazio.");
    if (valor.Length > 120) throw new ValidationException("O nome pode ter no máximo 120 caracteres.");
    return valor;
  }

  private static string ValidarEmail(string? email)
  {
    var valor = email?.Trim().ToLowerInvariant() ?? string.Empty;
    if (valor.Length == 0) throw new ValidationException("O e-mail não pode ser vazio.");
    if (valor.Length > 200) throw new ValidationException("O e-mail pode ter no máximo 200 caracteres.");

    // MailAddress aceita formatos como "Nome <a@b.com>"; exigimos que seja só o endereço.
    var valido = MailAddress.TryCreate(valor, out var endereco) && endereco.Address == valor && valor.Contains('.');
    if (!valido) throw new ValidationException("Informe um e-mail válido.");

    return valor;
  }

  private static void ValidarTipo(TipoUsuario tipo)
  {
    if (!Enum.IsDefined(tipo)) throw new ValidationException("Tipo de usuário inválido.");
  }

  private async Task ValidarEmailDisponivel(string email, int? idIgnorado)
  {
    var existente = await _usuarioRepository.ObterPorEmailAsync(email);
    if (existente != null && existente.ID != idIgnorado)
      throw new ValidationException("Já existe um usuário com este e-mail.");
  }

  private async Task GarantirQueNaoEOUltimoAdmin(Usuario usuario, string acao)
  {
    var ativos = await _usuarioRepository.ContarAtivosPorTipoAsync(TipoUsuario.AdminTotal);
    if (usuario.Ativo && usuario.Tipo == TipoUsuario.AdminTotal && ativos <= 1)
      throw new ValidationException($"Não é possível {acao} o último AdminTotal ativo.");
  }
  #endregion
}
