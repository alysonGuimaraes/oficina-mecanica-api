# Princípios de Código

O projeto **AutoGarage** adota os princípios de **DDD + Clean Architecture** (conforme definido em [`docs/arquitetura/README.md`](../arquitetura/README.md)) para garantir um código escalável, testável, legível e fácil de manter. 

## DDD (Domain-Driven Design)

O **Domain-Driven Design** é o alicerce da aplicação. Focamos em modelar o negócio da oficina mecânica utilizando uma linguagem ubíqua compartilhada entre desenvolvedores e domínio.

### Como aplicar

- **Foque no domínio:** Regras de negócio devem viver no `Domain`, não nos Controllers ou Services genéricos.
- **Linguagem Ubíqua:** Utilize termos do negócio (ex: `ServiceOrder`, `Customer`, `Vehicle`, `Material`) no código.
- **Aggregates:** Agrupe entidades relacionadas sob um **Aggregate Root** (ex: `ServiceOrder` controla `ServiceOrderService` e `ServiceOrderMaterial`).
- **Value Objects:** Utilize VOs para conceitos sem identidade, mas com regras próprias (ex: `Document`, `Plate`).
- **Bounded Contexts:** Respeite os limites entre contextos, evitando acoplamento desnecessário entre agregados.

**Exemplo:**

```csharp
// Bom - Regra de negócio vive na entidade
public class Customer
{
    public void Deactivate()
    {
        if (Status == EntityStatus.Inactive)
            throw new CustomerAlreadyInactiveException(Id);
            
        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
}

// Ruim - Regra de negócio no Controller/Service
[HttpPatch("{id}/deactivate")]
public IActionResult Deactivate(long id)
{
    var customer = _repository.GetById(id);
    if (customer.Status == EntityStatus.Inactive)
        return BadRequest("Cliente já está inativo"); // Regra no lugar errado
    customer.Status = EntityStatus.Inactive;
    _repository.Update(customer);
    return Ok();
}
```

## DRY (Don't Repeat Yourself)

**Não repita código.** Se uma lógica, validação ou mapeamento aparece em mais de um lugar, ela deve ser centralizada e reutilizada.

### Como aplicar

- **Value Objects reutilizáveis:** Crie VOs para conceitos repetidos (ex: `Document`, `Email`, `Money`).
- **Extensões e Helpers:** Utilize métodos de extensão para lógica comum e sem estado.
- **Mapeamento centralizado:** Utilize `Mapster` para evitar mapeamentos manuais repetitivos.
- **Exceções base:** Reutilize exceções base (`DomainException`, `ApplicationException`) ao invés de criar exceções genéricas.
- **Validadores compartilhados:** Centralize regras de validação com `FluentValidation`.

**Exemplo:**

```csharp
// Bom - Reutilizando Value Object
public class Customer
{
    public Document Document { get; private set; }
}

// Ruim - Validando CPF/CNPJ em vários lugares diferentes
if (!Regex.IsMatch(request.Document, @"^\d{11}$") && !Regex.IsMatch(request.Document, @"^\d{14}$"))
    throw new Exception("Documento inválido");
```

## KISS (Keep It Simple, Stupid)

**Mantenha simples.** Prefira soluções diretas e claras ao invés de abstrações complexas e desnecessárias.

### Como aplicar

- **Simplicidade primeiro:** Resolva o problema da forma mais simples possível.
- **Evite over-engineering:** Não crie interfaces, classes ou padrões apenas "por precaução".
- **Métodos pequenos:** Um método deve fazer **uma única coisa** e fazê-la bem.
- **Nomenclatura clara:** Nomes devem ser autoexplicativos. Evite abreviações obscuras.
- **Controllers enxutos:** Delegue toda lógica para Use Cases. O Controller deve apenas receber, delegar e retornar.

**Exemplo:**

```csharp
// Bom - Simples e direto
public class CreateCustomerUseCase
{
    public async Task<CustomerResponse> ExecuteAsync(CreateCustomerCommand command)
    {
        var document = new Document(command.Document);
        var customer = Customer.Create(document, command.Name, command.Email, command.Phone);
        await _repository.AddAsync(customer);
        return _mapper.Map<CustomerResponse>(customer);
    }
}

// Ruim - Complexo e desnecessário
public class CreateCustomerOrchestratorFactoryBuilderStrategy
{
    // Camadas excessivas para algo simples
}
```

## CLEAN (Clean Code)

**Código limpo é código fácil de ler, entender e modificar.** Escreva pensando em quem vai ler depois (inclusive você mesmo).

### Como aplicar

