# ADR-001: Project Structure and Technology Stack

## Status

Accepted

## Context

ConvoContentBuddy requires a highly resilient, AI-powered architecture to provide real-time programming interview assistance. The system must:
- Process live speech audio with sub-second latency
- Identify algorithmic problems from conversational descriptions
- Support Triple Modular Redundancy (TMR) for aerospace-grade reliability
- Provide a zero-interaction, ambient UI experience

## Decision

We will use a .NET 10 Aspire-orchestrated microservices architecture with the following components:

### Project Structure

```
ConvoContentBuddy.sln
├── src/
│   ├── ConvoContentBuddy.AppHost/           (Aspire Orchestrator)
│   ├── ConvoContentBuddy.ServiceDefaults/   (OTLP, Health Checks, Resilience)
│   ├── ConvoContentBuddy.API.Brain/         (ASP.NET Core Web API)
│   ├── ConvoContentBuddy.UI.Web/            (Blazor WASM)
│   └── ConvoContentBuddy.Data.Seeder/       (Worker Service)
└── tests/
    ├── ConvoContentBuddy.API.Brain.Tests/
    └── ConvoContentBuddy.UI.Web.Tests/
```

### Technology Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Runtime | .NET 10 | Latest LTS with performance improvements |
| Backend | ASP.NET Core 10 | Native Aspire integration, SignalR support |
| Frontend | Blazor WASM | C#-based SPA, easy interop with Web Speech API |
| Orchestration | .NET Aspire | Built-in service discovery, health checks, TMR support |
| AI/LLM | Microsoft.SemanticKernel | Unified AI abstraction, plugin architecture |
| Vector DB | Qdrant | High-performance gRPC, cosine similarity |
| Relational DB | PostgreSQL + pgvector | Graph relationships, hybrid search |
| Caching | Redis | SignalR backplane for TMR sync |

## Consequences

### Positive
- Single language (C#) across frontend and backend
- Built-in resilience patterns via Aspire
- Easy local development with Docker Compose fallback
- Strong typing and compile-time safety

### Negative
- Requires .NET 10 SDK (preview)
- Blazor WASM has larger initial payload
- Learning curve for Aspire orchestration

### Risks
- Microsoft.SemanticKernel has known vulnerabilities (tracked for resolution)
- .NET 10 is in preview (may have breaking changes)

## References

- [plan_docs/tech-stack.md](../../plan_docs/tech-stack.md)
- [plan_docs/architecture.md](../../plan_docs/architecture.md)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
