using System.Net;
using System.Text.Json;
using CosmoCash.Application.Exceptions;

namespace CosmoCash.API.Middlewares;

public class ErrorHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ErrorHandlingMiddleware> _logger;

  public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (ValidationException ex)
    {
      await Responder(context, HttpStatusCode.BadRequest, ex.Message);
    }
    catch (NotFoundException ex)
    {
      await Responder(context, HttpStatusCode.NotFound, ex.Message);
    }
    catch (UnauthorizedException ex)
    {
      await Responder(context, HttpStatusCode.Unauthorized, ex.Message);
    }
    catch (Exception ex)
    {
      // Erro inesperado: registra o detalhe no log, mas não vaza nada para o cliente.
      _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
      await Responder(context, HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado. Tente novamente.");
    }
  }

  private static async Task Responder(HttpContext context, HttpStatusCode status, string mensagem)
  {
    if (context.Response.HasStarted) return;

    context.Response.StatusCode = (int)status;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new { mensagem }));
  }
}
