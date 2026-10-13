using CosmoCash.Repository.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmoCash.Repository.Context;

// Usada só pelas ferramentas do EF (dotnet ef migrations ...). Não roda em produção.
// Sem CONNECTION_STRING definida, usa um valor local qualquer: gerar migration não conecta no banco.
public class CosmoCashDbContextFactory : IDesignTimeDbContextFactory<CosmoCashDbContext>
{
  public CosmoCashDbContext CreateDbContext(string[] args)
  {
    var conexao = Environment.GetEnvironmentVariable("CONNECTION_STRING")
      ?? "Host=localhost;Database=cosmocash;Username=postgres;Password=postgres";

    var options = new DbContextOptionsBuilder<CosmoCashDbContext>()
      .UseNpgsql(ConexaoHelper.Normalizar(conexao))
      .Options;

    return new CosmoCashDbContext(options);
  }
}
