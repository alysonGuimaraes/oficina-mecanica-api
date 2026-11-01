# Modelo de dados

**Fonte de verdade dos nomes:** o schema criado pelas migrations. Quando o código divergir do schema, o schema vence e o código é ajustado.

**Quem cria o schema:** FluentMigrator, executado na inicialização da API (`DatabaseMigrations.Migrate`), que também cria o banco caso ele não exista. As migrations ficam em `src/AutoGarage.Infrastructure/Migrations/Versions/`.

**Regra de migrations:** toda mudança de schema é uma classe nova `Version<NNNNNN>` herdando de `VersionBase` (ou de `ForwardOnlyMigration`), com número seguinte ao último. **Nunca altere uma migration já aplicada**: ela já rodou nos bancos existentes, e a alteração não chega a eles.

**Mapeamento do EF Core:** `AppDbContext` aplica as configurações de `Persistence/Configurations/`. Elas descrevem as mesmas constraints usadas pelas consultas, e precisam ficar alinhadas com as migrations.

## Regra de tradução

| Coluna no schema | Propriedade no código | Observação |
|---|---|---|
| `"Id"` `uuid` (PK) | `Guid Id` | Gerado na entidade (`Guid.NewGuid()`), não pelo banco |
| `"CreatedAt"` `timestamptz` | `DateTime CreatedAt` | Sempre UTC |
| `"UpdatedAt"` `timestamptz` (nulo) | `DateTime? UpdatedAt` | Nulo até a primeira alteração |
| `"Status"` `int` | `EntityStatus Status` | Conversão `HasConversion<int>()` |
| `"Role"` `int` | `EmployeeRole Role` | Conversão `HasConversion<int>()` |
| `varchar` de texto livre | `string` | Tamanho limitado em `AsString(n)` e `HasMaxLength(n)` |

Nomes de tabela e coluna seguem o schema: tabela em minúsculas (`employees`), colunas em PascalCase entre aspas, como foram criadas.

Dinheiro, quando existir, deve usar `numeric(10,2)` no schema e `decimal` no código. Não usar `double` ou `float`.

## Enums armazenados como inteiro

| Enum | Valores | Persistido como |
|---|---|---|
| `EntityStatus` | `INACTIVE` = 0, `ACTIVE` = 1 | `int` |
| `EmployeeRole` | `MECHANIC` = 1, `ATTENDANT` = 2 | `int` |
| `DocumentType` | `CPF` = 1, `CNPJ` = 2 | `int` |
| `PlateType` | `OLD` = 1, `MERCOSUL` = 2 | `int` |
| `VehicleType` | `CAR` = 1, `MOTORCYCLE` = 2, `PICKUP` = 3, `MINIBUS` = 4, `VAN` = 5, `OTHER` = 6 | `int` |
| `TransactionType` | `IN` = 1, `OUT` = 2 | `int` |
| `ServiceOrderStatus` | `RECEIVED` = 1, `IN_DIAGNOSIS` = 2, `AWAITING_APPROVAL` = 3, `AWAITING_MATERIALS` = 4, `IN_EXECUTION` = 5, `FINISHED` = 6, `CANCELLED` = 7, `DELIVERED` = 8 | `int` |
| `ServiceOrderEventType` | `STATUS_CHANGE` = 1, `ITEM_SERVICE` = 2, `ITEM_MATERIAL` = 3 | `int` |
| `ServiceOrderItemDirection` | `IN` = 1 (incluído na OS), `OUT` = 2 (removido da OS) | `int` |

Na API, enums trafegam como texto (`ACTIVE`, `MECHANIC`), por causa de `JsonStringEnumConverter` com `allowIntegerValues: false`. No banco ficam os inteiros acima. Os comentários de coluna nas migrations registram essa correspondência.

## Tabelas

