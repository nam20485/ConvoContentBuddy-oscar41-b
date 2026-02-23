# ConvoContentBuddy - System Architecture

## Overview

ConvoContentBuddy is an autonomous background listener designed to provide real-time programming interview assistance. The system processes live speech audio, isolates semantic intent, retrieves algorithmic problems from a hybrid vector/graph database, and presents optimal code solutions in a zero-interaction, ambient UI.

---

## High-Level Architecture

The system follows a **four-layer architecture** with aerospace-grade resilience patterns:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           LAYER 4: AMBIENT UI                                │
│                    Blazor WASM + SignalR Client                              │
│              (Zero-interaction, real-time dashboard)                         │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ SignalR WebSocket (Redis Backplane)
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      LAYER 3: RESOURCE RETRIEVAL                             │
│                    Hybrid Vector-Graph Search Chain                          │
│         Qdrant (Semantic) → PostgreSQL (Graph) → Gemini (Verify)             │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ gRPC / SQL / REST
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                  LAYER 2: CONTEXT & INTENT ANALYSIS                          │
│                         "The Brain" API (TMR)                                │
│        Semantic Kernel + Microsoft.Extensions.AI + Polly Failover            │
│              ┌─────────┬─────────┬─────────┐                                 │
│              │ API #1  │ API #2  │ API #3  │  (Triple Modular Redundancy)    │
│              └─────────┴─────────┴─────────┘                                 │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ REST / WebSocket
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                   LAYER 1: AUDIO INPUT & TRANSCRIPTION                       │
│                    Browser Web Speech API (JS Interop)                       │
│              webkitSpeechRecognition → Transcript Buffer                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Layer Details

### Layer 1: Audio Input & Transcription

**Responsibility:** Capture continuous audio streams and convert to timestamped text segments.

| Component | Technology | Purpose |
|-----------|------------|---------|
| Speech Recognition | webkitSpeechRecognition (JS) | Browser-native audio capture |
| Interop Layer | speechInterop.js | Bridge between JS and Blazor |
| Buffer Manager | Blazor Component | Accumulate and debounce transcripts |

**Data Flow:**
1. Browser captures microphone audio
2. Web Speech API produces text segments
3. JavaScript interop passes segments to Blazor
4. Client buffers until threshold (100 chars or timer)
5. Buffered text sent to API for analysis

---

### Layer 2: Context & Intent Analysis ("The Brain")

**Responsibility:** Evaluate transcript buffers to identify intents and coordinate the hybrid retrieval pipeline.

| Component | Technology | Purpose |
|-----------|------------|---------|
| API Framework | ASP.NET Core 10 | REST endpoints and SignalR hub |
| AI Orchestration | Microsoft.SemanticKernel | Prompt execution and plugin management |
| AI Abstraction | Microsoft.Extensions.AI | Model-agnostic AI operations |
| Resilience | Polly | Retry, circuit breaker, failover |
| Real-time | SignalR + Redis Backplane | WebSocket push to clients |

**Triple Modular Redundancy (TMR):**
- 3 identical API instances running in parallel
- Redis backplane ensures SignalR state synchronization
- If 1-2 instances fail, remaining instances continue serving
- Health checks trigger automatic restarts within 5 seconds

**Key Services:**
- `HybridRetrieverService` - Coordinates the search pipeline
- `ModelFailoverManager` - Manages AI tier switching
- `BuddyHub` - SignalR hub for real-time updates

---

### Layer 3: Resource Retrieval

**Responsibility:** Execute hybrid search chain to identify and verify coding problems.

**The Hybrid Chain:**

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Embed      │───▶│ Vector Search│───▶│ Graph Expand │───▶│ LLM Verify   │
│  (Gemini)    │    │  (Qdrant)    │    │ (PostgreSQL) │    │  (Gemini)    │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
      │                    │                   │                    │
      ▼                    ▼                   ▼                    ▼
  1536-dim          Top-3 candidates    Related problems     Confirmed match
  vector            + scores            + context             + confidence
