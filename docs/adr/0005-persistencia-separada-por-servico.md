# ADR-0005 - Persistência separada por serviço

## Status

Aceita.

## Contexto

Em uma arquitetura de microsserviços, compartilhar o mesmo banco entre contextos aumenta acoplamento e dificulta autonomia de evolução. O contexto de lançamentos é fonte transacional; o contexto de relatórios mantém uma projeção de leitura derivada.

## Decisão

Usar bancos PostgreSQL separados:

- `transactionsapiservicedb`: banco do serviço `Transactions`.
- `reportsapiservicedb`: banco do serviço `Reports`.

Cada serviço executa suas migrations no startup e acessa apenas seu próprio banco.

## Consequências positivas

- Autonomia de schema por serviço.
- Menor acoplamento entre domínios.
- Possibilidade de escalar leitura de relatórios independentemente da escrita de lançamentos.
- Facilita evolução futura para estratégias diferentes de armazenamento por contexto.

## Trade-offs

- Não há transação distribuída entre os dois bancos.
- A consistência entre escrita e relatório é eventual.
- Debug e reconciliação exigem melhor observabilidade.

## Alternativas consideradas

- Banco único compartilhado: simples, mas inadequado para autonomia de microsserviços.
- Mesmo PostgreSQL com schemas separados: reduziria containers, mas manteria acoplamento operacional maior.
- Banco de leitura especializado para relatórios: interessante para escala, mas desnecessário neste momento.
