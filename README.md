# ConvoContentBuddy

An autonomous background listener designed to assist users in real-time during technical interviews or coding sessions. By seamlessly transcribing audio and analyzing intents, it identifies the specific algorithmic problem (e.g., LeetCode) being discussed and automatically retrieves optimal solutions, complexities, and logic via the Gemini API—all without requiring manual user input.

## Features

- **Zero-Interaction UI:** The dashboard updates organically without mouse/keyboard input.
- **Live Transcript Feed:** Real-time visualization of recognized speech.
- **Hybrid Vector-Graph Search:** Sub-second algorithmic matching using Cosine similarity.
- **Active Problem Card & Solution Panel:** Code snippets with syntax highlighting that automatically adapt to the conversation's context.
- **Aerospace-Grade Resilience (TMR):** Triple Modular Redundancy with graceful failovers.

## Technology Stack

| Component | Technology |
|-----------|------------|
| **Runtime** | .NET 10 |
| **Backend** | ASP.NET Core 10 Web API |
| **Frontend** | Blazor WebAssembly 10 |
| **Orchestration** | .NET Aspire 10 |
| **AI/LLM** | Microsoft.SemanticKernel, Microsoft.Extensions.AI |
| **Vector DB** | Qdrant (gRPC) |
| **Relational DB** | PostgreSQL with pgvector |
| **Caching** | Redis (SignalR backplane) |
| **Resilience** | Polly |

## Project Structure

```
ConvoContentBuddy/
├── ConvoContentBuddy.sln
├── global.json
├── src/
│   ├── ConvoContentBuddy.AppHost/           # Aspire orchestrator
│   ├── ConvoContentBuddy.ServiceDefaults/   # Shared configuration (OTLP, Health Checks)
│   ├── ConvoContentBuddy.API.Brain/         # Core API (TMR - 3 replicas)
│   ├── ConvoContentBuddy.UI.Web/            # Blazor WASM frontend
│   └── ConvoContentBuddy.Data.Seeder/       # Knowledge base seeder
├── tests/
│   ├── ConvoContentBuddy.API.Brain.Tests/
│   └── ConvoContentBuddy.UI.Web.Tests/
├── docker/
│   ├── Dockerfile.api                       # Dockerfile for API.Brain
│   ├── Dockerfile.web                       # Dockerfile for UI.Web
│   ├── nginx.conf                           # Nginx config for Blazor WASM
│   └── docker-compose.yml                   # Local development environment
├── docs/
│   └── architecture/                        # Architecture Decision Records
└── plan_docs/
    ├── tech-stack.md
    ├── architecture.md
    └── New Application Spec_ ConvoContentBuddy.md
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) or [Podman](https://podman.io/)
- Google Gemini API key

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/nam20485/ConvoContentBuddy-oscar41-b.git
cd ConvoContentBuddy-oscar41-b
```

### 2. Local Development with Aspire

```bash
# Run the Aspire AppHost (orchestrates all services)
dotnet run --project src/ConvoContentBuddy.AppHost
```

This will start:
- API.Brain (3 replicas for TMR)
- UI.Web (Blazor WASM)
- PostgreSQL
- Qdrant
- Redis

### 3. Local Development with Docker Compose

```bash
# Start all infrastructure services
cd docker && docker-compose up -d && cd ..

# Build and run the API (run from the repository root)
dotnet run --project src/ConvoContentBuddy.API.Brain

# Build and run the UI (run from the repository root)
dotnet run --project src/ConvoContentBuddy.UI.Web
```

### 4. Configuration

Set the following environment variables:

```bash
# Required for AI features
GEMINI_API_KEY=your-gemini-api-key

# Database connections (auto-configured in Aspire)
ConnectionStrings__postgres=Host=localhost;Port=5432;Database=convocontentbuddy;Username=convocontentbuddy;Password=convocontentbuddy
QDRANT__HOST=localhost
QDRANT__PORT=6334
REDIS__HOST=localhost
REDIS__PORT=6379
```

## Development Commands

```bash
# Build the solution
dotnet build ConvoContentBuddy.sln -warnaserror

# Run code style verification
dotnet format ConvoContentBuddy.sln --verify-no-changes

# Run tests
dotnet test ConvoContentBuddy.sln --no-build

# Run all verification (build + format + test)
dotnet build ConvoContentBuddy.sln -warnaserror && \
dotnet format ConvoContentBuddy.sln --verify-no-changes && \
dotnet test ConvoContentBuddy.sln --no-build
```

## Architecture

The system follows a **four-layer architecture** with aerospace-grade resilience patterns:

1. **Audio Input & Transcription Layer:** Browser-based Web Speech API capturing continuous streams.
2. **Context & Intent Analysis Layer ("The Brain"):** Evaluates transcript buffers to identify intents, powered by Semantic Kernel and Gemini 2.5 Flash.
3. **Resource Retrieval Layer:** A hybrid chain executing a Qdrant semantic search, PostgreSQL graph expansion, and Google Search Grounding.
4. **Ambient UI Layer:** A Blazor WASM application that reacts autonomously to incoming SignalR events.

For detailed architecture documentation, see [docs/architecture/](docs/architecture/).

## Performance Targets

| Metric | Target |
|--------|--------|
| Vector Search Latency | < 200ms |
| Hybrid Chain Latency | < 500ms |
| E2E Processing | < 2s |
| Problem Identification Accuracy | > 95% |
| System Uptime | 99.9% |

## Contributing

1. Create a feature branch from `develop`
2. Make your changes
3. Ensure all tests pass: `dotnet test`
4. Submit a pull request

## License

See [LICENSE.md](LICENSE.md) for details.
