# ADR-0006 - Keycloak, OIDC e JWT para autenticação

## Status

Aceita.

## Contexto

O case técnico menciona segurança como requisito não funcional. A solução precisa proteger APIs e demonstrar um mecanismo realista de autenticação/autorização, mesmo em ambiente local.

## Decisão

Usar Keycloak como provedor de identidade, integrado ao .NET Aspire.

- O frontend Blazor autentica usuários via OpenID Connect.
- As APIs validam tokens JWT Bearer emitidos pelo Keycloak.
- O realm `flow` é importado automaticamente pelo AppHost.
- A política padrão das APIs exige usuário autenticado.
- Cada API valida o claim de audience do JWT de forma específica: `flow.transactions.api` para a Transactions API e `flow.reports.api` para a Reports API.

## Consequências positivas

- Separação entre aplicação e identidade.
- Uso de padrões abertos: OIDC e JWT.
- Ambiente local reproduz um fluxo realista de login e chamada autenticada entre frontend e APIs.
- Facilita evolução para roles, scopes e políticas granulares.

## Trade-offs

- Adiciona um componente operacional a mais.
- A configuração local relaxa validação HTTPS para facilitar desenvolvimento.
- O modelo atual é suficiente para autenticação, mas ainda não implementa autorização fina por perfil/permissão.

## Evolução para produção

- Exigir HTTPS e metadata segura.
- Armazenar secrets fora do código.
- Definir scopes e roles por operação.
- Aplicar autorização por política nos endpoints.
- Integrar com provedor corporativo de identidade compatível com OIDC, se necessário.