```

**Components:**

| Component | Technology | Purpose |
|-----------|------------|---------|
| VectorSearchProvider | Qdrant.Client (gRPC) | Semantic similarity search |
| GraphTraversalProvider | EF Core + PostgreSQL | Problem relationship expansion |
| HybridRetrieverService | Custom service | Pipeline orchestration |
| LLM Judge | Gemini 2.5 Flash | Match verification |

**Performance Target:** Sub-500ms for complete hybrid chain execution.

---

### Layer 4: Ambient UI

**Responsibility:** Display real-time updates without requiring user interaction.

| Component | Technology | Purpose |
|-----------|------------|---------|
| UI Framework | Blazor WebAssembly | C#-based SPA |
| Styling | Tailwind CSS | Responsive design |
| Real-time | SignalR Client | WebSocket event handling |
| Code Display | Syntax highlighting | Solution visualization |

**UI Components:**
- **Live Transcript Feed** - Real-time speech visualization
- **Active Problem Card** - Detected challenge details
- **Solution Panel** - Code snippets with complexity analysis
- **System Status** - Failover and health indicators

---

## Design Decisions

### 1. Triple Modular Redundancy (TMR)

**Decision:** Run 3 identical API instances with Redis-backed SignalR synchronization.

**Rationale:**
- Aerospace-grade reliability requirement
- Zero-downtime failover for active sessions
- Automatic recovery from single-instance failures

**Implementation:**
```csharp
// AppHost configuration
builder.AddProject<Projects.API_Brain>("api-brain")
    .WithReplicas(3)
    .WithReference(redis);
```

---

### 2. Hybrid Vector-Graph Search

**Decision:** Combine Qdrant vector search with PostgreSQL graph traversal.

**Rationale:**
- Vector search provides semantic matching (handles paraphrasing)
- Graph expansion provides context (related problems, complexity chains)
- Combined approach achieves 95%+ accuracy target

**Data Model:**
```sql
-- PostgreSQL Schema
CREATE TABLE problems (
    id SERIAL PRIMARY KEY,
    leetcode_id INTEGER UNIQUE,
    title VARCHAR(255),
    difficulty VARCHAR(50),
    description TEXT
);

CREATE TABLE problem_edges (
    source_id INTEGER REFERENCES problems(id),
    target_id INTEGER REFERENCES problems(id),
    relationship VARCHAR(100),
    PRIMARY KEY (source_id, target_id)
);
```

---

### 3. N+2 Failover Strategy

**Decision:** Implement 3-tier failover for AI operations.

**Tiers:**

| Tier | Mode | Behavior |
|------|------|----------|
| **Tier 1** | Gemini 2.5 Flash + Search Grounding | Full capability, real-time search |
| **Tier 2** | Fallback Model/Region | Reduced capability, cached responses |
| **Tier 3** | Deterministic Safe Mode | Local-only, vector match with low confidence flag |

**Implementation:**
```csharp
// Polly failover policy
var failoverPolicy = Policy<ProblemMatch>
    .Handle<Exception>()
    .FallbackAsync(
        fallbackAction: async (ct) => await SafeModeMatchAsync(ct),
        onFallbackAsync: async (ex) => await LogFailoverAsync(ex)
    );
```

---

### 4. Zero-Interaction UI

**Decision:** UI updates autonomously via SignalR push events.

**Rationale:**
- User focus should remain on interview/conversation
- Solutions appear organically as context evolves
- No clicks or manual triggers required

**Event Flow:**
```
User speaks → Transcript buffered → API analyzes → Problem identified
    → Solution retrieved → SignalR push → UI renders card
