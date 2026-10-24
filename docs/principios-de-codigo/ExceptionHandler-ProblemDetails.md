# Exception Handler - Padrão Problem Details (RFC 9457)

## Objetivo

Padronizar o tratamento de erros da API utilizando o formato **Problem Details** conforme [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457), garantindo respostas consistentes, sem expor detalhes internos da aplicação e com códigos HTTP corretos.

## Onde deve ficar

| Camada | Responsabilidade |
|---|---|
| **Domain** | Definição das exceções de negócio (regras centrais das entidades/agregados). |
| **Application** | Lançamento de exceções relacionadas a validações, regras de orquestração ou violações dentro dos Use Cases. |
| **Api** | Implementação do **ExceptionHandler global** responsável por converter exceções em respostas `application/problem+json`. |

## Estrutura de Resposta

Todas as respostas de erro devem seguir o padrão **Problem Details**:

```json
{
  "type": "https://api.autogarage.com/errors/invalid-document",
  "title": "Documento inválido",
  "status": 400,
  "detail": "O CPF ou CNPJ informado não possui um formato válido.",
  "instance": "/api/customers"
}
```

| Campo | Obrigatório | Descrição |
|---|---|---|
| `type` | Sim | URI que identifica o tipo do problema. Pode apontar para documentação ou usar um URI relativo/identificador único. |
| `title` | Sim | Resumo curto e legível do tipo de problema. |
| `status` | Sim | Código HTTP retornado. Deve ser o mesmo do response. |
| `detail` | Sim | Explicação específica para esta ocorrência do problema. |
| `instance` | Opcional | URI que identifica a ocorrência específica (geralmente o path da requisição). |

## Classificação das Exceções

| Origem | Quando utilizar | Exemplo |
|---|---|---|
| **Domain** | Violação de regra invariante da entidade/agregado. São regras que, se quebradas, tornam o objeto inválido. | `InvalidDocumentException`, `InvalidPlateException`, `CustomerAlreadyInactiveException` |
| **Application** | Violações nas validações de negócio dos Use Cases, regras de fluxo ou quando não há uma entidade/agregado específico para representar a violação. | `DuplicateDocumentException`, `CustomerNotFoundException`, `ServiceOrderNotInValidStatusException`, `InsufficientStockException` |

> **Regra:** Exceções de domínio devem permanecer puras (sem dependência de HTTP). A camada `Api` é a única responsável por mapear essas exceções para status codes e para o formato Problem Details.

## Mapeamento de Exceções para Status Code

| Exceção | Status HTTP | Motivo |
|---|---|---|
| `NotFoundException` (base ou específicas) | `404 Not Found` | Recurso não encontrado |
| `BadRequestException` / exceções de validação/invariante inválida | `400 Bad Request` | Requisição mal formada ou dados inválidos |
| `ConflictException` / duplicidade | `409 Conflict` | Conflito com estado atual (ex: documento já cadastrado) |
| `UnauthorizedException` | `401 Unauthorized` | Não autenticado |
| `ForbiddenException` | `403 Forbidden` | Autenticado mas sem permissão |
| `UnprocessableEntityException` | `422 Unprocessable Entity` | Requisição semanticamente correta, mas viola regra de negócio |
| `InvalidOperationException` customizada | `422 Unprocessable Entity` ou `409 Conflict` | Depende da regra violada |
| `Exception` (genérica) | `500 Internal Server Error` | Erro inesperado. **Nunca expor stack trace ou detalhes sensíveis.** |

## Implementação

### 1. Exceções Base

Todas as exceções customizadas devem herdar de uma base para facilitar o mapeamento no `ExceptionHandler`.

**Local:** `AutoGarage.Domain/Common/Exceptions/` ou `AutoGarage.Application/Common/Exceptions/`

```csharp
// AutoGarage.Domain/Common/Exceptions/DomainException.cs
public abstract class DomainException : Exception
{
    public string ErrorType { get; }
    
    protected DomainException(string errorType, string message) : base(message)
    {
        ErrorType = errorType;
    }
}

// AutoGarage.Application/Common/Exceptions/ApplicationException.cs
public abstract class ApplicationException : Exception
{
    public string ErrorType { get; }
    
    protected ApplicationException(string errorType, string message) : base(message)
    {
        ErrorType = errorType;
    }
}

// AutoGarage.Application/Common/Exceptions/NotFoundException.cs
public abstract class NotFoundException : ApplicationException
{
    protected NotFoundException(string errorType, string message) 
        : base(errorType, message) { }
}

// AutoGarage.Application/Common/Exceptions/ConflictException.cs
public abstract class ConflictException : ApplicationException
{
    protected ConflictException(string errorType, string message) 
        : base(errorType, message) { }
}

// AutoGarage.Application/Common/Exceptions/BadRequestException.cs
public abstract class BadRequestException : ApplicationException
{
    protected BadRequestException(string errorType, string message) 
        : base(errorType, message) { }
}
```

