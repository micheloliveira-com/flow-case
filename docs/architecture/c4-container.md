# C4 - Containers

Este diagrama apresenta os containers da solução, incluindo frontend, APIs, workers, bancos de dados e broker de mensagens.

```mermaid
C4Container
    title Flow - Containers

    Person(user, "Usuário")

    System_Boundary(flow, "Flow") {

        Container(web, "Flow.Web.Blazor", "Blazor Server", "Frontend Blazor Server")

        Container(transactionsApi, "Transactions API", "ASP.NET Core", "Lançamentos")

        Container(transactionsWorker, "Transactions Worker", "Worker Service", "Recompute diário")

        Container(reportsApi, "Reports API", "ASP.NET Core", "Consultas")

        Container(reportsWorker, "Reports Worker", "Worker Service", "Projeções")

        Container(transactionsDb, "Transactions DB", "PostgreSQL", "Dados de lançamentos")

        Container(reportsDb, "Reports DB", "PostgreSQL", "Dados de projeções")
    }

    System_Ext(keycloak, "Keycloak", "OIDC")
    System_Ext(rabbitmq, "RabbitMQ", "Messaging")
    System_Ext(aspire, ".NET Aspire", "Orquestração")

    Rel(user, web, "Usa")

    Rel(web, keycloak, "Login")
    Rel(web, transactionsApi, "HTTP")
    Rel(web, reportsApi, "HTTP")

    Rel(transactionsApi, keycloak, "JWT")
    Rel(reportsApi, keycloak, "JWT")

    Rel(transactionsApi, transactionsDb, "EF Core")
    Rel(reportsApi, reportsDb, "EF Core")

    Rel(transactionsApi, rabbitmq, "Publica eventos")
    Rel(rabbitmq, transactionsWorker, "Consome")

    Rel(transactionsWorker, transactionsDb, "Lê/escreve")
    Rel(transactionsWorker, rabbitmq, "Publica projeção")

    Rel(rabbitmq, reportsWorker, "Consome")
    Rel(reportsWorker, reportsDb, "Atualiza")

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
