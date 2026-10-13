namespace CosmoCash.Services.Token;

public class JwtOptions
{
  public string Chave { get; set; } = string.Empty;
  public string Issuer { get; set; } = "CosmoCash";
  public string Audience { get; set; } = "CosmoCash";
  public int HorasValidade { get; set; } = 8;
}
