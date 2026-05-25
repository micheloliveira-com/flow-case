# ADR-0007 - Observabilidade com Seq

## Status

Aceita.

## Contexto

A solução possui múltiplos processos: frontend, duas APIs, workers, RabbitMQ, Keycloak e bancos separados. Apenas visualizar logs no terminal ou no dashboard do Aspire ajuda durante a execução, mas dificulta consultar eventos estruturados, filtrar por propriedades e investigar fluxos assíncronos entre serviços.

Como a aplicação já usa `ILogger` em pontos relevantes dos casos de uso, workers, consumidores, publicadores e clientes, faz sentido direcionar esses logs para uma ferramenta própria de consulta.

## Decisão

Usar Seq como coletor de logs estruturados no ambiente orquestrado pelo .NET Aspire.

O `Flow.Aspire.AppHost` declara o recurso `seq` e referencia esse recurso nas APIs:

- `transactionsapiservice`
- `reportsapiservice`

As APIs configuram a integração com:

```csharp
builder.AddSeqEndpoint(connectionName: "seq");
```

No AppHost, o recurso é configurado com lifetime persistente para preservar logs locais entre execuções. O recurso também usa `ExcludeFromManifest`, pois esta decisão cobre a experiência do case técnico e não define, por si só, a estratégia de observabilidade de produção.

## Consequências positivas

- Consulta centralizada dos logs estruturados das APIs.
- Melhor diagnóstico de fluxos assíncronos envolvendo RabbitMQ, workers e projeção de relatórios.
- Facilidade para filtrar eventos por propriedades como data, identificador de transação, fila e tipo de operação.
- Integração simples com o AppHost Aspire, mantendo a experiência com um único comando.

## Trade-offs

- Adiciona mais um container ao ambiente.
- Exige atenção ao volume e retenção de logs se evoluir para produção.
- A configuração aceita a licença do Seq automaticamente para facilitar a execução do case.
- Como o recurso está fora do manifest, a estratégia produtiva precisa ser definida separadamente.

## Evolução para produção

- Definir retenção, storage e política de acesso.
- Proteger a interface do Seq ou substituir por uma solução corporativa de observabilidade.
- Correlacionar logs com traces distribuídos e métricas.
- Criar dashboards e alertas para falhas de consumo, atraso de filas, erros de domínio e indisponibilidade de serviços.
