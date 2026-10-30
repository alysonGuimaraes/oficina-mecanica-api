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
- A entrada de estoque grava uma transação `IN` sem vínculo com OS. A coluna `ServiceOrderId` entra junto com a tabela de OS, quando o vínculo passar a existir.
- Duas movimentações simultâneas no mesmo material não têm proteção explícita contra saldo incorreto (ver pendências).

## Pendências

| Tema | Situação |
|---|---|
| Algoritmo do hash de senha | SHA-512 com chave fixa. A spec pede bcrypt. Trocar exige recriar os hashes existentes. |
| Unicidade de `Name` | A spec não exige, e o schema não tem índice. Decidir se o nome deve ser único. |
| Índice único de e-mail sem diferenciar caixa | Hoje o índice é sensível à caixa. Um índice sobre `lower("Email")` fecharia a brecha da corrida, mas exige nova migration. |
| Demais entidades (materiais, ordens) | Ainda não existem no schema. Serão documentadas quando as migrations forem criadas. |
| Concorrência no saldo de material | Sem token de concorrência, por decisão. Duas movimentações simultâneas no mesmo material podem gravar saldo incorreto. Revisar antes de liberar a baixa de estoque por OS. |
