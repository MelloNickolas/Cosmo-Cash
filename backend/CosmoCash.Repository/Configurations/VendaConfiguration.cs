using CosmoCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmoCash.Repository.Configurations;

public class VendaConfiguration : IEntityTypeConfiguration<Venda>
{
  public void Configure(EntityTypeBuilder<Venda> builder)
  {
    builder.ToTable("Vendas");
    builder.HasKey(venda => venda.ID);
    builder.Property(venda => venda.DataVenda).IsRequired(true);
    builder.Property(venda => venda.FormaPagamento).IsRequired(true);
    builder.Property(venda => venda.Status).IsRequired(true);
    builder.Property(venda => venda.CanceladaEm).IsRequired(false);

    // Cliente é opcional; nada é apagado de verdade (soft delete), então Restrict.
    builder.HasOne(venda => venda.Cliente)
      .WithMany()
      .HasForeignKey(venda => venda.ClienteID)
      .IsRequired(false)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(venda => venda.Usuario)
      .WithMany()
      .HasForeignKey(venda => venda.UsuarioID)
      .IsRequired(true)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(venda => venda.DataVenda);
    builder.HasIndex(venda => venda.Status);
  }
}
