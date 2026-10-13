using CosmoCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmoCash.Repository.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
  public void Configure(EntityTypeBuilder<Usuario> builder)
  {
    builder.ToTable("Usuarios");
    builder.HasKey(usuario => usuario.ID);
    builder.Property(usuario => usuario.Nome).IsRequired(true).HasMaxLength(120);
    builder.Property(usuario => usuario.Email).IsRequired(true).HasMaxLength(200);
    builder.Property(usuario => usuario.SenhaHash).IsRequired(true).HasMaxLength(100);
    builder.Property(usuario => usuario.Tipo).IsRequired(true);
    builder.Property(usuario => usuario.Ativo).HasDefaultValue(true);
    builder.HasIndex(usuario => usuario.Email).IsUnique();
  }
}
