# ADR-0001 - Usar .NET Aspire como AppHost e orquestrador local

## Status

Aceita.

## Contexto

O case técnico exige uma solução distribuída, documentada e executável localmente. A aplicação possui frontend, duas APIs, RabbitMQ, Redis, Keycloak e dois bancos PostgreSQL. Manter a configuração manual desses recursos aumentaria o custo de execução e dificultaria a avaliação do projeto.

## Decisão

Usar .NET Aspire como AppHost da solução. O `Flow.Aspire.AppHost` declara os recursos da aplicação, suas dependências e a ordem de inicialização.

O AppHost provisiona:

- `webfrontend`
- `transactionsapiservice`
- `reportsapiservice`
- RabbitMQ
- Redis
- Keycloak com importação de realm
- PostgreSQL para Transactions
- PostgreSQL para Reports
- pgAdmin

## Consequências positivas

- Execução local simplificada com um único comando.
- Service discovery nativo entre os projetos .NET.
- Centralização de logs, health checks e endpoints no dashboard Aspire.
- Redução de configuração manual de connection strings.
- Melhor demonstração de arquitetura distribuída em ambiente local.

## Trade-offs

- O projeto fica dependente do modelo de AppHost do Aspire para a experiência local.
- Ambientes produtivos ainda exigiriam definição explícita de infraestrutura, IaC, secrets e políticas de deploy.
- O uso de versões recentes do .NET/Aspire exige SDK compatível na máquina de avaliação.

## Alternativas consideradas

- Docker Compose manual: simples e conhecido, mas exigiria mais configuração de rede, variáveis e dependências.
- Executar cada serviço isoladamente: reduziria complexidade inicial, mas não demonstraria bem o comportamento distribuído.
- Kubernetes local: mais próximo de produção, porém exagerado para o escopo da solução.