- **Nomenclatura semântica:** Classes, métodos e variáveis devem ter nomes que revelem intenção.
- **Métodos pequenos e coesos:** Idealmente entre 5-20 linhas. Se crescer muito, quebre em métodos menores.
- **Sem comentários desnecessários:** O código deve ser autoexplicativo. Use comentários apenas para explicar **"porquê"**, nunca **"o quê"**.
- **Early return:** Evite aninhamentos profundos. Retorne cedo sempre que possível.
- **Tratamento de erros claro:** Utilize exceções específicas ao invés de códigos de retorno genéricos.
- **Organização por responsabilidade:** Siga a estrutura de camadas do DDD + Clean Architecture.

**Exemplo:**

```csharp
// Bom - Claro e legível
public async Task<ServiceOrder> GetActiveOrderByPlateAsync(string plateValue)
{
    var plate = new Plate(plateValue);
    var order = await _repository.GetActiveByPlateAsync(plate);
    
    if (order is null)
        throw new ServiceOrderNotFoundException(plateValue);
    
    return order;
}

// Ruim - Confuso, aninhado e sem intenção
public async Task<ServiceOrder> Get(string p)
{
    if (!string.IsNullOrEmpty(p))
    {
        if (Regex.IsMatch(p, "..."))
        {
            var o = await _repo.Find(p);
            if (o != null) return o;
        }
    }
    throw new Exception("err");
}
```

## SOLID

Os princípios **SOLID** são fundamentais para manter o código desacoplado, flexível e fácil de evoluir.

| Princípio | Significado | Como aplicar no AutoGarage |
|---|---|---|
| **S - Single Responsibility Principle (SRP)** | Uma classe deve ter **um único motivo para mudar**. | Cada Use Case tem uma única responsabilidade. Controllers lidam apenas com HTTP. Repositórios lidam apenas com persistência. |
| **O - Open/Closed Principle (OCP)** | Aberto para extensão, fechado para modificação. | Utilize interfaces (`IPaymentGateway`) para adicionar novas implementações sem alterar código existente. |
| **L - Liskov Substitution Principle (LSP)** | Subtipos devem ser substituíveis por seus tipos base sem quebrar a aplicação. | Implementações concretas de `ICustomerRepository` devem respeitar o contrato da interface, sem comportamentos inesperados. |
| **I - Interface Segregation Principle (ISP)** | Prefira **interfaces específicas** ao invés de interfaces grandes e genéricas. | Crie interfaces pequenas e coesas. Ex: `ICustomerReadRepository` e `ICustomerWriteRepository` ao invés de um `ICustomerRepository` gigante. |
| **D - Dependency Inversion Principle (DIP)** | Dependa de **abstrações**, não de implementações concretas. | `Application` define interfaces (portas), `Infrastructure` implementa (adaptadores). A injeção de dependência é feita na `Api`. |

### Exemplos práticos

```csharp
// D - Dependency Inversion
// Application define a abstração
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(long id);
    Task AddAsync(Customer customer);
}

// Infrastructure implementa
public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    public async Task AddAsync(Customer customer) => await _context.Customers.AddAsync(customer);
}

// Api registra via DI
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
```

```csharp
// I - Interface Segregation
// Ruim - Interface muito genérica
public interface IRepository<T>
{
    Task<T> GetAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<IEnumerable<T>> SearchAsync(string filter); // Nem todo repositório precisa disso
}

// Bom - Interfaces segregadas
public interface IReadRepository<T>
{
    Task<T?> GetByIdAsync(long id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWriteRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
}
```

```csharp
// S - Single Responsibility
// Ruim - Controller faz tudo
public class OrdersController : ControllerBase
{
    public IActionResult Create(CreateOrderDto dto)
    {
        // validação, mapeamento, regra de negócio, persistência...
    }
}

// Bom - Responsabilidades separadas
public class OrdersController : ControllerBase
{
    private readonly ICreateServiceOrderUseCase _useCase;
    private readonly IMapper _mapper;

    public async Task<IActionResult> Create(CreateOrderRequest request)
    {
        var command = _mapper.Map<OpenServiceOrderCommand>(request);
        var result = await _useCase.ExecuteAsync(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

## Checklist para Revisão

Antes de submeter código, verifique:

- [ ] **DDD:** A regra de negócio está na camada correta (`Domain` ou `Application`)?
- [ ] **DRY:** Não existe duplicação de lógica, validação ou mapeamento?
- [ ] **KISS:** A solução é simples o suficiente para ser entendida rapidamente?
- [ ] **CLEAN:** Os nomes são claros, métodos pequenos e sem comentários desnecessários?
- [ ] **SOLID:** Estou respeitando os 5 princípios, principalmente DIP e SRP?
- [ ] **Arquitetura:** As dependências respeitam a direção definida em `docs/arquitetura/README.md`?
- [ ] **Erros:** Exceções seguem o padrão Problem Details definido em [`docs/principios-de-codigo/ExceptionHandler-ProblemDetails.md`](ExceptionHandler-ProblemDetails.md)?
- [ ] **Domínio:** Uso de Value Objects e Entidades conforme [`docs/principios-de-codigo/ValueObjects-e-Entidades.md`](ValueObjects-e-Entidades.md)?
