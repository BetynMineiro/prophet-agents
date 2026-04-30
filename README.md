# Prophet

**Prophet** is an open academic project that turns requirement documents, ideas, or feature descriptions into a structured set of technical artifacts — domain models, architecture proposals, Mermaid diagrams, interactive HTML PoCs, mobile wireframes, and technical documentation — using a multi-agent LLM pipeline.

The project has no authentication, no login, no database, no external storage. It is designed to run entirely on a local machine — all state lives in memory — with any compatible LLM provider (Azure OpenAI, OpenAI, Anthropic, or others).

---

## What it does

You create a **project**, upload one or more **input documents** (requirement specs, product briefs, ideas in plain text, PDFs, etc.), and trigger the pipeline. The pipeline runs a sequence of specialized agents, each producing a structured artifact that feeds the next agent:

```
Input documents
       │
       ▼
 [1] file-agent          → chunks.json          (splits and normalises source text)
       │
       ▼
 [2] insight-agent       → insights.json        (entities, use cases, business rules, domain summary)
       │
       ▼
 [3] market-agent        → market-analysis.json (competitors, patterns, risks)
       │
       ▼
 [4] model-agent         → domain-model.json    (aggregates, value objects, relationships, ubiquitous language)
       │
       ▼
 [5] architecture-agent  → architecture.json    (style, layers, components, integration points)
       │
       ▼
 [6] diagram-agent       → class-diagram.json   (Mermaid class diagram)
                        → flow-diagram.json    (Mermaid flow diagram)
       │
       ▼
 [7] poc-web-agent       → index.html           (navigable single-file web PoC, stored on disk)
       │
       ▼
 [8] poc-mobile-agent    → mobile-poc.html      (mobile wireframe / PoC, stored on disk)
       │
       ▼
 [9] doc-agent           → documentation.md     (full technical documentation, stored on disk)
       │
       ▼
[10] packaging-agent     → packaging-manifest.json (index of all generated artifacts and files)
```

Each step is independently retryable, rewindable, and pausable. You can refine a project (change request → new version branching from any prior step) and compare versions side by side.

---

## Multi-model routing

Each agent uses one of three **LLM categories**, each of which can be routed to a different model or provider:

| Category | Used by | Recommended model type |
|---|---|---|
| `reasoning` | insight-agent, architecture-agent | Most capable reasoning model |
| `structured` | model-agent, diagram-agent, poc-web-agent, poc-mobile-agent | Fast structured-output model |
| `research` | market-research-agent | Model with broad knowledge |

This means you can, for example, use `o3` for reasoning, `gpt-4.1-mini` for structured output, and `gpt-4.1` for research — all in the same pipeline run. Routing is configured entirely in `appsettings.json` or User Secrets; no code changes required.

Supported drivers:

| Driver | `Driver` value | Notes |
|---|---|---|
| Azure OpenAI | `AzureOpenAI` | Requires deployment name as model |
| OpenAI | `OpenAiV1` | Uses `api.openai.com` |
| Anthropic | `AnthropicMessages` | Uses `api.anthropic.com` |

---

## Project structure

```
Prophet/
├── README.md
├── .gitignore
├── backend/                    .NET 10 solution
│   ├── backend.slnx
│   ├── Directory.Build.props
│   ├── Directory.Packages.props
│   ├── global.json
│   └── src/
│       ├── Prophet.Api/                 ASP.NET Core host (port 7017)
│       ├── Prophet.Application/         Use cases, agents, interfaces
│       ├── Prophet.Domain/              Entities, value objects, pipeline step IDs
│       ├── Prophet.CrossCutting/        Result pattern, validation, pagination
│       ├── Prophet.Adapters.LLM/        LLM adapter (Azure OpenAI, OpenAI, Anthropic)
│       ├── Prophet.Adapters.InMemory/   In-memory stores (no DB, no filesystem)
│       ├── Prophet.Tests/               Unit tests
│       └── Prophet.Tests.E2E/           E2E tests (in-process API)
└── frontend/                   Next.js 16 app (port 4000)
    ├── app/
    ├── components/
    ├── lib/
    ├── messages/               i18n (pt, en, es)
    └── .env.example
```

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- An LLM API key (Azure OpenAI, OpenAI, or Anthropic)

---

## Running locally

### 1. Backend secrets

