namespace CosmoCash.Application.DTOs.Clientes;

// Só o nome é obrigatório. CPF, telefone, CEP e endereço podem ser enviados com ou sem máscara
// (a API guarda apenas os dígitos).
public class ClienteRequest
{
  public string Nome { get; set; } = string.Empty;
  public string? Cpf { get; set; }
  public string? Telefone { get; set; }
  public string? Cep { get; set; }
  public string? Endereco { get; set; }
}
