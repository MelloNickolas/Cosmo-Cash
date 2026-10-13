namespace CosmoCash.Application.DTOs.Clientes;

// Detalhe de um cliente: aqui o CPF vai completo (só dígitos).
public class ClienteResponse
{
  public int ID { get; set; }
  public string Nome { get; set; } = string.Empty;
  public string? Cpf { get; set; }
  public string? Telefone { get; set; }
  public string? Cep { get; set; }
  public string? Endereco { get; set; }
  public bool Ativo { get; set; }
  public DateTime CriadoEm { get; set; }
}