Prophet uses [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for local credentials. None of these values are ever committed.

```bash
cd Prophet/backend

# LLM — see "Configuring models" section below for all options
dotnet user-secrets set "Llm:Providers:azure:ApiKey" "<your-key>" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:azure:BaseUrl" "https://YOUR-RESOURCE.openai.azure.com" --project src/Prophet.Api
```

> Without a key the API still starts, but running the pipeline returns an error. All other endpoints work normally.

### 2. Run the backend

```bash
cd Prophet/backend
dotnet run --project src/Prophet.Api
# → https://localhost:7017
```

All state is kept in memory — no database or filesystem setup needed. State is lost on restart (by design).

### 3. Run the frontend

```bash
cd Prophet/frontend
cp .env.example .env.local     # set NEXT_PUBLIC_PROPHET_API_URL if backend is not on 7017
npm install
npm run dev
# → http://localhost:4000
```

---

## Configuring models

All LLM configuration lives in `appsettings.json` (defaults) and is overridden via User Secrets or environment variables locally. No restart required after changing secrets — restart the API to pick up new values.

### Azure OpenAI

```bash
dotnet user-secrets set "Llm:Providers:azure:Driver" "AzureOpenAI" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:azure:BaseUrl" "https://YOUR-RESOURCE.openai.azure.com" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:azure:ApiKey" "<key>" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:azure:AzureApiVersion" "2024-10-21" --project src/Prophet.Api

# Route all categories to different deployments
dotnet user-secrets set "Llm:Routing:reasoning:Provider" "azure" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:reasoning:Model" "my-o3-deployment" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Provider" "azure" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Model" "my-gpt4mini-deployment" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Provider" "azure" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Model" "my-gpt4-deployment" --project src/Prophet.Api
```

> **`AzureOmitTemperature`**: set to `true` for deployments that do not accept a `temperature` parameter (e.g. `o3`, `gpt-5-nano`). Default is `false`.

### OpenAI (api.openai.com)

```bash
dotnet user-secrets set "Llm:Providers:openai:Driver" "OpenAiV1" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:openai:BaseUrl" "https://api.openai.com" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:openai:ApiKey" "sk-..." --project src/Prophet.Api

dotnet user-secrets set "Llm:Routing:reasoning:Provider" "openai" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:reasoning:Model" "o3" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Provider" "openai" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Model" "gpt-4.1-mini" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Provider" "openai" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Model" "gpt-4.1" --project src/Prophet.Api
```

### Anthropic

```bash
dotnet user-secrets set "Llm:Providers:anthropic:Driver" "AnthropicMessages" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:anthropic:BaseUrl" "https://api.anthropic.com" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:anthropic:ApiKey" "sk-ant-..." --project src/Prophet.Api

dotnet user-secrets set "Llm:Routing:reasoning:Provider" "anthropic" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:reasoning:Model" "claude-opus-4-6" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Provider" "anthropic" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Model" "claude-sonnet-4-6" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Provider" "anthropic" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Model" "claude-sonnet-4-6" --project src/Prophet.Api
```

### Mixed multi-model example (recommended)

Route each category to the best model for that task across different providers:

```bash
# Reasoning → o3 on Azure (most capable)
dotnet user-secrets set "Llm:Providers:azure:Driver" "AzureOpenAI" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:azure:BaseUrl" "https://RESOURCE.openai.azure.com" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:azure:ApiKey" "<azure-key>" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:reasoning:Provider" "azure" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:reasoning:Model" "o3-deployment" --project src/Prophet.Api

# Structured → claude-sonnet-4-6 on Anthropic (fast, precise JSON)
dotnet user-secrets set "Llm:Providers:anthropic:Driver" "AnthropicMessages" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:anthropic:BaseUrl" "https://api.anthropic.com" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:anthropic:ApiKey" "sk-ant-..." --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Provider" "anthropic" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:structured:Model" "claude-sonnet-4-6" --project src/Prophet.Api

# Research → gpt-4.1 on OpenAI (broad knowledge)
dotnet user-secrets set "Llm:Providers:openai:Driver" "OpenAiV1" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:openai:BaseUrl" "https://api.openai.com" --project src/Prophet.Api
dotnet user-secrets set "Llm:Providers:openai:ApiKey" "sk-..." --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Provider" "openai" --project src/Prophet.Api
dotnet user-secrets set "Llm:Routing:research:Model" "gpt-4.1" --project src/Prophet.Api
```

### Development pin (single model for all categories)

Useful when you only have one deployment available or want to reduce costs while testing:

```bash
dotnet user-secrets set "Llm:Development:PinAllToProvider" "azure" --project src/Prophet.Api
dotnet user-secrets set "Llm:Development:Model" "gpt-4o-mini-deployment" --project src/Prophet.Api
```

This overrides all routing and sends every agent to the same model. Only active when `ASPNETCORE_ENVIRONMENT=Development` (default for `dotnet run`).

### Testing the LLM route

A dev-only endpoint is available to test the LLM config without running the full pipeline:

```bash
# POST /v1/prophet/dev/llm/complete (only available in Development, returns 404 in Production)
curl -X POST https://localhost:7017/v1/prophet/dev/llm/complete \
  -H "Content-Type: application/json" \
  -d '{"message": "Say hello", "category": "reasoning"}'
```

---

## Testing

```bash
# Backend unit tests (101 tests)
cd Prophet/backend
dotnet test src/Prophet.Tests/

# Backend E2E tests — in-memory DB, no external services needed (16 tests)
dotnet test src/Prophet.Tests.E2E/

# Frontend tests (99 tests)
cd Prophet/frontend
npm run test:run

# Type check + lint + format (frontend)
npm run check
```

## API exploration (Bruno)

A Bruno collection is available at `Prophet/bruno-requests/`. Open the folder in [Bruno](https://www.usebruno.com/) and select the `local` environment (`https://localhost:7017`). All requests are open — no auth headers needed.

---

## API overview

All endpoints are open — no `Authorization` header required.

| Method | Path | Description |
|---|---|---|
| `GET/POST` | `/v1/prophet/projects` | List / create projects |
| `GET/PUT/DELETE` | `/v1/prophet/projects/{id}` | Get / update / soft-delete a project |
| `PATCH` | `/v1/prophet/projects/{id}/restore` | Restore a soft-deleted project |
| `POST` | `/v1/prophet/projects/{id}/refine` | Submit a change request (creates new version) |
| `GET/POST` | `/v1/prophet/projects/{id}/inputs` | List / upload input documents |
| `DELETE` | `/v1/prophet/projects/{id}/inputs/{inputId}` | Delete an input document |
| `GET` | `/v1/prophet/projects/{id}/versions` | List artifact versions |
| `GET` | `/v1/prophet/projects/{id}/versions/{versionId}` | Get version details + pipeline status |
| `POST` | `/v1/prophet/projects/{id}/versions/{versionId}/pipeline/run` | Start / resume pipeline |
| `POST` | `/v1/prophet/projects/{id}/versions/{versionId}/pipeline/continue/{step}` | Continue from a paused step |
| `POST` | `/v1/prophet/projects/{id}/versions/{versionId}/pipeline/retry/{step}` | Retry a failed step |
| `POST` | `/v1/prophet/projects/{id}/versions/{versionId}/pipeline/rewind/{step}` | Rewind to a prior step |
| `GET` | `/v1/prophet/projects/{id}/versions/{versionId}/pipeline/artifacts` | List all pipeline artifacts |
| `GET` | `/v1/prophet/projects/{id}/versions/{versionId}/pipeline/artifacts/{type}` | Get artifact by type |
| `GET/POST/DELETE` | `/v1/prophet/projects/{id}/final-artifacts` | Manage final output files |
| `GET/POST/DELETE` | `/v1/prophet/projects/{id}/html-pocs` | Manage HTML PoC files |
| `GET` | `/v1/prophet/files/{*path}` | Serve a stored file by path |
| `POST` | `/v1/prophet/dev/llm/complete` | Test LLM call (Development only) |

---

## Configuration reference

| Key | Default | Description |
|---|---|---|
| `Storage:Root` | `prophet` | Root prefix for stored paths |
| `Storage:ApiBaseUrl` | `https://localhost:7017` | Base URL used to build file-serve URLs |
| `Cors:LocalhostPortFrom` | `3000` | First localhost port allowed (inclusive) |
| `Cors:LocalhostPortTo` | `5000` | Last localhost port allowed (inclusive) |
| `RateLimit:Api:PermitLimit` | `100` | Requests per window |
| `RateLimit:Api:WindowSeconds` | `60` | Rate limit window in seconds |
| `Llm:Providers:{name}:Driver` | — | `AzureOpenAI`, `OpenAiV1`, or `AnthropicMessages` |
| `Llm:Providers:{name}:BaseUrl` | — | Provider base URL (no trailing slash) |
| `Llm:Providers:{name}:ApiKey` | — | API key (set via User Secrets) |
| `Llm:Providers:azure:AzureApiVersion` | `2024-10-21` | Azure OpenAI API version |
| `Llm:Providers:azure:AzureOmitTemperature` | `false` | Set `true` for deployments that reject `temperature` |
| `Llm:Routing:{category}:Provider` | — | Which provider to use for `reasoning`, `structured`, `research` |
| `Llm:Routing:{category}:Model` | — | Model name or deployment name for that category |
| `Llm:Development:PinAllToProvider` | — | Dev only: override all routing to this provider |
| `Llm:Development:Model` | — | Dev only: override all routing to this model |

---

## Architecture notes
- **No authentication** — all endpoints are open. This is by design for a local academic tool.
- **Hexagonal architecture** — `Domain` has zero dependencies. `Application` depends only on `Domain`. Adapters (`LLM`, `InMemory`) implement Application interfaces. `Prophet.Api` composes everything via DI.
- **In-memory storage** — all state (projects, files, pipeline artifacts) lives in `ConcurrentDictionary` singletons. State is lost on restart — by design for a local tool. Files are served via `GET /v1/prophet/files/{*path}`.
- **Pipeline is resumable** — each step persists its output as a `ProphetPipelineArtifact`. Rewind, retry, and continue operations allow iterating on specific steps without rerunning the whole pipeline.
- **Versioning with branching** — a "refine" creates a new `ProphetArtifactVersion` that copies parent outputs up to the re-entry step, then re-runs from that point forward with the change request in context.
- **Multi-model** — the LLM adapter resolves `reasoning / structured / research` to independent provider + model combinations at runtime. Changing a model requires only a config update, not a code change.