### 2. Exceções Específicas

**Exemplo no Domain:**

```csharp
// AutoGarage.Domain/Customers/Exceptions/InvalidDocumentException.cs
public class InvalidDocumentException : DomainException
{
    public InvalidDocumentException(string document) 
        : base("invalid-document", $"O documento '{document}' é inválido.") { }
}
```

**Exemplo na Application:**

```csharp
// AutoGarage.Application/Customers/Exceptions/CustomerNotFoundException.cs
public class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException(long id) 
        : base("customer-not-found", $"Cliente com id '{id}' não encontrado.") { }
    
    public CustomerNotFoundException(string document) 
        : base("customer-not-found", $"Cliente com documento '{document}' não encontrado.") { }
}

// AutoGarage.Application/Customers/Exceptions/DuplicateDocumentException.cs
public class DuplicateDocumentException : ConflictException
{
    public DuplicateDocumentException(string document) 
        : base("duplicate-document", $"Já existe um cliente cadastrado com o documento '{document}'.") { }
}
```

### 3. Global Exception Handler na API

**Local:** `AutoGarage.Api/Middlewares/ExceptionHandlingMiddleware.cs` ou `AutoGarage.Api/Extensions/ExceptionHandlerExtensions.cs`

Usando **Problem Details** nativo do ASP.NET Core (`ProblemDetails`, `IExceptionHandler` ou `ExceptionHandlerOptions`).

```csharp
// AutoGarage.Api/Extensions/ExceptionHandlerExtensions.cs
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionApp =>
        {
            exceptionApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                if (exception == null) return;

                var (statusCode, type, title) = exception switch
                {
                    NotFoundException => ((int)HttpStatusCode.NotFound, 
                        $"https://api.autogarage.com/errors/{exception.GetType().Name.Replace("Exception", "").ToLowerInvariant()}",
                        "Recurso não encontrado"),
                    
                    ConflictException => ((int)HttpStatusCode.Conflict,
                        $"https://api.autogarage.com/errors/{exception.GetType().Name.Replace("Exception", "").ToLowerInvariant()}",
                        "Conflito"),
                    
                    BadRequestException or DomainException => ((int)HttpStatusCode.BadRequest,
                        $"https://api.autogarage.com/errors/{exception.GetType().Name.Replace("Exception", "").ToLowerInvariant()}",
                        "Requisição inválida"),
                    
                    ApplicationException => ((int)HttpStatusCode.UnprocessableEntity,
                        $"https://api.autogarage.com/errors/{exception.GetType().Name.Replace("Exception", "").ToLowerInvariant()}",
                        "Regra de negócio violada"),
                    
                    _ => ((int)HttpStatusCode.InternalServerError,
                        "https://api.autogarage.com/errors/internal-server-error",
                        "Erro interno do servidor")
                };

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = statusCode;

                var problemDetails = new ProblemDetails
                {
                    Type = type,
                    Title = title,
                    Status = statusCode,
                    Detail = exception.Message,
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonOf<ProblemDetails>(problemDetails);
            });
        });

        return app;
    }
}
```

> **Dica:** Também é possível utilizar `IExceptionHandler` introduzido no ASP.NET Core 8+, que fica ainda mais limpo e melhor para testes.

## Regras a Seguir

- **Domínio puro:** Exceções do `Domain` não devem conhecer HTTP, `ProblemDetails` ou qualquer tipo da camada `Api`.
- **Use Cases limpos:** Não utilizar `try/catch` desnecessário nos Use Cases. Apenas lançar as exceções quando a regra for violada.
- **Único handler:** Todo erro deve passar pelo **Global Exception Handler** da camada `Api`. Não criar handlers individuais por controller.
- **Sem vazamento de informações:** Para exceções genéricas (`Exception`) nunca retornar stack trace. Apenas `title`, `detail` genérico e `type` padrão.
- **Detalhes contextuais:** Quando fizer sentido, incluir informações da ocorrência no `detail` (como id ou documento), mas sempre sem dados sensíveis.
- **Consistência:** Sempre utilizar `application/problem+json` como `Content-Type`.
- **RFC 9457:** Seguir estritamente os campos `type`, `title`, `status`, `detail` e `instance`.
