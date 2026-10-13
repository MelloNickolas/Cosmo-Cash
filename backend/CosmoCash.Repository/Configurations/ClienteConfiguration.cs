using CosmoCash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmoCash.Repository.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
  public void Configure(EntityTypeBuilder<Cliente> builder)
  {
    builder.ToTable("Clientes");
    builder.HasKey(cliente => cliente.ID);
    builder.Property(cliente => cliente.Nome).IsRequired(true).HasMaxLength(120);
    // CPF e CEP são guardados só com dígitos (a Application normaliza).
    builder.Property(cliente => cliente.Cpf).IsRequired(false).HasMaxLength(11);
    builder.Property(cliente => cliente.Telefone).IsRequired(false).HasMaxLength(20);
    builder.Property(cliente => cliente.Cep).IsRequired(false).HasMaxLength(8);
    builder.Property(cliente => cliente.Endereco).IsRequired(false).HasMaxLength(300);
    builder.Property(cliente => cliente.Ativo).HasDefaultValue(true);
    builder.HasIndex(cliente => cliente.Cpf).IsUnique();
    builder.HasIndex(cliente => cliente.Nome);
  }
}
