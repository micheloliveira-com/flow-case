# C4 - Contexto

Este diagrama apresenta a solução no nível de contexto, mostrando o usuário, o sistema Flow e as principais dependências de plataforma usadas para autenticação, mensageria, persistência e orquestração local.

```mermaid
C4Context
    title Flow - Contexto do Sistema

    Person(user, "Usuário autenticado", "Operador do fluxo de caixa")

    System(flow, "Flow", "Sistema de controle de lançamentos e saldo diário consolidado")

    System_Ext(keycloak, "Keycloak", "Provedor de identidade OIDC/JWT")
    System_Ext(rabbitmq, "RabbitMQ", "Broker de mensagens")
    SystemDb(postgres, "PostgreSQL", "Persistência transacional e projeções")
    System_Ext(aspire, ".NET Aspire AppHost", "Orquestração local, service discovery, health checks e observabilidade")

    Rel(user, flow, "Acessa interface web")
    Rel(flow, keycloak, "Autentica usuário e valida tokens")
    Rel(flow, rabbitmq, "Publica e consome eventos")
    Rel(flow, postgres, "Persiste dados por contexto")

    Rel(aspire, flow, "Provisiona e integra recursos locais")
    Rel(aspire, keycloak, "Provisiona recursos locais")
    Rel(aspire, rabbitmq, "Provisiona recursos locais")
    Rel(aspire, postgres, "Provisiona recursos locais")
```

## Observações

- O sistema foi dividido em dois contextos principais: lançamentos e relatórios.
- A autenticação é centralizada no Keycloak com OIDC/JWT.
- A comunicação entre lançamentos e relatórios é assíncrona via RabbitMQ.
- O .NET Aspire é usado para simplificar a execução local da aplicação distribuída.
