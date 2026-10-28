# Exception Handler - Padrão Problem Details (RFC 9457)

## Objetivo

Padronizar o tratamento de erros da API utilizando o formato **Problem Details** conforme [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457), garantindo respostas consistentes, sem expor detalhes internos da aplicação e com códigos HTTP corretos.

## Onde deve ficar

| Camada | Responsabilidade |
|---|---|
| **Domain** | Definição das exceções de negócio e das exceções base que representam a categoria HTTP (`NotFoundDomainException`, `ConflictDomainException`, `BadRequestDomainException`). |
| **Application** | Lançamento de exceções relacionadas a validações e regras de orquestração dentro dos Use Cases. |
| **Api** | Implementação do **ExceptionHandler global** (`AutoGarage.Api/Exceptions/ExceptionHandler.cs`), responsável por converter exceções em respostas `application/problem+json`. |

> **Decisão:** `ErrorOnValidationException` fica em `AutoGarage.Domain/Common/Exceptions/`, por legibilidade. Ver [Arquitetura - Decisões de arquitetura](../arquitetura/README.md#decisoes-de-arquitetura), item 7.

## Estrutura de Resposta

Todas as respostas de erro devem seguir o padrão **Problem Details**:

```json
{
  "type": "https://api.autogarage.com/errors/duplicate-email",
  "title": "duplicate-email",
  "status": 409,
  "detail": "The email 'john@autogarage.com' is already registered in the system.",
  "instance": "/api/employees",
  "traceId": "0HN7..."
}
```

| Campo | Obrigatório | Descrição |
|---|---|---|
| `type` | Sim | URI do tipo do problema: base `https://api.autogarage.com/errors/` + `ErrorType` da exceção. |
| `title` | Sim | Para exceções de domínio, o próprio `ErrorType`. Para validação e erros inesperados, um título fixo em inglês. |
| `status` | Sim | Código HTTP retornado. Deve ser o mesmo do response. |
| `detail` | Sim | Mensagem da exceção (domínio) ou texto genérico (erro inesperado). |
| `instance` | Opcional | Path da requisição. |
| `traceId` | Não padronizado | Extensão adicionada em todas as respostas de domínio e de erro inesperado, para correlacionar com logs. |

## Classificação das Exceções

| Origem | Quando utilizar | Exemplo atual |
|---|---|---|
| **Domain (específica)** | Violação de regra do negócio ou de uma entidade. Herda de uma exceção base de categoria HTTP. | `EmployeeNotFoundException`, `DuplicateEmailException`, `InvalidEmployeeRoleException` |
| **Domain (base de categoria)** | Define a categoria HTTP. Não é lançada diretamente, apenas herdada. | `NotFoundDomainException`, `ConflictDomainException`, `BadRequestDomainException` |
| **Validação** | Falhas de validação de entrada (FluentValidation) agregadas em uma lista de mensagens. | `ErrorOnValidationException` |

> **Regra:** Exceções de domínio devem permanecer puras (sem dependência de HTTP). A camada `Api` é a única responsável por mapear essas exceções para status codes e para o formato Problem Details.

## Mapeamento de Exceções para Status Code

O mapeamento é feito em `ExceptionHandler.ResolveDomainErrorStatusCode` pelo **tipo base**. Uma exceção específica nova herda o status da sua base sem alterar o handler.

| Exceção (ou base) | Status HTTP | Situação atual |
|---|---|---|
| `NotFoundDomainException` (e específicas) | `404 Not Found` | Implementado |
| `ConflictDomainException` (e específicas) | `409 Conflict` | Implementado |
| `BadRequestDomainException` (e específicas) | `400 Bad Request` | Implementado |
| `ErrorOnValidationException` | `400 Bad Request` (com `invalid-params`) | Implementado |
| `DomainException` sem base de categoria | `422 Unprocessable Entity` | Fallback implementado |
| Qualquer outra exceção | `500 Internal Server Error` | Implementado. **Nunca expor stack trace ou detalhes sensíveis.** |
| `UnauthorizedException` / `ForbiddenException` | `401` / `403` | Previsto, depende da autenticação (ainda não implementada) |

## Implementação

### 1. Exceções Base

Todas as exceções de domínio herdam de `DomainException`, que recebe `message` e `errorType` nesta ordem. As exceções de categoria HTTP herdam dela.

**Local:** `AutoGarage.Domain/Common/Exceptions/`

```csharp
// AutoGarage.Domain/Common/Exceptions/DomainException.cs
public abstract class DomainException : AutoGarageException
{
    public string ErrorType { get; }

    protected DomainException(string message, string errorType) : base(message)
    {
        ErrorType = errorType;
    }
}

// AutoGarage.Domain/Common/Exceptions/NotFoundDomainException.cs
public abstract class NotFoundDomainException : DomainException
{
    protected NotFoundDomainException(string message, string errorType) : base(message, errorType) { }
}

// ConflictDomainException e BadRequestDomainException seguem o mesmo formato.
```

### 2. Exceções Específicas

```csharp
// AutoGarage.Domain/Exceptions/Employees/Exceptions/EmployeeNotFoundException.cs
public class EmployeeNotFoundException : NotFoundDomainException
{
    public EmployeeNotFoundException(Guid id)
        : base($"Employee with id '{id}' was not found.", "employee-not-found") { }
}

// AutoGarage.Domain/Exceptions/Employees/Exceptions/DuplicateEmailException.cs
public class DuplicateEmailException : ConflictDomainException
{
    public DuplicateEmailException(string email)
        : base($"The email '{email}' is already registered in the system.", "duplicate-email") { }
}
```

Ao criar uma nova exceção, escolha a base que corresponde ao status HTTP desejado. O `ErrorType` é um identificador em kebab-case, único por tipo de problema.

### 3. Global Exception Handler na API

**Local:** `AutoGarage.Api/Exceptions/ExceptionHandler.cs`, implementado com `IExceptionHandler` e registrado em `Program.cs` via `AddExceptionHandler<ExceptionHandler>()`.

```csharp
private static int ResolveDomainErrorStatusCode(DomainException domainException) => domainException switch
{
    NotFoundDomainException => StatusCodes.Status404NotFound,
    ConflictDomainException => StatusCodes.Status409Conflict,
    BadRequestDomainException => StatusCodes.Status400BadRequest,
    _ => StatusCodes.Status422UnprocessableEntity
};
```

O handler delega para `BuildDomainError`, `BuildOnValidationException` ou `BuildUnknownError` conforme o tipo da exceção, e grava o resultado com `IProblemDetailsService`.

## Regras a Seguir

- **Domínio puro:** Exceções do `Domain` não devem conhecer HTTP, `ProblemDetails` ou qualquer tipo da camada `Api`. O status HTTP é decidido apenas no handler.
- **Categoria por base:** Não criar `if` por nome de exceção no handler. Escolha a base de categoria.
- **Use Cases limpos:** Não utilizar `try/catch` desnecessário nos Use Cases. Apenas lançar as exceções quando a regra for violada.
- **Único handler:** Todo erro deve passar pelo **Global Exception Handler** da camada `Api`. Não criar handlers individuais por controller.
- **Sem vazamento de informações:** Para exceções inesperadas, nunca retornar stack trace nem a mensagem original. Apenas `title`, `detail` genérico e `type` padrão.
- **Detalhes contextuais:** Incluir informações da ocorrência no `detail` (como id ou e-mail), mas sem dados sensíveis (senhas, hashes, tokens).
- **Consistência:** Sempre utilizar `application/problem+json` como `Content-Type`.
- **RFC 9457:** Seguir os campos `type`, `title`, `status`, `detail` e `instance`.

> **Pendente:** o handler de erros inesperados ainda não registra a exceção em log. Ver [Arquitetura - Decisões de arquitetura](../arquitetura/README.md#decisoes-de-arquitetura).
