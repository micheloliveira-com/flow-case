# C4 – Flow (Contexto + Containers)

## Visão geral

O sistema **Flow** é uma plataforma distribuída para controle de lançamentos financeiros e consolidação de saldo diário. Ele foi desenhado com separação clara entre interface, serviços de domínio, persistência e processamento assíncrono baseado em eventos.

A arquitetura segue um modelo orientado a eventos, com processamento desacoplado via mensageria e projeções materializadas para consultas.

---

## C4 Model (Structurizr DSL)

## Context Diagram

![Flow Context](assets/c4-context.jpg)

## Container Diagram

![Flow Container](assets/c4-container.jpg)


## Diagrama

<details>
<summary>Ver Structurizr DSL</summary>

````text
workspace "Flow" "Distributed transaction system" {

    model {

        user = person "Usuário autenticado"

        keycloak = softwareSystem "Keycloak"
        rabbitmq = softwareSystem "RabbitMQ"

        flow = softwareSystem "Flow" {

            web = container "Flow.Web.Blazor" "UI" "Blazor Server"

            transactionsApi = container "Transactions API" "Lançamentos e processamento assíncrono" "ASP.NET Core"

            reportsApi = container "Reports API" "Consultas e projeções" "ASP.NET Core"

            transactionsDb = container "Transactions DB" "Dados de lançamentos" "PostgreSQL"

            reportsDb = container "Reports DB" "Dados de projeções" "PostgreSQL"
        }

        user -> web "Usa interface"

        web -> transactionsApi "HTTP"
        web -> reportsApi "HTTP"

        transactionsApi -> keycloak "Auth"
        reportsApi -> keycloak "Auth"

        transactionsApi -> transactionsDb "Persistência"
        reportsApi -> reportsDb "Persistência"

        transactionsApi -> rabbitmq "Eventos"
        reportsApi -> rabbitmq "Eventos"
    }

    views {

        systemContext flow {
            include *
            autolayout lr
            title "Flow - Context"
        }

        container flow {
            include *
            autolayout lr
            title "Flow - Containers"
        }

        theme default
    }
}
````

</details>

---

## Contexto do sistema

O usuário autenticado interage com o sistema Flow através de uma aplicação web Blazor. Todas as operações passam por autenticação centralizada via Keycloak utilizando OIDC/JWT.

O sistema depende de dois componentes externos principais:

- **Keycloak**: responsável por autenticação e autorização.
- **RabbitMQ**: utilizado como broker para comunicação assíncrona entre serviços.

O PostgreSQL é utilizado como persistência principal, separada por contexto.

---

## Containers

### Flow.Web.Blazor
Interface do usuário responsável por autenticação e interação com APIs.

### Transactions API
Responsável pelo ciclo de vida dos lançamentos financeiros (write model).

### Reports API
Responsável pelas consultas e projeções (read model).

### Transactions DB
Armazena dados transacionais.

### Reports DB
Armazena projeções de leitura.

---

## Fluxo de comunicação

1. Usuário acessa a Web.
2. Web autentica via Keycloak.
3. Web consome Transactions API e Reports API via HTTP.
4. Transactions API persiste dados e publica eventos.
5. Reports API consome eventos e atualiza projeções.
6. Consultas são realizadas no Reports DB.

---

## Decisões arquiteturais

- Autenticação centralizada no Keycloak (OIDC/JWT).
- Separação entre write model (Transactions) e read model (Reports).
- Comunicação assíncrona via RabbitMQ.
- Bancos separados por contexto.
- Consistência eventual nas projeções.
