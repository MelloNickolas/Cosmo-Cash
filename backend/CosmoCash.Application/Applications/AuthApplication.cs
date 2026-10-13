using CosmoCash.Application.Common;
using CosmoCash.Application.DTOs.Auth;
using CosmoCash.Application.Exceptions;
using CosmoCash.Application.Interfaces;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Services.Interfaces;

namespace CosmoCash.Application.Applications;

public class AuthApplication : IAuthApplication
{
  private const string MensagemCredencialInvalida = "E-mail ou senha inválidos.";

  private readonly IUsuarioRepository _usuarioRepository;
  private readonly IHashService _hashService;
  private readonly ITokenService _tokenService;

  public AuthApplication(
    IUsuarioRepository usuarioRepository,
    IHashService hashService,
    ITokenService tokenService)
  {
    _usuarioRepository = usuarioRepository;
    _hashService = hashService;
    _tokenService = tokenService;
  }

  public async Task<LoginResponse> LoginAsync(LoginRequest request)
  {
    if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
      throw new ValidationException("Informe e-mail e senha.");

    var email = request.Email.Trim().ToLowerInvariant();
    var usuario = await _usuarioRepository.ObterPorEmailAsync(email);

    // Sempre roda o BCrypt (mesmo sem usuário) para o tempo de resposta não revelar
    // se o e-mail existe.
    var senhaConfere = _hashService.Verificar(request.Senha, usuario?.SenhaHash ?? _hashService.HashFicticio);

    // Mesma mensagem para e-mail inexistente, senha errada e usuário inativo:
    // não revelamos qual dos três aconteceu.
    if (usuario == null || !usuario.Ativo || !senhaConfere)
      throw new UnauthorizedException(MensagemCredencialInvalida);

    var (token, expiraEm) = _tokenService.GerarToken(usuario);

    return new LoginResponse
    {
      Token = token,
      ExpiraEm = expiraEm,
      UsuarioID = usuario.ID,
      Nome = usuario.Nome,
      Email = usuario.Email,
      Tipo = usuario.Tipo
    };
  }

  public async Task AlterarMinhaSenhaAsync(int usuarioLogadoID, AlterarMinhaSenhaRequest request)
  {
    if (request == null || string.IsNullOrEmpty(request.SenhaAtual))
      throw new ValidationException("Informe a senha atual e a nova senha.");

    var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioLogadoID);
    if (usuario == null || !usuario.Ativo)
      throw new UnauthorizedException("Sessão inválida.");

    if (!_hashService.Verificar(request.SenhaAtual, usuario.SenhaHash))
      throw new ValidationException("A senha atual está incorreta.");

    RegrasSenha.Validar(request.NovaSenha);

    usuario.SenhaHash = _hashService.GerarHash(request.NovaSenha);
    await _usuarioRepository.AtualizarAsync(usuario);
  }
}