```

---

## Component Interactions

### Sequence Diagram: Problem Detection Flow

```
┌──────┐     ┌──────┐     ┌─────────┐     ┌────────┐     ┌──────────┐     ┌──────┐
│Browser│     │Blazor│     │API.Brain│     │ Qdrant │     │PostgreSQL│     │Gemini│
└──┬───┘     └──┬───┘     └────┬────┘     └───┬────┘     └────┬─────┘     └──┬───┘
   │            │              │              │               │              │
   │ Audio      │              │              │               │              │
   │───────────▶│              │              │               │              │
   │            │ Transcript   │              │               │              │
   │            │─────────────▶│              │               │              │
   │            │              │ Embed        │               │              │
   │            │              │─────────────────────────────────────────────▶│
   │            │              │ Vector       │               │              │
   │            │              │◀─────────────────────────────────────────────│
   │            │              │ Search       │               │              │
   │            │              │─────────────▶│               │              │
   │            │              │ Candidates   │               │              │
   │            │              │◀─────────────│               │              │
   │            │              │ Graph Expand │               │              │
   │            │              │─────────────────────────────▶│              │
   │            │              │ Related      │               │              │
   │            │              │◀─────────────────────────────│              │
   │            │              │ Verify       │               │              │
   │            │              │─────────────────────────────────────────────▶│
   │            │              │ Confirmed    │               │              │
   │            │              │◀─────────────────────────────────────────────│
   │            │ SignalR Push │              │               │              │
   │            │◀─────────────│              │               │              │
   │ Render     │              │              │               │              │
   │◀───────────│              │              │               │              │
```

---

## Project Structure

```
ConvoContentBuddy/
├── ConvoContentBuddy.sln
├── global.json
├── src/
│   ├── ConvoContentBuddy.AppHost/           # Aspire orchestrator
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── ConvoContentBuddy.ServiceDefaults/   # Shared configuration
│   │   ├── Extensions.cs
│   │   └── Resilience.cs
│   │
│   ├── ConvoContentBuddy.API.Brain/         # Core API (TMR)
│   │   ├── Program.cs
│   │   ├── Hubs/
│   │   │   └── BuddyHub.cs
│   │   ├── Services/
│   │   │   ├── HybridRetrieverService.cs
│   │   │   ├── ModelFailoverManager.cs
│   │   │   ├── VectorSearchProvider.cs
│   │   │   └── GraphTraversalProvider.cs
│   │   └── Controllers/
│   │
│   ├── ConvoContentBuddy.UI.Web/            # Blazor WASM frontend
│   │   ├── Program.cs
│   │   ├── wwwroot/
│   │   │   ├── index.html
│   │   │   └── js/
│   │   │       └── speechInterop.js
│   │   ├── Pages/
│   │   ├── Components/
│   │   └── Services/
│   │       └── TranscriptBufferService.cs
│   │
│   └── ConvoContentBuddy.Data.Seeder/       # Knowledge base seeder
│       ├── Program.cs
│       └── Services/
│           ├── LeetCodeScraper.cs
│           └── EmbeddingService.cs
│
├── tests/
│   ├── ConvoContentBuddy.UnitTests/
│   └── ConvoContentBuddy.IntegrationTests/
│
├── plan_docs/
│   ├── tech-stack.md
│   ├── architecture.md
│   └── ...
│
└── .github/
    └── workflows/
```

---

## Resilience Patterns

### Health Check Endpoints

| Endpoint | Purpose |
|----------|---------|
| `/health/live` | Liveness probe (container restart) |
| `/health/ready` | Readiness probe (traffic routing) |

### Circuit Breaker States

```
         ┌──────────────┐
         │   CLOSED     │ ◀── Normal operation
         │  (requests)  │
         └──────┬───────┘
                │ failures > threshold
                ▼
         ┌──────────────┐
         │    OPEN      │ ◀── Failing fast
         │  (block)     │
         └──────┬───────┘
                │ timeout elapsed
                ▼
         ┌──────────────┐
         │  HALF-OPEN   │ ◀── Testing recovery
         │  (probe)     │
         └──────────────┘
```

---

## Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| Vector Search Latency | < 200ms | Qdrant gRPC response time |
| Hybrid Chain Latency | < 500ms | End-to-end retrieval |
| E2E Processing | < 2s | Transcript to UI card |
| Problem Identification Accuracy | > 95% | Benchmark test suite |
| System Uptime | 99.9% | Health check monitoring |

---

## Security Considerations

| Area | Implementation |
|------|----------------|
| API Keys | Environment variables / Azure Key Vault |
| CORS | Configured for Blazor WASM origin |
| SignalR | Authentication optional (local dev) |
| Database | Connection string encryption |

---

*Last Updated: February 2026*
