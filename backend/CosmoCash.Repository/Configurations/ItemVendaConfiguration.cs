using CosmoCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmoCash.Repository.Configurations;

public class ItemVendaConfiguration : IEntityTypeConfiguration<ItemVenda>
{
  public void Configure(EntityTypeBuilder<ItemVenda> builder)
  {
    builder.ToTable("ItensVenda", tabela =>
    {
      tabela.HasCheckConstraint("CK_ItensVenda_Quantidade", "\"Quantidade\" > 0");
      tabela.HasCheckConstraint("CK_ItensVenda_Precos", "\"PrecoVenda\" >= 0 AND \"PrecoCusto\" >= 0");
    });
    builder.HasKey(item => item.ID);
    builder.Property(item => item.Quantidade).IsRequired(true);
    builder.Property(item => item.PrecoVenda).IsRequired(true).HasPrecision(18, 2);
    builder.Property(item => item.PrecoCusto).IsRequired(true).HasPrecision(18, 2);

    builder.HasOne(item => item.Venda)
      .WithMany(venda => venda.Itens)
      .HasForeignKey(item => item.VendaID)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(item => item.Produto)
      .WithMany()
      .HasForeignKey(item => item.ProdutoID)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(item => item.VendaID);
    builder.HasIndex(item => item.ProdutoID);
  }
}
