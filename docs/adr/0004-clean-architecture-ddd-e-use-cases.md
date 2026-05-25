# ADR-0004 - Clean Architecture, DDD tático e use cases

## Status

Aceita.

## Contexto

O desafio valoriza boas práticas, SOLID, design patterns, decomposição de domínio e segregação de responsabilidades. A solução precisa demonstrar clareza arquitetural sem depender apenas de frameworks.

## Decisão

Organizar cada contexto com camadas inspiradas em Clean Architecture:

- `Domain`: entidades, regras e invariantes.
- `Application`: casos de uso, contratos de repositório e contratos de mensageria.
- `Infrastructure`: Entity Framework, RabbitMQ, repositórios e implementações externas.
- `ApiService`: endpoints HTTP, autenticação, DI, workers e composição da aplicação.
- `Tests`: testes automatizados do contexto.

Os fluxos de negócio são modelados como use cases, por exemplo:

- `CreateTransactionService`
- `UpdateTransactionService`
- `DeleteTransactionService`
- `GetTransactionsService`
- `ExecuteTransactionDailyRecomputeService`
- `GetTransactionDailyBalance`
- `ExecuteTransactionDailyBalanceService`

## Consequências positivas

- Regras de negócio ficam isoladas de detalhes de infraestrutura.
- Casos de uso ficam explícitos e testáveis.
- Dependências apontam para abstrações, preservando inversão de dependência.
- Facilita manutenção, evolução e leitura durante uma avaliação técnica.

## Trade-offs

- Mais projetos e arquivos do que uma API simples.
- Exige disciplina para não vazar detalhes de infraestrutura para domínio/aplicação.
- Pode parecer mais verboso para um desafio pequeno, mas evidencia as decisões esperadas para uma vaga de arquitetura.

## Alternativas consideradas

- Minimal API com acesso direto ao DbContext: mais rápido, mas menos expressivo arquiteturalmente.
- CQRS completo com mediador: seria válido, mas adicionaria uma dependência e complexidade que não eram necessárias para o escopo atual.
