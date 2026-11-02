# Como testar a API

Este guia mostra como subir um ambiente de testes completo (PostgreSQL + API) com Docker Compose e como usar a collection do Postman que fica nesta pasta.

## O que é o ambiente

O `docker-compose.yml` na raiz do projeto sobe dois containers:

| Container | Imagem | Função |
|---|---|---|
| `autogarage-db` | `postgres:15` | Banco de testes. Os dados ficam em `tmpfs`: somem quando os containers são removidos. |
| `autogarage-api` | build a partir do `Dockerfile` | API em `http://localhost:7169`. Cria o banco `autogarage` e roda as migrations no startup. |

O banco do compose é separado do seu banco local, então os dados de teste nunca se misturam com o que você usa no dia a dia.

## Pré-requisitos

- Docker com o daemon em execução (Docker Desktop, por exemplo).
- Postman, para importar a collection.

## Subir o ambiente

Na raiz do projeto:

```bash
docker compose up --build -d
```

Na primeira vez, o build baixa as imagens e restaura as dependências, então demora mais. As próximas subidas usam o cache.

Confira se a API respondeu:

```bash
curl http://localhost:7169/health
```

A resposta esperada é `Healthy`. Se a API ainda não estiver pronta, aguarde alguns segundos e tente de novo. Para ver os logs de startup e as migrations:

```bash
docker compose logs -f api
```

## Autenticação

Todas as rotas exigem um token JWT, exceto `POST /auth/login`, `GET /health`, `GET /public/service-orders` e `POST /public/service-orders/{id}/approval`.

- O token vem do login e vale **15 minutos**. Depois disso a API responde `401`, e é preciso fazer login de novo.
- Envie o token no cabeçalho `Authorization: Bearer <token>`.
- O funcionário que realiza a ação é o que está no token. Os requests de ordem de serviço **não** recebem `employeeId`: se o corpo trouxer esse campo, ele é ignorado.
- Um funcionário desativado mantém acesso até o token expirar, porque o status não é consultado a cada requisição.

Na collection do Postman, o login guarda o token automaticamente na variável `token`, e todas as requisições internas usam essa variável.

## Dados de teste

Na subida, o compose cria um conjunto fixo de registros (`Settings:TestData:Enabled=true`). Assim você já tem o que consultar e usar sem precisar cadastrar nada antes:

| Tipo | Registros |
|---|---|
| Serviços | Um ativo (**Troca de óleo**) e um inativo (**Alinhamento**) |
| Materiais | **Filtro de óleo**, com estoque cheio (10, mínimo 3). **Pastilha de freio**, com estoque zero (mínimo 2) |
| Clientes | **Cliente Um** (CPF `529.982.247-25`), **Cliente Dois** (`390.533.447-05`) e **Cliente Três** (`111.444.777-35`) |
| Veículos | Um por cliente: `ABC1D23` (Ford Ka), `DEF2G34` (Fiat Uno) e `GHI3J45` (VW Gol) |
| Ordens de serviço | Três em `RECEIVED`, uma por par cliente/veículo |
| Funcionários | **Mecânico Teste** (`mecanico@autogarage.local`), **Atendente Teste** (`atendente@autogarage.local`) e **Atendente Inativo** (`atendente.inativo@autogarage.local`, inativo) |

A senha dos três funcionários é `Teste@123`. Ela é gravada com o mesmo hash da aplicação, então o login funciona normalmente.

Os IDs são gerados na criação, então para pegá-los use as listagens (`GET /api/services`, `GET /api/materials`, `GET /api/customers`, `GET /api/vehicles`, `GET /api/service-orders` e `GET /api/employees`) e copie o `id` que precisar para as variáveis da collection.

A carga é controlada por `Settings:TestData:Enabled`. Ela está ligada no `appsettings.Development.json`, então vale tanto para o compose quanto para a API rodada localmente em `Development`. Nesse caso, o seed também roda no banco local, na primeira subida. Ele é idempotente: se o mecânico de teste já existir, nada é criado de novo. Em `Production`, a configuração fica `false` e nenhum dado de teste é criado.

### Logins de teste

| E-mail | Senha | Papel | Resultado esperado no login |
|---|---|---|---|
| `mecanico@autogarage.local` | `Teste@123` | Mecânico | `200` com token |
| `atendente@autogarage.local` | `Teste@123` | Atendente | `200` com token |
| `atendente.inativo@autogarage.local` | `Teste@123` | Atendente (inativo) | `401` `invalid-credentials` |
| `admin@autogarage.local` | `Admin@123456` | Atendente (admin) | `200` com token |

O admin vem do `BootstrapAdmin` do `appsettings.Development.json`. Ele é criado também no compose, porque o compose roda em `Development`. Essas senhas são só para o ambiente local.

## Verificar o ambiente

Depois do `docker compose up`, estes comandos confirmam que o ambiente está pronto:

