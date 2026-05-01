# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Prophet** is an academic LLM-powered document processing pipeline that transforms requirement documents into structured technical artifacts (domain models, architecture proposals, Mermaid diagrams, web/mobile PoCs, and documentation).

- **Backend**: .NET 10, ASP.NET Core, port 7017
- **Frontend**: Next.js 16, port 4000
- **State**: Entirely in-memory — all state is lost on restart by design (no database)
- **Auth**: None — all endpoints open for local academic use

## Commands

### Backend

```bash
cd backend

# Run the API
dotnet run --project src/Prophet.Api

# Run unit tests
dotnet test src/Prophet.Tests/

# Run E2E tests
dotnet test src/Prophet.Tests.E2E/

# Run a single test
dotnet test src/Prophet.Tests/ --filter "FullyQualifiedName~TestClassName"
```

### Frontend

```bash
cd frontend
npm install

# Development server (Turbopack, port 4000)
npm run dev

# Production build
npm run build && npm start

# Run tests
npm run test:run

# Lint + typecheck + format + test
npm run check

# Run a single test file
npx vitest run path/to/test.ts
```

### LLM Configuration (User Secrets)

```bash
dotnet user-secrets set "Llm:Providers:azure:ApiKey" "..." --project backend/src/Prophet.Api
dotnet user-secrets set "Llm:Providers:openai:ApiKey" "..." --project backend/src/Prophet.Api
dotnet user-secrets set "Llm:Providers:anthropic:ApiKey" "..." --project backend/src/Prophet.Api
```

Frontend environment: copy `frontend/.env.example` to `frontend/.env.local` and set `NEXT_PUBLIC_PROPHET_API_URL`.

## Architecture

### Backend: Hexagonal (Ports & Adapters)

```
Prophet.Api              → ASP.NET Core host, 7 controllers, middleware
Prophet.Application      → Use cases, agents, pipeline orchestration
Prophet.Domain           → Entities (PipelineProject, ArtifactVersion, PipelineArtifact), zero dependencies
Prophet.CrossCutting     → Result pattern, FluentValidation integration, middleware
Prophet.Adapters.LLM     → Multi-provider routing (Azure OpenAI, OpenAI, Anthropic)
Prophet.Adapters.InMemory → Concurrent dictionaries for all runtime state
```

Dependency direction: `Api → Application → Domain`. Adapters implement interfaces defined in `Application`.

### Agent Pipeline (10 steps, resumable & retryable)

Each step writes a `ProphetPipelineArtifact` with JSON/HTML content to the in-memory store. Steps can be individually retried or rewound without re-running prior steps.

| Step | Agent | Output | LLM Category |
|------|-------|--------|--------------|
| 1 | file-agent | `chunks.json` | — |
| 2 | insight-agent | `insights.json` | `reasoning` |
| 3 | market-agent | `market-analysis.json` | `research` |
| 4 | model-agent | `domain-model.json` | `structured` |
| 5 | architecture-agent | `architecture.json` | `reasoning` |
| 6 | diagram-agent | Mermaid JSON | `structured` |
| 7 | poc-web-agent | `index.html` | `structured` |
| 8 | poc-mobile-agent | `mobile-poc.html` | `structured` |
| 9 | doc-agent | `documentation.md` | `reasoning` |
| 10 | packaging-agent | `packaging-manifest.json` | — |

### Multi-Model Routing

Three independent LLM categories configured in `appsettings.json`:
- `reasoning` → most capable model (e.g., o3)
- `research` → broad knowledge (e.g., gpt-4.1)
- `structured` → fast JSON output (e.g., gpt-4.1-mini)

Implemented in `Prophet.Adapters.LLM/CategoryRoutingLlmAdapter.cs`.

### Versioning & Branching

`refine` creates a new `ProphetArtifactVersion` branching from any prior pipeline step, copying parent outputs and re-running forward. This enables non-destructive iteration.

### Frontend Structure

```
app/[locale]/(prophet)/prophet/  → Project list, create, edit/run pages
lib/api/prophet/                 → HTTP client for all API calls
lib/prophet/                     → Pipeline helpers, auto-continue logic, status mapping
components/                      → UI components (forms, tables, pipeline viewer)
messages/                        → i18n strings (pt, en, es)
```

The pipeline view auto-polls and auto-continues steps based on execution status from `lib/prophet/` helpers.

### Key Files

- `backend/src/Prophet.Api/appsettings.json` — CORS, rate limit, LLM routing config
- `backend/src/Prophet.Application/AgentPipeline/` — All 10 agents + executor + context
- `backend/src/Prophet.Application/Interfaces/` — Port contracts (stores, LLM adapter, storage)
- `backend/src/Prophet.Adapters.InMemory/` — All in-memory store implementations
- `frontend/lib/api/prophet/` — API client calls
- `frontend/lib/prophet/` — Pipeline UI logic

## Testing Notes

- Backend tests use xUnit + Moq; E2E tests use the full in-memory stack (no mocks for stores)
- Frontend tests use Vitest + jsdom with MSW for API mocking (`vitest.config.ts`)
- Test project: `Prophet.Tests` (101 unit), `Prophet.Tests.E2E` (16 E2E), frontend (99 tests)