### employees

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `Name` | `varchar(150)` | não | — | — |
| `Email` | `varchar(150)` | não | — | Índice único `IX_employees_Email` |
| `PasswordHash` | `varchar` (sem tamanho explícito) | não | — | Hash da senha (ver pendências) |
| `Role` | `int` | não | — | Valores de `EmployeeRole` |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus` |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | — |

**Leitura do modelo:**
- Funcionário nunca é removido fisicamente. Desativar é mudar `Status` para `INACTIVE`.
- O índice único de `Email` diferencia maiúsculas de minúsculas no banco. A aplicação compara sem diferenciar caixa, e grava o e-mail em maiúsculas desde a normalização do cadastro. Registros gravados antes dessa normalização podem estar em minúsculas.
- Se duas requisições gravarem o mesmo e-mail ao mesmo tempo, a checagem da aplicação pode passar nas duas, e o índice único rejeita a segunda com erro de banco, que hoje vira `500`.
- `PasswordHash` nunca é retornado pela API.

### customers

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `Name` | `varchar(150)` | não | — | — |
| `Document` | `varchar(14)` | não | — | Índice único `IX_customers_Document` |
| `DocumentType` | `int` | não | — | Valores de `DocumentType` |
| `Email` | `varchar(150)` | não | — | Índice único `IX_customers_Email` |
| `Ddd` | `varchar(2)` | não | — | — |
| `PhoneNumber` | `varchar(9)` | não | — | — |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus` |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | — |

**Leitura do modelo:**
- Cliente nunca é removido fisicamente. Inativar é mudar `Status` para `INACTIVE`.
- `Document` é gravado só com caracteres alfanuméricos, em maiúsculas, sem máscara (`52998224725`, `12ABC34501DE35`). A higienização é feita pelo Value Object `Document`, no domínio. O índice único impede dois clientes com o mesmo CPF/CNPJ, e o documento não é alterado por nenhum endpoint.
- `Ddd` e `PhoneNumber` são gravados só com dígitos, com 2 e 9 caracteres.
- O índice único de `Email` diferencia maiúsculas de minúsculas, como o de `employees`. A aplicação compara sem diferenciar caixa e grava o e-mail em maiúsculas. Corridas entre duas requisições com o mesmo e-mail caem no índice único, que responde com erro de banco e vira `500`.

### vehicles

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `Brand` | `varchar(50)` | não | — | — |
| `Model` | `varchar(50)` | não | — | — |
| `Plate` | `varchar(7)` | não | — | Índice único `IX_vehicles_Plate` |
| `PlateType` | `int` | não | — | Valores de `PlateType` |
| `VehicleType` | `int` | não | — | Valores de `VehicleType` (carro, motocicleta, caminhonete, microônibus, van, outros) |
| `Year` | `int` | não | — | Entre 1900 e o ano seguinte, validado na Application |
| `CustomerId` | `uuid` | não | — | FK para `customers(Id)`, `ON DELETE NO ACTION`, índice `IX_vehicles_CustomerId` |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus` |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | — |

**Leitura do modelo:**
- Veículo nunca é removido fisicamente. Inativar é mudar `Status` para `INACTIVE`.
- `Brand` e `Model` são gravados em maiúsculas, mantendo os caracteres digitados (`MERCEDES-BENZ`, `GOL 1.0`). Espaços nas pontas e espaços repetidos são reduzidos a um só.
- `Plate` é gravada normalizada, sem separadores (`ABC1234` ou `ABC1D23`). `PlateType` diz qual dos dois formatos ela é. A higienização e a validação são feitas pelo Value Object `Plate`, no domínio, da mesma forma que `Document` e `DocumentType` no cliente.
- `VehicleType` classifica o veículo e é obrigatório. Ele é informado no cadastro e não é alterado depois, nem pelo `PUT` nem pela troca de dono.
- `CustomerId` é o dono atual. A troca de dono altera só essa coluna. As ordens de serviço guardam o cliente da abertura, então o histórico de donos fica nelas, e não aqui.

### services

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `Name` | `varchar(150)` | não | — | Índice único `IX_services_Name` |
| `Description` | `varchar(500)` | sim | — | — |
| `Price` | `numeric(10,2)` | não | — | `CHECK ("Price" > 0)` |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus` |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | — |

