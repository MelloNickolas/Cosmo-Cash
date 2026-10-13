using CosmoCash.Application.Common;
using CosmoCash.Application.Exceptions;
using CosmoCash.Domain.Entities;
using CosmoCash.Domain.Enums;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Services.Interfaces;

namespace CosmoCash.API.Seed;

public static class AdminSeeder
{
  // Cria o primeiro AdminTotal a partir de ADMIN_EMAIL / ADMIN_SENHA (/ ADMIN_NOME).
  // Só roda se ainda não existir nenhum AdminTotal. Nunca sobrescreve usuário existente.
  public static async Task SemearAsync(IServiceProvider services, IConfiguration configuracao, ILogger logger)
  {
    var usuarioRepository = services.GetRequiredService<IUsuarioRepository>();
    var hashService = services.GetRequiredService<IHashService>();

    if (await usuarioRepository.ExisteTipoAsync(TipoUsuario.AdminTotal)) return;

    var email = configuracao["ADMIN_EMAIL"]?.Trim().ToLowerInvariant();
    var senha = configuracao["ADMIN_SENHA"];
    var nome = configuracao["ADMIN_NOME"]?.Trim();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
    {
      logger.LogWarning("Nenhum AdminTotal existe e ADMIN_EMAIL/ADMIN_SENHA não foram informados. Ninguém conseguirá entrar.");
      return;
    }

    // Mesma regra de senha dos demais usuários (mínimo e máximo).
    try { RegrasSenha.Validar(senha); }
    catch (ValidationException ex)
    {
      logger.LogWarning("ADMIN_SENHA inválida: {Motivo} O admin inicial não foi criado.", ex.Message);
      return;
    }

    await usuarioRepository.SalvarAsync(new Usuario
    {
      Nome = string.IsNullOrWhiteSpace(nome) ? "Administrador" : nome,
      Email = email,
      SenhaHash = hashService.GerarHash(senha),
      Tipo = TipoUsuario.AdminTotal,
      Ativo = true
    });

    logger.LogInformation("AdminTotal inicial criado ({Email}).", email);
  }
}
