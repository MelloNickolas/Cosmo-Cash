namespace CosmoCash.Application.DTOs.Clientes;

// Item da listagem: o CPF vai MASCARADO (LGPD). O completo só aparece ao abrir o cliente.
public class ClienteResumoResponse
{
  public int ID { get; set; }
  public string Nome { get; set; } = string.Empty;
  public string? CpfMascarado { get; set; }
  public string? Telefone { get; set; }
  public bool Ativo { get; set; }
}