```bash
# API no ar
curl http://localhost:7169/health                      # Healthy

# Login do mecânico: copie o valor de "token" da resposta para TOKEN
curl -X POST http://localhost:7169/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Email":"mecanico@autogarage.local","Password":"Teste@123"}'
TOKEN="cole-o-token-aqui"

# Consulta pública de um dos clientes de teste (CPF 529.982.247-25), sem token
curl "http://localhost:7169/public/service-orders?document=52998224725"   # 1 ordem em RECEIVED

# Serviços e materiais (exigem token)
curl -H "Authorization: Bearer $TOKEN" http://localhost:7169/api/services     # Troca de óleo (ativo)
curl -H "Authorization: Bearer $TOKEN" http://localhost:7169/api/materials    # Filtro de óleo (10) e Pastilha de freio (0)

# Sem token, a API responde 401
curl -i http://localhost:7169/api/services                                    # 401
```

Confira também o banco: `docker compose exec db psql -U postgres -d autogarage -c 'select count(*) from service_orders;'` deve retornar `3`.

## Avisos conhecidos

- **`Error: libgssapi_krb5.so.2: cannot open shared object file` no log do startup.** O Npgsql tenta carregar a biblioteca de Kerberos, que não existe na imagem base. Não impede a API de funcionar: o `/health` responde `Healthy` e as migrations e o seed rodam normalmente. É um erro de log, não de execução.
- **`Failed to determine the https port for redirect`.** A API não expõe HTTPS no compose, então o redirecionamento não é aplicado. Os testes usam HTTP, em `http://localhost:7169`.

## Zerar os dados

O banco não persiste nada. Para começar do zero, basta reiniciar:

```bash
docker compose down
docker compose up --build -d
```

O `down` remove os containers e o `tmpfs` com eles, então o próximo `up` cria um banco vazio.

## Inspecionar o banco

O Postgres não é exposto no host. Para consultar, use o `psql` dentro do container:

```bash
docker compose exec db psql -U postgres -d autogarage
```

## Importar a collection

1. No Postman, clique em **Import** e selecione `Autogarage-collection.postman_collection`, nesta pasta.
2. Abra as variáveis da collection (aba **Variables**) e preencha `baseUrl` com `http://localhost:7169`.
3. Preencha as variáveis de ID conforme você cria os recursos. Elas não são preenchidas sozinhas: cada requisição de criação retorna um `id` que você copia para a variável correspondente, como `customerID`, `vehicleID`, `serviceID`, `materialID`, `employeeID` e `serviceOrderID`.

Como o banco começa vazio a cada subida, não existe nenhum ID pronto para usar. Crie os dados na ordem em que dependem uns dos outros: funcionário e cliente antes de veículo, veículo e serviço antes da ordem de serviço. O corpo de cada requisição está na própria collection.

## Explorar a API pelo Scalar

Como alternativa ao Postman, a documentação interativa da API fica em `http://localhost:7169/scalar`. Ela lê o documento OpenAPI em `http://localhost:7169/openapi/v1.json` e lista todos os endpoints com os corpos de requisição.

- A interface só existe no ambiente de desenvolvimento, que é o que o `docker-compose.yml` sobe. Em produção ela não é mapeada.
- As rotas que exigem autenticação pedem um token. Faça o login pelo próprio Scalar, copie o valor de `token` e informe-o como Bearer no botão de autorização.
- Os IDs são os mesmos da seção de dados de teste. Use as listagens para descobri-los.

## Fluxo de ordem de serviço

Os endpoints de ordem de serviço seguem a máquina de estados da aplicação. Uma ordem vai de `RECEIVED` para `IN_DIAGNOSIS` (requisição **Move to diagnosis**), depois para `AWAITING_APPROVAL` (**Move to await approval**), e daí para o fim do fluxo. Os endpoints devolvem `409` quando a transição não é permitida no estado atual, então um `409` costuma indicar uma ordem em outro estado, não um erro da API.

A aprovação pública do orçamento tem regras próprias: prazo de 10 dias contado a partir de `BudgetSentAt`, baixa de estoque na aprovação e estados conforme o caso: `IN_EXECUTION` (aprovação com estoque suficiente), `AWAITING_MATERIALS` (aprovação sem estoque), `FINISHED` (recusa) ou `CANCELLED` (prazo vencido). Os estados e o prazo estão em `docs/arquitetura/modelo-de-dados.md`.

## Health check

Além de `/health`, a collection tem a requisição **health-check**, na raiz. Ela serve para confirmar que a API está de pé antes de começar os testes.

## Encerrar

```bash
docker compose down
```

Isso para e remove os containers. O banco de testes some junto, sem precisar de limpeza manual.

## Ordem e filtros das listagens

- `GET /api/service-orders` (interna): traz todas as ordens de serviço, exceto as `DELIVERED`. A ordem segue a sequência do enum de status (`RECEIVED`, `IN_DIAGNOSIS`, `AWAITING_APPROVAL`, `AWAITING_MATERIALS`, `IN_EXECUTION`, `FINISHED`, `CANCELLED`) e, dentro de cada status, as mais antigas primeiro (data de criação e depois `Id`).
- `?status=` filtra pelo status. Com `?status=DELIVERED` a lista volta vazia, porque as entregues ficam fora da listagem. A ordem continua acessível por `GET /api/service-orders/{id}`.
- `GET /public/service-orders?document=`: mesma ordem, restrita ao cliente identificado pelo CPF/CNPJ, também sem as `DELIVERED`.
