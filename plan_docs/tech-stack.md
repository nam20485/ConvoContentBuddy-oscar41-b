# ConvoContentBuddy - Technology Stack

## Overview

ConvoContentBuddy is an autonomous background listener designed to assist users in real-time during technical interviews or coding sessions. This document outlines the complete technology stack used for the application.

---

## Languages

| Language | Version | Purpose |
|----------|---------|---------|
| **C#** | 14 (.NET 10) | Primary backend and frontend language |
| **JavaScript** | ES6+ | Browser interop for Web Speech API |
| **SQL** | PostgreSQL dialect | Database queries and graph operations |

---

## Frameworks

### Core Frameworks

| Framework | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 10.0 | Runtime and SDK |
| **ASP.NET Core** | 10 | Backend Web API |
| **Blazor WebAssembly** | 10 | Frontend UI framework |
| **.NET Aspire** | 10 | Cloud-native orchestration |

### Aspire Packages

| Package | Purpose |
|---------|---------|
| `Aspire.Hosting.AppHost` | Application orchestrator |
| `Aspire.Hosting.Redis` | Redis container integration |
| `Aspire.Hosting.PostgreSQL` | PostgreSQL container integration |
| `Aspire.Hosting.Qdrant` | Qdrant vector DB integration |

---

## AI/LLM Stack

| Component | Library | Purpose |
|-----------|---------|---------|
| **Semantic Kernel** | `Microsoft.SemanticKernel` | AI orchestration and prompt management |
| **Extensions AI** | `Microsoft.Extensions.AI` | AI abstraction layer |
| **Embedding Model** | Gemini `text-embedding-004` | Vector embeddings (1536 dimensions) |
| **Chat Model** | Gemini `2.5 Flash` | Intent analysis and verification |
| **Search Grounding** | Google Search via Gemini API | Real-time solution retrieval |

---

## Data & Storage

### Vector Database

| Component | Details |
|-----------|---------|
| **Engine** | Qdrant |
| **Connection** | gRPC |
| **Library** | `Qdrant.Client` |
| **Collection** | `leetcode_problems` |
| **Dimensions** | 1536 (Cosine similarity) |

### Relational Database

| Component | Details |
|-----------|---------|
| **Engine** | PostgreSQL |
| **Extension** | pgvector |
| **ORM** | Entity Framework Core |
| **Provider** | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| **Schema** | Problems table, ProblemEdges table (adjacency list) |

### Caching & State

| Component | Details |
|-----------|---------|
| **Engine** | Redis |
| **Purpose** | SignalR backplane for TMR sync |
| **Library** | `Microsoft.AspNetCore.SignalR.StackExchangeRedis` |

---

## Resilience & Reliability

| Component | Library | Purpose |
|-----------|---------|---------|
| **Polly** | `Microsoft.Extensions.Http.Resilience` | Retry, circuit breaker, timeout policies |
| **Health Checks** | `Microsoft.Extensions.Diagnostics.HealthChecks` | Liveness/readiness probes |
| **OpenTelemetry** | Built-in .NET 10 | Distributed tracing and metrics |

### Resilience Policies

| Policy | Configuration |
|--------|---------------|
| **Retry** | Exponential backoff (1s, 2s, 4s, 8s, 16s) |
| **Circuit Breaker** | 30s break duration |
| **Timeout** | Configurable per operation |

---

## Frontend Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| **UI Framework** | Blazor WebAssembly | C#-based SPA |
| **Styling** | Tailwind CSS | Utility-first CSS |
| **Real-time** | SignalR Client | WebSocket communication |
| **Speech API** | webkitSpeechRecognition (JS Interop) | Browser audio capture |

---

## Observability

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Tracing** | OpenTelemetry (OTLP) | Distributed request tracing |
| **Metrics** | System.Diagnostics.Metrics | Performance counters |
| **Logging** | .NET ILogger | Structured logging |
| **Dashboard** | Aspire Dashboard | Real-time service monitoring |

---

## Containerization

| Component | Technology |
|-----------|------------|
| **Container Runtime** | Docker / Podman |
| **Orchestration** | .NET Aspire AppHost |
| **Deployment** | Azure Developer CLI (azd) |

---

## Development Tools

| Tool | Purpose |
|------|---------|
| **dotnet CLI** | Build, test, run |
| **dotnet format** | Code style enforcement |
| **GitHub CLI (gh)** | Issue/PR management |
| **Docker Compose** | Local development (via Aspire) |

---

## Package Dependencies Summary

```xml
<!-- Core framework references are declared in the SDK, not as PackageReference items.
     Web projects use Sdk="Microsoft.NET.Sdk.Web" and inherit Microsoft.AspNetCore.App
     as a framework reference automatically. -->

<!-- Aspire -->
<PackageReference Include="Aspire.Hosting.AppHost" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.Redis" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Aspire.Hosting.Qdrant" Version="9.0.0" />

<!-- AI/LLM -->
<PackageReference Include="Microsoft.SemanticKernel" Version="1.0.0" />
<PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0" />

<!-- Data -->
<PackageReference Include="Qdrant.Client" Version="1.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="10.0.0" />

<!-- Resilience -->
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.0.0" />
```

---

## Version Requirements

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor",
    "allowPrerelease": false
  },
  "msbuild-sdks": {
    "Microsoft.NET.Sdk.WebAssembly": "10.0.100"
  }
}
```

---

## External Service Dependencies

| Service | Purpose | Required |
|---------|---------|----------|
| Google Gemini API | LLM inference and embeddings | Yes |
| Google Search Grounding | Real-time solution retrieval | Yes (Tier 1) |
| Azure OpenAI | Fallback LLM (Tier 2) | Optional |

---

*Last Updated: February 2026*
