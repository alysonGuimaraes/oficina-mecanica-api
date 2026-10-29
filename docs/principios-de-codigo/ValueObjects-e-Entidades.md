# Value Objects e Entidades

## Objetivo

Garantir a integridade do domínio através de **Value Objects (VOs)** imutáveis e **Entidades** bem definidas, respeitando os princípios do DDD e mantendo o domínio puro, sem dependências de frameworks.

## Diferença entre Entidade e Value Object

| Aspecto | Entidade | Value Object |
|---|---|---|
| **Identidade** | Possui identidade única (geralmente `Id`). Duas entidades são iguais apenas se tiverem o mesmo `Id`. | Não possui identidade. É definido unicamente pelo seu valor. Dois VOs com mesmos valores são considerados iguais. |
| **Mutabilidade** | Pode ser mutável, mas deve expor **comportamentos** ao invés de setters. Alterações devem respeitar regras de negócio. | **Imutável**. Após criado, seu valor nunca deve ser alterado. |
| **Validação** | Validações de ciclo de vida e transições de estado. | Validações de formato, consistência e regras próprias no **construtor**. |
| **Persistência** | Mapeado como entidade no EF Core. | Geralmente mapeado como **Owned Entity Type** ou convertido para valor primitivo no banco. |

## Value Objects

### Regras

- **Imutáveis:** Utilizar `record` ou propriedades `init`/`readonly`. Preferencialmente `record` para simplificar igualdade por valor.
- **Auto-validáveis:** Toda validação obrigatória deve ocorrer no construtor/fábrica. Nunca permitir um VO em estado inválido.
- **Sem dependências:** Não podem ter referências a entidades, repositórios ou frameworks.
- **Sem identidade:** Nunca devem possuir um `Id`.
- **Sem efeitos colaterais:** Métodos devem ser puros e retornar novos VOs quando houver transformação.
- **Localização:** Devem ficar em `AutoGarage.Domain/<Aggregate>/ValueObjects/` ou `AutoGarage.Domain/Common/ValueObjects/` caso sejam compartilhados.

### Exemplos

```csharp
// AutoGarage.Domain/Customers/ValueObjects/Document.cs
public sealed record Document
{
    public string Value { get; }

    public Document(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDocumentException(value);

        var cleaned = value.Replace(".", "").Replace("-", "").Replace("/", "");

        if (!IsValid(cleaned))
            throw new InvalidDocumentException(value);

        Value = cleaned;
    }

    private static bool IsValid(string document)
    {
        return document.Length is 11 or 14 && HasValidCheckDigits(document);
    }

    private static bool HasValidCheckDigits(string document) => /* lógica de validação CPF/CNPJ */ true;

    public override string ToString() => Value;
}

// AutoGarage.Domain/Vehicles/ValueObjects/Plate.cs
public sealed record Plate
{
    public string Value { get; }

    public Plate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPlateException(value);

        var normalized = value.ToUpperInvariant().Replace("-", "");
        
        if (!Regex.IsMatch(normalized, @"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$"))
            throw new InvalidPlateException(value);

        Value = normalized;
    }
}
```

## Entidades

### Regras

- **Identidade:** Devem possuir um identificador único (`Id`). Pode ser `long`, `Guid` ou `Value Object` de ID.
- **Comportamento sobre dados:** Expor **métodos com intenção** (`Activate()`, `Deactivate()`, `AddItem()`) ao invés de `SetX()`.
- **Invariantes protegidas:** Regras que devem sempre ser verdadeiras devem ser garantidas pelos próprios métodos da entidade.
  > **Decisão:** a entidade protege apenas invariantes de estado e transição (ex: role válido, não ativar quem já está ativo). Campos obrigatórios e formato de nome e e-mail são validados na Application. Ver [Arquitetura - Decisões de arquitetura](../arquitetura/README.md#decisoes-de-arquitetura), item 5.
- **Sem anotações de infraestrutura:** Nenhuma anotação do EF Core ou ASP.NET Core no `Domain`.
- **Construtor controlado:** Preferir construtor privado + **Factory Method** (`Create`) para criação de novas instâncias.
- **Localização:** Devem ficar em `AutoGarage.Domain/<Aggregate>/Entities/` ou diretamente em `AutoGarage.Domain/<Aggregate>/` caso seja Aggregate Root.
- **Nada de deleção física:** Conforme regra do projeto, entidades nunca devem ser removidas fisicamente. Utilizar inativação (`EntityStatus.INACTIVE`).

### Exemplo

```csharp
// AutoGarage.Domain/Customers/Customer.cs
public class Customer
{
    public long Id { get; private set; }
    public Document Document { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public EntityStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Customer() { }

    private Customer(Document document, string name, string email, string phone)
    {
        Document = document;
        Name = name;
        Email = email;
        Phone = phone;
        Status = EntityStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Customer Create(Document document, string name, string email, string phone)
    {
        return new Customer(document, name, email, phone);
    }

    public void UpdateContact(string name, string email, string phone)
    {
        if (Status == EntityStatus.INACTIVE)
            throw new CustomerAlreadyInactiveException(Id);

        Name = name;
        Email = email;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == EntityStatus.INACTIVE)
            throw new CustomerAlreadyInactiveException(Id);

        Status = EntityStatus.INACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == EntityStatus.ACTIVE)
            throw new CustomerAlreadyActiveException(Id);

        Status = EntityStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

## Aggregate Roots

- **Um Aggregate Root** é a única porta de entrada para modificar entidades dentro do agregado.
- Referências entre agregados devem ser feitas **apenas por ID** (`long CustomerId`), nunca por navegação direta.
- Toda operação que cruza fronteira de agregado deve passar pelo **Aggregate Root** correspondente.
- O `ServiceOrder` é o exemplo central no projeto, controlando `ServiceOrderService`, `ServiceOrderMaterial` e `ServiceOrderHistory`.

## Diretrizes Práticas

- **Records para VOs:** Sempre que possível utilizar `sealed record` para garantir imutabilidade e igualdade semântico.
- **EF Core mapping:** VOs podem ser mapeados com `OwnsOne` no `DbContext`. Nunca expor setters públicos para facilitar o mapeamento.
- **Testabilidade:** Regras de VO e Entidade devem ser testadas com **testes unitários** no projeto `Domain`, sem necessidade de banco ou HTTP.
- **Sem null desnecessário:** Utilizar tipos nullable com critério (`string?`) apenas quando realmente opcional no negócio.
- **Fail Fast:** Validar o mais cedo possível. Um VO inválido nunca deve existir em memória.
