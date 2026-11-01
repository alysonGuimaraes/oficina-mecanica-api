# Linguagem ubíqua - AutoGarage

A linguagem ubíqua é o vocabulário compartilhado entre quem conhece o negócio da oficina e quem escreve o código. Os mesmos termos aparecem no código, na API, na collection, nos testes e nesta documentação. Se um termo muda no negócio, ele muda aqui e no código, no mesmo commit.

## Glossário

| Termo (negócio) | Nome no código | Significado |
|---|---|---|
| Cliente | `Customer` | Pessoa física (CPF) ou jurídica (CNPJ) que contrata os serviços. Pode ser ativada ou inativada, mas nunca excluída. |
| Veículo | `Vehicle` | Carro, moto ou outro veículo de um cliente, identificado pela placa. Pertence a um único cliente por vez. |
| Funcionário | `Employee` | Pessoa que opera a oficina. Tem um papel: mecânico (`MECHANIC`) ou atendente (`ATTENDANT`). |
| Serviço | `Service` | Trabalho oferecido pela oficina, com preço fixo. Um serviço inativo não pode entrar em novas ordens. |
| Peça ou material | `Material` | Item de estoque usado nos serviços, com preço, estoque atual e estoque mínimo. |
| Ordem de serviço (OS) | `ServiceOrder` | Raiz do agregado do atendimento: um veículo, um cliente, seus itens e seu histórico de status. |
| Item de serviço | `ServiceOrderService` | Serviço incluído em uma OS. |
| Item de peça | `ServiceOrderMaterial` | Peça incluída em uma OS, com a quantidade usada. |
| Histórico de status | `ServiceOrderStatusHistory` | Registro de cada mudança de status, de cada item adicionado ou removido, com o funcionário responsável. |
| Movimentação de estoque | `MaterialTransaction` | Entrada (`IN`) ou saída (`OUT`) de peças. Quando vem de uma OS, guarda a OS de origem. |
| Orçamento | `BudgetTotal`, `BudgetSentAt` | Valor total da OS, calculado a partir dos itens ativos. `BudgetSentAt` marca o início do prazo de resposta. |
| Prazo de aprovação | `ApprovalDeadlineDays` | Número de dias que o cliente tem para aprovar ou recusar o orçamento (10 dias). Vencido o prazo, a OS é cancelada. |
| Baixa de estoque | `DecreaseStock` | Saída das peças da OS quando o serviço começa a ser executado. |
| Devolução de estoque | `IncreaseStock`, `ServiceOrderStockReturn` | Retorno das peças de uma OS cancelada durante a execução. |
| Documento | `Document` (VO) | CPF ou CNPJ, validados e normalizados (só dígitos). |
| Placa | `Plate` (VO) | Placa no formato antigo ou Mercosul, normalizada. |
| Motivo de cancelamento | `CancellationReason` | Texto obrigatório que explica por que a OS foi cancelada. |
| Observação de finalização | `Observation` | Texto que registra o resultado do serviço ao finalizar a OS. |

## Item da OS

Um **item da OS** é a linha que liga um serviço ou uma peça do catálogo a uma ordem de serviço. Cada linha tem o seu próprio `Id` (`ServiceOrderService` ou `ServiceOrderMaterial`), que é diferente do id do catálogo (`Service` ou `Material`). Os dois ids são usados em momentos diferentes:

- **Para adicionar**, a API recebe o id do catálogo: `ServiceId` ou `MaterialId`.
- **Para remover**, a API recebe o id da linha: `ItemId`. O valor vem da resposta de adicionar ou do detalhe da OS (`services[].id` e `materials[].id`).

Remover só é permitido enquanto a OS está em `IN_DIAGNOSIS`. Usar o id do catálogo na remoção responde `404` (`service-order-item-not-found`) e não remove nada. O histórico registra a remoção como `ITEM_SERVICE` ou `ITEM_MATERIAL`, com direção `OUT`, no campo `ItemId`.

## Estados da ordem de serviço

Os nomes de negócio são os usados nas APIs de consulta e nos relatórios. O código usa os nomes em inglês e o valor numérico persistido.

