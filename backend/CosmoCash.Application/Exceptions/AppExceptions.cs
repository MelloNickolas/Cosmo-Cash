namespace CosmoCash.Application.Exceptions;

// Erro de regra de negócio / dado inválido -> 400
public class ValidationException : Exception
{
  public ValidationException(string mensagem) : base(mensagem) { }
}

// Recurso não encontrado -> 404
public class NotFoundException : Exception
{
  public NotFoundException(string mensagem) : base(mensagem) { }
}

// Credencial inválida / sem permissão -> 401
public class UnauthorizedException : Exception
{
  public UnauthorizedException(string mensagem) : base(mensagem) { }
}