**Leitura do modelo:**
- Serviço nunca é removido fisicamente. Inativar é mudar `Status` para `INACTIVE`.
- `Name` é gravado em maiúsculas, com espaços nas pontas e repetidos reduzidos a um só. A unicidade é o índice único sobre o valor já normalizado, então não diferencia caixa.
- `Description` é opcional. A spec cita descrição no `PUT`, mas não no `POST`.
- `Price` é validado no domínio (maior que zero) e repetido no `CHECK` do schema. Alterar o preço não afeta OS já emitidas: a OS guarda o preço no item, no momento da inclusão. Essa regra pertence à futura tabela de itens da OS.

### materials

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `Name` | `varchar(150)` | não | — | Índice único `IX_materials_Name` |
| `Description` | `varchar(500)` | sim | — | — |
| `Price` | `numeric(10,2)` | não | — | `CHECK ("Price" >= 0)` |
| `StockQuantity` | `int` | não | — | `CHECK ("StockQuantity" >= 0)`. Saldo atual |
| `StockMinimum` | `int` | não | — | `CHECK ("StockMinimum" >= 0)` |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus` |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | — |

**Leitura do modelo:**
- Material nunca é removido fisicamente. Inativar é mudar `Status` para `INACTIVE`.
- `Name` é gravado em maiúsculas, com espaços nas pontas e repetidos reduzidos a um só. A unicidade é o índice único sobre o valor já normalizado, então não diferencia caixa.
- `Price`, `StockQuantity` e `StockMinimum` não podem ser negativos, como exige a spec no `PUT`.
- `StockQuantity` é o saldo atual. Ele é alterado somente pelas movimentações (`stock-entry` e `stock-out`), e cada movimentação grava o novo saldo em `material_transactions.BalanceAfter`, na mesma transação do banco. O `PUT` do material não altera `StockQuantity`.
- Saída que deixaria o saldo negativo é rejeitada com `409` (código `insufficient-stock`), e nenhuma transação é gravada.
- Material está em estoque crítico quando `StockQuantity` é menor que `StockMinimum`.

### material_transactions

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `MaterialId` | `uuid` | não | — | FK para `materials(Id)`, `ON DELETE NO ACTION`, índice `IX_material_transactions_MaterialId` |
| `Type` | `int` | não | — | Valores de `TransactionType` |
| `Quantity` | `int` | não | — | `CHECK ("Quantity" > 0)` |
| `BalanceAfter` | `int` | não | — | `CHECK ("BalanceAfter" >= 0)`. Saldo do material logo após a movimentação |
| `CreatedAt` | `timestamptz` | não | — | — |

**Leitura do modelo:**
- A transação é um registro imutável: nunca é alterada nem removida. Por isso a tabela não tem `Status` nem `UpdatedAt`.
- A entrada de estoque grava uma transação `IN` sem vínculo com OS. A coluna `ServiceOrderId` (`uuid`, nulo, FK para `service_orders(Id)`) será adicionada na mesma migration das tabelas de OS, ainda não existe no schema. Ela será preenchida na devolução de estoque por cancelamento, que grava uma transação `IN` vinculada à OS.
- Duas movimentações simultâneas no mesmo material não têm proteção explícita contra saldo incorreto (ver pendências).

## Ordens de serviço (planejado, sem migration)

### service_orders

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `CustomerId` | `uuid` | não | — | FK para `customers(Id)`, `ON DELETE NO ACTION`, índice `IX_service_orders_CustomerId` |
| `VehicleId` | `uuid` | não | — | FK para `vehicles(Id)`, `ON DELETE NO ACTION`, índice `IX_service_orders_VehicleId` |
| `Status` | `int` | não | — | Valores de `ServiceOrderStatus`. Índice `IX_service_orders_Status` |
| `BudgetTotal` | `numeric(10,2)` | sim | — | `CHECK ("BudgetTotal" >= 0)`. Preenchido na transição para `AWAITING_APPROVAL` |
| `BudgetSentAt` | `timestamptz` | sim | — | Momento da entrada em `AWAITING_APPROVAL`. Início do prazo de 10 dias |
| `Observation` | `varchar(500)` | sim | — | Motivo de finalização (`completion`) |
| `CancellationReason` | `varchar(500)` | sim | — | Motivo de cancelamento, inclusive o de aprovação fora do prazo |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | — |

**Leitura do modelo:**
- A OS guarda o cliente da abertura (`CustomerId`), e não o dono atual do veículo.
- A OS não guarda funcionário. Quem fez a abertura e cada mudança de status, serviço ou material fica em `service_order_status_history.EmployeeId`.
- `Status` usa `ServiceOrderStatus`, e não `EntityStatus`. Por isso a tabela não usa o helper `VersionBase.CreateTable`, que adiciona `Status` com padrão `1` (ACTIVE).
- `BudgetTotal` é nulo até a transição para `AWAITING_APPROVAL`, e não muda depois disso.
- `BudgetSentAt`, `Observation`, `CancellationReason` e os tamanhos de `varchar` são propostas para atender às regras de prazo e de motivos.

### service_order_services

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `ServiceOrderId` | `uuid` | não | — | FK para `service_orders(Id)`, `ON DELETE NO ACTION`, índice `IX_service_order_services_ServiceOrderId` |
| `ServiceId` | `uuid` | não | — | FK para `services(Id)`, `ON DELETE NO ACTION`. Índice único com `ServiceOrderId`: `IX_service_order_services_ServiceOrderId_ServiceId` |
| `Price` | `numeric(10,2)` | não | — | `CHECK ("Price" > 0)`. Preço no momento da inclusão |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus`. `INACTIVE` quando o serviço é removido da OS |
| `CreatedAt` | `timestamptz` | não | — | — |
| `UpdatedAt` | `timestamptz` | sim | — | Preenchido quando a linha é reativada |

