using CosmoCash.Domain.Entities;
using CosmoCash.Repository.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CosmoCash.Repository.Context;

public class CosmoCashDbContext : DbContext
{
  public CosmoCashDbContext(DbContextOptions<CosmoCashDbContext> options) : base(options) { }

  public DbSet<Usuario> Usuarios { get; set; } = null!;
  public DbSet<Produto> Produtos { get; set; } = null!;
  public DbSet<Cliente> Clientes { get; set; } = null!;
  public DbSet<Venda> Vendas { get; set; } = null!;
  public DbSet<ItemVenda> ItensVenda { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
    modelBuilder.ApplyConfiguration(new ProdutoConfiguration());
    modelBuilder.ApplyConfiguration(new ClienteConfiguration());
    modelBuilder.ApplyConfiguration(new VendaConfiguration());
    modelBuilder.ApplyConfiguration(new ItemVendaConfiguration());
  }
}
