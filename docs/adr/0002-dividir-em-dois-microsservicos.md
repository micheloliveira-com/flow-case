# ADR-0002 - Dividir a solução em dois microsserviços

## Status

Aceita.

## Contexto

O case técnico define dois requisitos de negócio claros:

- Serviço de controle de lançamentos.
- Serviço de consolidado diário.

Também existe um requisito não funcional importante: o controle de lançamentos não deve ficar indisponível caso o consolidado diário caia.

## Decisão

Separar a solução em dois microsserviços:

- `Transactions`: responsável pelo ciclo de vida dos lançamentos financeiros.
- `Reports`: responsável por expor a consulta do saldo diário consolidado.

Cada serviço possui suas próprias camadas de domínio, aplicação, infraestrutura, API e testes.

## Consequências positivas

- O serviço de lançamentos não depende diretamente da disponibilidade do serviço de relatórios.
- Cada contexto pode evoluir com regras, persistência e escala independentes.
- O modelo deixa explícita a decomposição de domínios esperada para o case.
- Permite escalar o consolidado diário sem escalar necessariamente o CRUD de lançamentos.

## Trade-offs

- A solução fica mais complexa do que um monolito modular.
- A consistência entre lançamentos e saldo diário passa a ser eventual.
- Exige mensageria, observabilidade e tratamento de falhas de integração.

## Alternativas consideradas

- Monolito modular: seria mais simples, mas atenderia pior ao requisito de isolamento de disponibilidade.
- Um único serviço com duas tabelas: reduziria infraestrutura, mas manteria acoplamento operacional.
- Serverless: poderia escalar bem, mas adicionaria dependência de plataforma e não favoreceria a execução local da solução.
