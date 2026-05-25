# C4 - Contexto

Este diagrama apresenta a solução no nível de contexto, mostrando o usuário, o sistema Flow e as principais dependências de plataforma usadas para autenticação, mensageria, persistência e orquestração local.

```mermaid
flowchart LR
    User["Usuário autenticado<br/>Operador do fluxo de caixa"]

    Flow["Flow<br/>Sistema de controle de lançamentos<br/>e saldo diário consolidado"]

    Keycloak["Keycloak<br/>Provedor de identidade OIDC/JWT"]
    RabbitMQ["RabbitMQ<br/>Broker de mensagens"]
    Postgres["PostgreSQL<br/>Persistência transacional e projeções"]
    Aspire[".NET Aspire AppHost<br/>Orquestração local, service discovery,<br/>health checks e observabilidade"]

    User -->|"Acessa interface web"| Flow
    Flow -->|"Autentica usuário e valida tokens"| Keycloak
    Flow -->|"Publica e consome eventos"| RabbitMQ
    Flow -->|"Persiste dados por contexto"| Postgres
    Aspire -->|"Provisiona e integra recursos locais"| Flow
    Aspire -->|"Provisiona recursos locais"| Keycloak
    Aspire -->|"Provisiona recursos locais"| RabbitMQ
    Aspire -->|"Provisiona recursos locais"| Postgres
```

## Observações

- O sistema foi dividido em dois contextos principais: lançamentos e relatórios.
- A autenticação é centralizada no Keycloak com OIDC/JWT.
- A comunicação entre lançamentos e relatórios é assíncrona via RabbitMQ.
- O .NET Aspire é usado para simplificar a execução local da aplicação distribuída.
