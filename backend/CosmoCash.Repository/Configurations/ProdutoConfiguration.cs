using CosmoCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmoCash.Repository.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
  public void Configure(EntityTypeBuilder<Produto> builder)
  {
    builder.ToTable("Produtos");
    builder.HasKey(produto => produto.ID);
    builder.Property(produto => produto.Marca).IsRequired(true).HasMaxLength(80);
    builder.Property(produto => produto.Descricao).IsRequired(true).HasMaxLength(200);
    builder.Property(produto => produto.VolumeMl).IsRequired(true);
    builder.Property(produto => produto.Variacao).IsRequired(true).HasMaxLength(60);
    builder.Property(produto => produto.Ativo).HasDefaultValue(true);
    builder.HasIndex(produto => produto.Marca);
  }
}
