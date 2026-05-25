# C4 - Componentes

Este diagrama apresenta os principais componentes internos dos microsserviços `Transactions` e `Reports`, seguindo Clean Architecture, use cases explícitos e separação entre domínio, aplicação, infraestrutura e API.

```mermaid
C4Component
    title Flow - Components

    Person(user, "Usuário")

    System_Boundary(txs, "Flow.Transactions") {

        Container(transactionsApi, "Transactions API", "ASP.NET Core", "Minimal API")

        Component(endpoints, "Transaction Endpoints", "Minimal API", "/transactions")

        Component(create, "CreateTransactionService", "Application Service", "Criação")
        Component(update, "UpdateTransactionService", "Application Service", "Atualização")
        Component(delete, "DeleteTransactionService", "Application Service", "Remoção")
        Component(get, "GetTransactionsService", "Application Service", "Consulta")

        Component(recompute, "ExecuteTransactionDailyRecomputeService", "Application Service", "Recompute diário")

        Component(domain, "Transaction", "Domain Entity", "Agregado raiz")
        Component(repo, "TransactionRepository", "EF Core", "Persistência")

        Component(recomputePub, "TransactionDailyRecomputePublisher", "Messaging", "Publica evento")
        Component(balancePub, "TransactionDailyBalancePublisher", "Messaging", "Publica balance")

        Component(recomputeWorker, "TransactionDailyRecomputeWorker", "Worker", "Processamento assíncrono")
        Component(recomputeConsumer, "TransactionDailyRecomputeConsumer", "Messaging", "Consumer")
    }

    System_Boundary(rep, "Flow.Reports") {

        Container(reportsApi, "Reports API", "ASP.NET Core", "Minimal API")

        Component(getBalance, "GetTransactionDailyBalance", "Application Service", "Consulta saldo diário")
        Component(execBalance, "ExecuteTransactionDailyBalanceService", "Application Service", "Processamento")

        Component(balanceDomain, "TransactionDailyBalance", "Domain Entity", "Projeção")
        Component(balanceRepo, "TransactionDailyBalanceRepository", "EF Core", "Persistência")

        Component(balanceWorker, "TransactionDailyBalanceWorker", "Worker", "Processamento assíncrono")
        Component(balanceConsumer, "TransactionDailyBalanceConsumer", "Messaging", "Consumer")
    }

    System_Ext(rabbitmq, "RabbitMQ", "Message Broker")
    SystemDb(txsDb, "Transactions DB", "PostgreSQL")
    SystemDb(repDb, "Reports DB", "PostgreSQL")

    Rel(endpoints, create, "calls")
    Rel(endpoints, update, "calls")
    Rel(endpoints, delete, "calls")
    Rel(endpoints, get, "calls")

    Rel(create, domain, "uses")
    Rel(update, domain, "uses")
    Rel(delete, domain, "uses")
    Rel(get, repo, "reads")
    Rel(repo, txsDb, "EF Core")

    Rel(create, recomputePub, "publishes")
    Rel(update, recomputePub, "publishes")
    Rel(delete, recomputePub, "publishes")

    Rel(recomputePub, rabbitmq, "events")

    Rel(rabbitmq, recomputeConsumer, "consumes")
    Rel(recomputeConsumer, recomputeWorker, "runs")
    Rel(recomputeWorker, recompute, "executes")

    Rel(recompute, repo, "reads/writes")
    Rel(recompute, balancePub, "publishes")
    Rel(balancePub, rabbitmq, "events")

    Rel(getBalance, balanceRepo, "reads")
    Rel(balanceRepo, repDb, "EF Core")

    Rel(rabbitmq, balanceConsumer, "consumes")
    Rel(balanceConsumer, balanceWorker, "runs")
    Rel(balanceWorker, execBalance, "executes")

    Rel(execBalance, balanceDomain, "updates")
    Rel(execBalance, balanceRepo, "persists")
```

## Leitura arquitetural

- A camada de API faz composição, autenticação, endpoints e workers.
- A camada de aplicação concentra os casos de uso.
- A camada de domínio concentra entidades e invariantes.
- A camada de infraestrutura implementa persistência e mensageria.
- Projetos compartilhados carregam apenas contratos e abstrações necessárias para integração entre serviços.
