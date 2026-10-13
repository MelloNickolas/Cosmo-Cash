using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CosmoCash.API.Middlewares;
using CosmoCash.API.Seed;
using CosmoCash.Application.Applications;
using CosmoCash.Application.Interfaces;
using CosmoCash.Repository.Context;
using CosmoCash.Repository.Helpers;
using CosmoCash.Repository.Interfaces;
using CosmoCash.Repository.Repositories;
using CosmoCash.Services.Hash;
using CosmoCash.Services.Interfaces;
using CosmoCash.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// .env local (desenvolvimento). Em produção as variáveis vêm do ambiente do host.
try { DotNetEnv.Env.TraversePath().Load(); } catch { }

var builder = WebApplication.CreateBuilder(args);

#region Configuração obrigatória
var conexao = ConexaoHelper.Normalizar(builder.Configuration["CONNECTION_STRING"]);

var jwtChave = builder.Configuration["JWT_KEY"];
if (string.IsNullOrWhiteSpace(jwtChave) || jwtChave.Length < 32)
  throw new Exception("JWT_KEY não configurada ou curta demais (mínimo 32 caracteres).");

var jwtOptions = new JwtOptions { Chave = jwtChave };
#endregion

#region Banco
builder.Services.AddDbContext<CosmoCashDbContext>(options => options.UseNpgsql(conexao));
#endregion

#region Injeção de dependência
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVendaRepository, VendaRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

builder.Services.AddScoped<IAuthApplication, AuthApplication>();
builder.Services.AddScoped<IUsuarioApplication, UsuarioApplication>();
builder.Services.AddScoped<IProdutoApplication, ProdutoApplication>();
builder.Services.AddScoped<IClienteApplication, ClienteApplication>();
builder.Services.AddScoped<IVendaApplication, VendaApplication>();
builder.Services.AddScoped<IDashboardApplication, DashboardApplication>();
#endregion

#region CORS
// CORS_ORIGINS (plural), separadas por vírgula.
var origens = (builder.Configuration["CORS_ORIGINS"] ?? "http://localhost:5173")
  .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
  options.AddDefaultPolicy(policy => policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod()));
#endregion

#region Autenticação JWT
builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Chave)),
      ValidateIssuer = true,
      ValidIssuer = jwtOptions.Issuer,
      ValidateAudience = true,
      ValidAudience = jwtOptions.Audience,
      ValidateLifetime = true,
      ClockSkew = TimeSpan.FromMinutes(1),
      NameClaimType = "name",
      RoleClaimType = "role"
    };

    // O JWT sozinho não sabe que o usuário foi desativado ou rebaixado depois de logar.
    // A cada requisição conferimos no banco: ativo e com o mesmo papel do token.
    options.Events = new JwtBearerEvents
    {
      OnTokenValidated = async contexto =>
      {
        var repositorio = contexto.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
        var sub = contexto.Principal?.FindFirst("sub")?.Value;
        var papel = contexto.Principal?.FindFirst("role")?.Value;

        if (!int.TryParse(sub, out var usuarioID))
        {
          contexto.Fail("Token inválido.");
          return;
        }

        var usuario = await repositorio.ObterPorIdAsync(usuarioID);
        if (usuario == null || !usuario.Ativo || usuario.Tipo.ToString() != papel)
          contexto.Fail("Sessão expirada. Entre novamente.");
      }
    };
  });

builder.Services.AddAuthorization();
#endregion

#region Rate limit (proteção contra força bruta no login)
builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
  options.AddPolicy("login", contexto => RateLimitPartition.GetFixedWindowLimiter(
    contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
    _ => new FixedWindowRateLimiterOptions
    {
      PermitLimit = 10,
      Window = TimeSpan.FromMinutes(1),
      QueueLimit = 0
    }));
});
#endregion

builder.Services
  .AddControllers()
  .AddJsonOptions(options =>
  {
    // Enums trafegam como texto ("Socio", "Pix", "Concluida"); no banco continuam inteiros.
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  })
  .ConfigureApiBehaviorOptions(options =>
  {
    // JSON malformado ou enum desconhecido: mesmo formato { "mensagem": "..." } dos demais erros.
    options.InvalidModelStateResponseFactory = contexto =>
    {
      var erros = contexto.ModelState.Values.SelectMany(valor => valor.Errors).ToList();

      // Quando o corpo não deserializa, o motivo real está na exceção do JSON (o ErrorMessage
      // só diz "The request field is required"). Traduzimos para algo que o usuário entende.
      // Falha ao ler o JSON: o ASP.NET registra o erro numa chave com o caminho do campo ("$.tipo"),
      // com a exceção embrulhada (InputFormatterException). A chave "request" só diz "campo obrigatório".
      var entradaJson = contexto.ModelState
        .Where(par => par.Key.StartsWith('$') && par.Value!.Errors.Count > 0)
        .Select(par => par)
        .FirstOrDefault();

      if (entradaJson.Value != null)
      {
        var campo = entradaJson.Key.TrimStart('$', '.');
        var detalhe = string.Join(" ", entradaJson.Value.Errors.SelectMany(e =>
          CadeiaDeExcecoes(e.Exception).Select(x => x.Message).Append(e.ErrorMessage)));
        var valorInvalido = detalhe.Contains("could not be converted", StringComparison.OrdinalIgnoreCase);

        var textoJson = valorInvalido && !string.IsNullOrWhiteSpace(campo)
          ? $"Valor inválido no campo '{campo}'."
          : "O corpo da requisição não é um JSON válido.";

        return new BadRequestObjectResult(new { mensagem = textoJson });
      }

      // Parâmetro de rota/query com valor que não converte (ex.: dataInicio=abc): o framework
      // responde "The value 'abc' is not valid." em inglês; apontamos o parâmetro em português.
      var parametroInvalido = contexto.ModelState
        .FirstOrDefault(par => par.Value!.Errors.Any(e =>
          e.ErrorMessage.Contains("is not valid", StringComparison.OrdinalIgnoreCase)));

      if (parametroInvalido.Value != null && !string.IsNullOrWhiteSpace(parametroInvalido.Key))
        return new BadRequestObjectResult(new { mensagem = $"Valor inválido no parâmetro '{parametroInvalido.Key}'." });

      // Mensagens padrão do framework vêm em inglês; traduzimos as que podem aparecer.
      var erro = erros.Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
      var textoErro = erro switch
      {
        null => "Requisição inválida.",
        _ when erro.Contains("non-empty request body", StringComparison.OrdinalIgnoreCase)
          => "O corpo da requisição não pode ser vazio.",
        _ when erro.EndsWith("field is required.", StringComparison.OrdinalIgnoreCase)
          => "Os dados enviados são inválidos ou estão incompletos.",
        _ => erro
      };

      return new BadRequestObjectResult(new { mensagem = textoErro });
    };
  });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cosmo Cash API", Version = "v1" });
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Cole apenas o token JWT."
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
      Array.Empty<string>()
    }
  });
});

var app = builder.Build();

#region Startup: migrations + seed do admin
using (var escopo = app.Services.CreateScope())
{
  var logger = escopo.ServiceProvider.GetRequiredService<ILogger<Program>>();
  var db = escopo.ServiceProvider.GetRequiredService<CosmoCashDbContext>();
  db.Database.Migrate();
  await AdminSeeder.SemearAsync(escopo.ServiceProvider, builder.Configuration, logger);
}
#endregion

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Keep-alive (UptimeRobot): sem acesso ao banco.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static IEnumerable<Exception> CadeiaDeExcecoes(Exception? excecao)
{
  for (var atual = excecao; atual != null; atual = atual.InnerException)
    yield return atual;
}