| Estado (negócio) | Código | Valor | Quando acontece |
|---|---|---|---|
| Recebida | `RECEIVED` | 1 | A OS é aberta para o veículo do cliente |
| Diagnóstico | `IN_DIAGNOSIS` | 2 | Um mecânico está avaliando o veículo |
| Aguardando Aprovação | `AWAITING_APPROVAL` | 3 | O orçamento foi enviado ao cliente |
| Aguardando Materiais | `AWAITING_MATERIALS` | 4 | O cliente aprovou, mas falta peça em estoque |
| Em Execução | `IN_EXECUTION` | 5 | O serviço está sendo feito, com as peças já baixadas |
| Finalizada | `FINISHED` | 6 | O serviço terminou, ou o orçamento foi recusado |
| Cancelada | `CANCELLED` | 7 | A OS foi cancelada, inclusive por prazo de aprovação vencido |
| Entregue | `DELIVERED` | 8 | O veículo foi devolvido ao cliente |

As transições permitidas estão em `StateMachines/ServiceOrderTransitions.cs`, e a documentação do ciclo completo está em [modelo de dados](modelo-de-dados.md).

## Ações que o negócio reconhece

| Ação (negócio) | Endpoint | Efeito |
|---|---|---|
| Abrir OS | `POST /api/service-orders` | Cria a OS em `RECEIVED` |
| Iniciar diagnóstico | `POST /api/service-orders/{id}/diagnosis` | `RECEIVED` para `IN_DIAGNOSIS` |
| Enviar orçamento | `POST /api/service-orders/{id}/budget` | `IN_DIAGNOSIS` para `AWAITING_APPROVAL` |
| Aprovar ou recusar orçamento | `POST /public/service-orders/{id}/approval` | Aprovação leva a `IN_EXECUTION` ou `AWAITING_MATERIALS`; recusa leva a `FINISHED` |
| Iniciar execução | `POST /api/service-orders/{id}/in-execution` | `AWAITING_MATERIALS` para `IN_EXECUTION`, com baixa de estoque |
| Finalizar | `POST /api/service-orders/{id}/completion` | Leva a `FINISHED`, com observação |
| Entregar | `POST /api/service-orders/{id}/delivery` | Leva a `DELIVERED` |
| Cancelar | `POST /api/service-orders/{id}/cancellation` | Leva a `CANCELLED`, com motivo |

## Regras de uso

- **Use o termo do negócio.** Escreva `ServiceOrder`, não `Order` ou `Ticket`; escreva `Customer`, não `User` ou `Client`.
- **Não use sinônimos.** Por exemplo, "peça" e "material" são o mesmo conceito, e no código o nome é `Material`. Evite "OS" no código, mas use a sigla nos textos para o usuário.
- **Nomes no código seguem o termo.** Se a regra fala em "baixa", o método se chama `DecreaseStock`, não `Remove` ou `Update`.
- **Estado é o nome do estado.** Não use "pendente", "aberta" ou "em andamento" no lugar dos estados listados acima.
- **Exclusão lógica.** Clientes, veículos, serviços, materiais e funcionários são inativados, nunca apagados. Itens de uma OS também são desativados, para preservar o histórico.
- **Cada mudança de status deixa um registro.** Não existe transição sem linha em `ServiceOrderStatusHistory`.

## Contextos

- **Atendimento**: abertura, diagnóstico, orçamento, execução e entrega da OS. Usa clientes, veículos, serviços e funcionários.
- **Estoque**: peças, entradas, saídas e baixa por OS. Usa materiais e movimentações.
- **Acompanhamento do cliente**: consulta pública das OS pelo documento do cliente e aprovação do orçamento. Não expõe dados internos de funcionários.

Os contextos compartilham as entidades `ServiceOrder` e `Material`, mas cada um usa os termos do seu próprio ponto de vista.

## Referências

- [Arquitetura de código](README.md): camadas, dependências e padrões.
- [Modelo de dados](modelo-de-dados.md): tabelas, colunas e regras de persistência.
- [Princípios de código](../principios-de-codigo/Principios-de-Codigo.md): DDD, Clean Code e SOLID.
