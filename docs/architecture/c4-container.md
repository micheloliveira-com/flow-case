# C4 - Containers

Este diagrama apresenta os containers da solução, incluindo frontend, APIs, workers, bancos de dados e broker de mensagens.

```mermaid
flowchart LR
    User["Usuário"]

    subgraph Flow["Flow"]
        Web["Flow.Web.Blazor<br/>Frontend Blazor Server<br/>Login OIDC e interface mínima"]

        TransactionsApi["Flow.Transactions.ApiService<br/>API de lançamentos<br/>CRUD e publicação de recomputação"]
        TransactionsWorker["TransactionDailyRecomputeWorker<br/>Worker de recomputação diária"]

        ReportsApi["Flow.Reports.ApiService<br/>API de relatórios<br/>Consulta de saldo diário"]
        ReportsWorker["TransactionDailyBalanceWorker<br/>Worker de consolidação diária"]
    end

    Keycloak["Keycloak<br/>OIDC/JWT"]
    RabbitMQ["RabbitMQ<br/>Filas duráveis"]
    TransactionsDb[("PostgreSQL<br/>transactionsapiservicedb")]
    ReportsDb[("PostgreSQL<br/>reportsapiservicedb")]
    Aspire[".NET Aspire AppHost<br/>Orquestração local"]

    User -->|"HTTPS / Browser"| Web
    Web -->|"OIDC login"| Keycloak
    Web -->|"HTTP + Bearer token"| TransactionsApi
    Web -->|"HTTP + Bearer token"| ReportsApi

    TransactionsApi -->|"Valida JWT"| Keycloak
    ReportsApi -->|"Valida JWT"| Keycloak

    TransactionsApi -->|"EF Core"| TransactionsDb
    ReportsApi -->|"EF Core"| ReportsDb

    TransactionsApi -->|"Publica<br/>transaction-daily-recompute"| RabbitMQ
    RabbitMQ -->|"Consome<br/>transaction-daily-recompute"| TransactionsWorker
    TransactionsWorker -->|"Consulta lançamentos do dia"| TransactionsDb
    TransactionsWorker -->|"Publica<br/>transaction-daily-balance"| RabbitMQ
    RabbitMQ -->|"Consome<br/>transaction-daily-balance"| ReportsWorker
    ReportsWorker -->|"Atualiza projeção diária"| ReportsDb

    Aspire -.-> Web
    Aspire -.-> TransactionsApi
    Aspire -.-> ReportsApi
    Aspire -.-> Keycloak
    Aspire -.-> RabbitMQ
    Aspire -.-> TransactionsDb
    Aspire -.-> ReportsDb
```

## Decisões representadas

- O frontend Blazor evita chamadas manuais autenticadas durante o teste, pois integra login OIDC e encaminhamento de token.
- O serviço de lançamentos é independente do serviço de relatórios.
- O saldo diário consolidado é atualizado de forma assíncrona.
- Cada microsserviço possui sua própria base PostgreSQL.
- Redis não foi utilizado, pois o requisito de 50 requisições por segundo é atendido pela projeção materializada e pela mensageria.
