# C4 - Componentes

Este diagrama apresenta os principais componentes internos dos microsserviços `Transactions` e `Reports`, seguindo Clean Architecture, use cases explícitos e separação entre domínio, aplicação, infraestrutura e API.

```mermaid
flowchart TB
    subgraph Transactions["Flow.Transactions"]
        TransactionsEndpoints["Minimal API Endpoints<br/>/transactions"]
        CreateUseCase["CreateTransactionService"]
        UpdateUseCase["UpdateTransactionService"]
        DeleteUseCase["DeleteTransactionService"]
        GetUseCase["GetTransactionsService"]
        RecomputeUseCase["ExecuteTransactionDailyRecomputeService"]

        TransactionDomain["Transaction<br/>Entidade de domínio"]
        TransactionRepository["TransactionRepository<br/>EF Core"]
        RecomputePublisher["TransactionDailyRecomputePublisher"]
        BalancePublisher["TransactionDailyBalancePublisher"]
        RecomputeConsumer["TransactionDailyRecomputeConsumer"]
        RecomputeWorker["TransactionDailyRecomputeWorker"]
    end

    subgraph Reports["Flow.Reports"]
        ReportsEndpoints["Minimal API Endpoint<br/>/transaction_daily_balance"]
        GetBalanceUseCase["GetTransactionDailyBalance"]
        ExecuteBalanceUseCase["ExecuteTransactionDailyBalanceService"]

        BalanceDomain["TransactionDailyBalance<br/>Entidade de domínio"]
        BalanceRepository["TransactionDailyBalanceRepository<br/>EF Core"]
        BalanceConsumer["TransactionDailyBalanceConsumer"]
        BalanceWorker["TransactionDailyBalanceWorker"]
    end

    SharedContracts["Flow.Shared.Application.Abstractions<br/>TransactionDailyBalanceMessage"]
    SharedMessaging["Flow.Shared.Infrastructure.Abstractions<br/>IMessageConsumer / RabbitMqConsumer"]
    RabbitMQ["RabbitMQ"]
    TransactionsDb[("Transactions DB")]
    ReportsDb[("Reports DB")]

    TransactionsEndpoints --> CreateUseCase
    TransactionsEndpoints --> UpdateUseCase
    TransactionsEndpoints --> DeleteUseCase
    TransactionsEndpoints --> GetUseCase

    CreateUseCase --> TransactionDomain
    UpdateUseCase --> TransactionDomain
    CreateUseCase --> TransactionRepository
    UpdateUseCase --> TransactionRepository
    DeleteUseCase --> TransactionRepository
    GetUseCase --> TransactionRepository
    TransactionRepository --> TransactionsDb

    CreateUseCase --> RecomputePublisher
    UpdateUseCase --> RecomputePublisher
    DeleteUseCase --> RecomputePublisher
    RecomputePublisher --> RabbitMQ

    RabbitMQ --> RecomputeConsumer
    RecomputeConsumer --> RecomputeWorker
    RecomputeWorker --> RecomputeUseCase
    RecomputeUseCase --> TransactionRepository
    RecomputeUseCase --> BalancePublisher
    BalancePublisher --> SharedContracts
    BalancePublisher --> RabbitMQ

    ReportsEndpoints --> GetBalanceUseCase
    GetBalanceUseCase --> BalanceRepository
    BalanceRepository --> ReportsDb

    RabbitMQ --> BalanceConsumer
    BalanceConsumer --> BalanceWorker
    BalanceWorker --> ExecuteBalanceUseCase
    ExecuteBalanceUseCase --> SharedContracts
    ExecuteBalanceUseCase --> BalanceDomain
    ExecuteBalanceUseCase --> BalanceRepository

    RecomputeConsumer --> SharedMessaging
    BalanceConsumer --> SharedMessaging
```

## Leitura arquitetural

- A camada de API faz composição, autenticação, endpoints e workers.
- A camada de aplicação concentra os casos de uso.
- A camada de domínio concentra entidades e invariantes.
- A camada de infraestrutura implementa persistência e mensageria.
- Projetos compartilhados carregam apenas contratos e abstrações necessárias para integração entre serviços.
