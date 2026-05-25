# ADR-0003 - Comunicação orientada a eventos com RabbitMQ

## Status

Aceita.

## Contexto

O consolidado diário não deve impactar a disponibilidade do controle de lançamentos. Além disso, o relatório pode receber picos de carga e não precisa ser atualizado de forma estritamente sincronizada no mesmo request de escrita.

## Decisão

Usar comunicação assíncrona orientada a eventos com RabbitMQ.

Fluxo adotado:

1. `Transactions` persiste o lançamento.
2. `Transactions` publica `transaction-daily-recompute` com a data afetada.
3. Um worker recalcula o saldo da data com base na fonte transacional.
4. O worker publica `transaction-daily-balance`.
5. `Reports` consome a mensagem e atualiza a projeção de saldo diário.

## Consequências positivas

- Desacoplamento temporal entre escrita de lançamentos e consolidação.
- Maior resiliência quando o serviço de relatórios está fora do ar.
- Possibilidade de absorver picos com fila.
- Reprocessamento em caso de falha de consumo via `nack` com requeue.
- Base adequada para evoluir com DLQ, retries e backpressure.

## Trade-offs

- O saldo diário se torna eventualmente consistente.
- A operação passa a depender de broker de mensagens.
- É necessário lidar com duplicidade, ordenação, reprocessamento e mensagens antigas.

## Observações de confiabilidade

O consumidor de relatórios compara `ProcessedAt` para ignorar mensagens antigas. Isso reduz o risco de uma consolidação defasada sobrescrever uma mais recente.

Para produção, a decisão deve ser complementada com:

- Outbox Pattern para atomicidade entre banco e evento.
- Mensagens persistentes no broker.
- Dead-letter queue.
- Retry com backoff.
- Monitoramento de lag da fila.
- Testes de carga para validar o requisito de 50 requisições por segundo e perda máxima de 5%.

## Alternativas consideradas

- Chamada HTTP síncrona entre serviços: mais simples, mas violaria o isolamento de disponibilidade.
- Banco compartilhado: reduziria integração, mas aumentaria acoplamento e quebraria autonomia dos serviços.
- Kafka: robusto para alto volume e stream processing, mas RabbitMQ é suficiente e mais simples para o escopo da solução.
