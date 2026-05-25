# C4 - Containers

Este diagrama apresenta os containers da solução, incluindo frontend, APIs, workers, bancos de dados e broker de mensagens.

```mermaid
C4Container
    title Flow - Containers

    Person(user, "Usuário")

    System_Boundary(flow, "Flow") {

        Container(web, "Flow.Web.Blazor", "Blazor Server",
            "Frontend Blazor Server com login OIDC e interface")

        Container(transactionsApi, "Flow.Transactions.ApiService", "ASP.NET Core",
            "API de lançamentos (CRUD + eventos de recomputação)")

        Container(transactionsWorker, "TransactionDailyRecomputeWorker", "Worker Service",
            "Processamento assíncrono de recomputação diária")

        Container(reportsApi, "Flow.Reports.ApiService", "ASP.NET Core",
            "API de relatórios e consultas de saldo diário")

        Container(reportsWorker, "TransactionDailyBalanceWorker", "Worker Service",
            "Consolidação diária de projeções")
    }

    System_Ext(keycloak, "Keycloak", "OIDC / JWT Provider")
    System_Ext(rabbitmq, "RabbitMQ", "Message Broker")
    SystemDb(transactionsDb, "PostgreSQL (transactionsapiservicedb)", "Persistência de lançamentos")
    SystemDb(reportsDb, "PostgreSQL (reportsapiservicedb)", "Persistência de projeções")
    System_Ext(aspire, ".NET Aspire AppHost", "Orquestração local")

    Rel(user, web, "Usa via browser")

    Rel(web, keycloak, "Login OIDC")
    Rel(web, transactionsApi, "HTTP + Bearer token")
    Rel(web, reportsApi, "HTTP + Bearer token")

    Rel(transactionsApi, keycloak, "Valida JWT")
    Rel(reportsApi, keycloak, "Valida JWT")

    Rel(transactionsApi, transactionsDb, "EF Core")
    Rel(reportsApi, reportsDb, "EF Core")

    Rel(transactionsApi, rabbitmq, "Publica transaction-daily-recompute")
    Rel(rabbitmq, transactionsWorker, "Consome recompute")

    Rel(transactionsWorker, transactionsDb, "Consulta lançamentos do dia")
    Rel(transactionsWorker, rabbitmq, "Publica transaction-daily-balance")

    Rel(rabbitmq, reportsWorker, "Consome balance events")
    Rel(reportsWorker, reportsDb, "Atualiza projeções")

    Rel(aspire, web, "Orquestra")
    Rel(aspire, transactionsApi, "Orquestra")
    Rel(aspire, reportsApi, "Orquestra")
    Rel(aspire, keycloak, "Orquestra")
    Rel(aspire, rabbitmq, "Orquestra")
    Rel(aspire, transactionsDb, "Orquestra")
    Rel(aspire, reportsDb, "Orquestra")
```

## Decisões representadas

- O frontend Blazor evita chamadas manuais autenticadas durante o teste, pois integra login OIDC e encaminhamento de token.
- O serviço de lançamentos é independente do serviço de relatórios.
- O saldo diário consolidado é atualizado de forma assíncrona.
- Cada microsserviço possui sua própria base PostgreSQL.
- Redis não foi utilizado, pois o requisito de 50 requisições por segundo é atendido pela projeção materializada e pela mensageria.