**Leitura do modelo:**
- Serviço não tem quantidade. Cada serviço aparece uma vez por OS (a unicidade é o índice composto).
- Remover um serviço muda `Status` para `INACTIVE`. A linha não é apagada, e a remoção fica registrada em `service_order_status_history`.
- Incluir de novo um serviço que está inativo na OS reativa a linha existente: `Status` volta para `ACTIVE`, `Price` recebe o preço atual do catálogo e `UpdatedAt` é preenchido. Assim o índice único não precisa ser parcial.
- Serviço já ativo na OS não pode ser incluído de novo (400).
- Alterar o preço do catálogo não altera as linhas já gravadas.

### service_order_materials

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `ServiceOrderId` | `uuid` | não | — | FK para `service_orders(Id)`, `ON DELETE NO ACTION`, índice `IX_service_order_materials_ServiceOrderId` |
| `MaterialId` | `uuid` | não | — | FK para `materials(Id)`, `ON DELETE NO ACTION`. Índice único com `ServiceOrderId`: `IX_service_order_materials_ServiceOrderId_MaterialId` |
| `Quantity` | `int` | não | — | `CHECK ("Quantity" > 0)` |
| `UnitPrice` | `numeric(10,2)` | não | — | `CHECK ("UnitPrice" >= 0)`. Preço unitário no momento da inclusão |
| `CreatedAt` | `timestamptz` | não | — | — |
| `Status` | `int` | não | `1` (ACTIVE) | Valores de `EntityStatus`. `INACTIVE` quando o material é removido da OS |
| `UpdatedAt` | `timestamptz` | sim | — | Preenchido quando a quantidade é somada ou quando a linha é reativada |

**Leitura do modelo:**
- Material repetido na mesma OS, com a linha ativa, soma a quantidade na linha existente.
- Incluir de novo um material cuja linha está inativa reativa a linha: `Status` volta para `ACTIVE`, `Quantity` recebe a quantidade informada (não soma com a anterior), `UnitPrice` recebe o preço atual e `UpdatedAt` é preenchido.
- Remover um material muda `Status` para `INACTIVE`. A linha não é apagada.
- A baixa de estoque na aprovação (ou em `in-execution`) usa só as linhas `ACTIVE` desta tabela, em uma única gravação atômica com as transações de saída.

### service_order_status_history

| Coluna | Tipo | Nulo | Padrão | Constraint |
|---|---|---|---|---|
| `Id` | `uuid` | não | — | Chave primária |
| `ServiceOrderId` | `uuid` | não | — | FK para `service_orders(Id)`, `ON DELETE NO ACTION`, índice `IX_service_order_status_history_ServiceOrderId` |
| `EventType` | `int` | não | — | Valores de `ServiceOrderEventType` |
| `FromStatus` | `int` | sim | — | Estado anterior, em eventos `STATUS_CHANGE`. Nulo na criação |
| `ToStatus` | `int` | sim | — | Estado novo, em eventos `STATUS_CHANGE` |
| `ItemId` | `uuid` | sim | — | Id da linha afetada em eventos de item (`ITEM_SERVICE` ou `ITEM_MATERIAL`). **Sem FK**, porque o item pertence a uma de duas tabelas |
| `ItemDirection` | `int` | sim | — | Valores de `ServiceOrderItemDirection`. Obrigatório quando `ItemId` está preenchido, e nulo quando não está |
| `EmployeeId` | `uuid` | não | — | FK para `employees(Id)`, `ON DELETE NO ACTION`. Funcionário que fez a mudança |
| `CreatedAt` | `timestamptz` | não | — | — |

**Leitura do modelo:**
- O histórico é imutável: não tem `UpdatedAt`.
- Registra mudanças de status (`EventType` = `STATUS_CHANGE`, com `FromStatus` e `ToStatus`) e inclusões e remoções de serviços e materiais (`EventType` = `ITEM_SERVICE` ou `ITEM_MATERIAL`, com `ItemId` e `ItemDirection`).
- `ItemDirection` = `IN` é inclusão na OS, e `OUT` é remoção. Não tem relação com a movimentação de estoque (`material_transactions.Type`), embora use a mesma ideia de entrada e saída.
- Regra de coerência, no banco ou na aplicação: `ItemId` e `ItemDirection` são nulos juntos. Em eventos `STATUS_CHANGE`, os dois são nulos.

### Alteração em material_transactions (planejada)

- Coluna `ServiceOrderId` (`uuid`, nulo, FK para `service_orders(Id)`, `ON DELETE NO ACTION`) entra na mesma migration. Ver a leitura do modelo de `material_transactions`.

## Pendências

| Tema | Situação |
|---|---|
| Algoritmo do hash de senha | SHA-512 com chave fixa. A spec pede bcrypt. Trocar exige recriar os hashes existentes. |
| Unicidade de `Name` | A spec não exige, e o schema não tem índice. Decidir se o nome deve ser único. |
| Índice único de e-mail sem diferenciar caixa | Hoje o índice é sensível à caixa. Um índice sobre `lower("Email")` fecharia a brecha da corrida, mas exige nova migration. |
| Ordens de serviço | Tabelas documentadas como planejadas, sem migration. Passam a valer como schema quando a migration for criada. |
| Concorrência no saldo de material | Sem token de concorrência, por decisão, inclusive para a baixa por OS (aprovação e `in-execution`). Duas movimentações simultâneas no mesmo material podem gravar saldo incorreto. Risco aceito. |
