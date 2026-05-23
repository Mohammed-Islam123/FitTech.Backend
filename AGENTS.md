you MUST read and internalize the project-wide rules in: @.ai\MASTER_INSTRUCTIONS.md


# AGENTS.md — FitTech.Backend

## Quick start

```powershell
# Run entire system via Aspire (Docker required)
dotnet run --project FitTech.AppHost/FitTech.AppHost/FitTech.AppHost.csproj

# Build everything
dotnet build FitTech.slnx
```

## Architecture

Microservices (.NET 10 + C# 14) orchestrated by **Aspire** (AppHost.cs), gateway via **YARP** (`gateway.yaml`).

| Service | Style | Messaging | DB in AppHost |
|---------|-------|-----------|---------------|
| Identity | Controllers (Clean Architecture) | MassTransit + RabbitMQ | identity-db |
| Membership | Carter + Wolverine | Wolverine + RabbitMQ | membershipDb |
| Payment | Carter + Wolverine | Wolverine + RabbitMQ | **NOT wired** |
| Notification | Worker + MassTransit | MassTransit + RabbitMQ | none |
| Gateway | YARP reverse proxy | none | none |

**Cross-service communication:** Refit HTTP clients (sync), Wolverine/MassTransit events via RabbitMQ (async).  
**Shared events/contracts** live in `Shared/Events/`.

## Route mapping (gateway.yaml)

| Path | Target |
|------|--------|
| `/api/User/**` | Identity |
| `/api/members/**` | Membership |
| `/api/plans/**` | Membership |
| `/docs/identity/**` | Identity OpenAPI |
| `/docs/membership/**` | Membership OpenAPI |

## Conventions

- **Result pattern:** All handlers return `ErrorOr<T>`; endpoints use `MapErrorsToResult()` extension.
- **Wolverine mediation:** Handlers invoked via `IMessageBus.InvokeAsync()` (Carter modules call Wolverine).
- **FluentValidation** as Wolverine middleware (`ValidationBehavior`).
- **API docs:** `.WithDescription()`, `.WithTags()`, custom `AddOpenApiOperationTransformer()` using `JsonObject`.
- **XML docs:** Only for service methods & business logic — not endpoints. Use `<description>` only.
- **Nullable:** `is null` / `is not null` (never `== null`).
- **Cross-service events** must be in `Shared/Events/`.

## Known gaps

- **No tests exist** in the repository.
- **No CI/CD** — `.github/workflows/` is empty.
- **Payment project** on disk but not in `FitTech.slnx`, not in AppHost.cs, no database container, no gateway routes.
- **No .editorconfig** despite being referenced in `.github/csharp.instructions.md`.

## EF Core migrations

```powershell
dotnet ef migrations add <Name> --project Services/Membership/Membership.csproj
```

Each service auto-migrates on startup in Development (`context.Database.MigrateAsync()`).

## Seed users (Identity)

- admin/Admin@12345, member/Member@12345, coach/Coach@12345

## Existing instruction files

| File | Purpose |
|------|---------|
| `.ai/MASTER_INSTRUCTIONS.md` | Agent workflow, planning, session handover protocol |
| `.ai/architecture.md` | Tech stack & architecture summary |
| `.ai/project_brief.md` | Full domain context |
| `.ai/progress.md` | Current implementation state |
| `.ai/features/*.md` | Individual feature specs |
| `.github/csharp.instructions.md` | C# coding guidelines |


# Registered Agents

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, invoke the `skill` tool with `skill: "graphify"` before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

## Agent Categories

This repository contains 38 specialized agents and 282 total skills (36 shared + 246 agent-local) registered and available for use in Copilot CLI.

Use `/agent` command in Copilot CLI to browse and select agents, or reference them directly by ID.

### Operations
- `infrastructure-manager` - Cloud infrastructure lifecycle: DNS, Cloudflare, server provisioning, origin security, email, CI/CD, edge Workers, Zero Trust

### Development
- `agent-builder` - Agent and skill creation, scaffolding, validation, and registration
- `ai-engineer` - Full-spectrum AI/ML engineering: ML, DL, NLP, CV, data science, data engineering, plus LLM workflows (prompts, RAG, fine-tuning, evaluation, safety)
- `skill-builder` - Skill creation, optimization, validation, and migration
- `backend-developer` - Backend/API development
- `frontend-developer` - Frontend/UI development
- `engineer` - General engineering, architecture, and Cloudflare infrastructure management
- `qa-tester` - Quality assurance and testing
- `web-scraper` - Scrapes, extracts, and structures data from websites, search engines, maps, and social media. Its skills are available as shared skill references across development, cybersecurity, marketing, and research agents.
- `n8n-specialist` - n8n workflow automation: MCP-first SDK integration, design patterns, error handling, API integration, webhook endpoints, credential security

### Design & Multimedia
- `graphical-designer` - Graphic design and visual creation
- `ui-ux-designer` - User interface and experience design
- `motion-designer` - Motion graphics and animation
- `video-editor` - Video editing and production

### Content & Marketing
- `copywriter` - Writing and content creation
- `social-media-manager` - Social media operations
- `media-buyer` - Media purchasing and advertising
- `seo-specialist` - Search engine optimization

### Business Operations
- `orchestrator` - Multi-agent task orchestration: tmux-based interactive dispatch, preflight workflow, cross-agent comparison, session monitoring, retrospective evaluation
- `second-brain-manager` - Obsidian knowledge base orchestration: scans vaults, discovers projects, spins up free agents for parallel content processing, summarizes sessions, indexes notes, maintains cross-note connections
- `project-manager` - Project management and orchestration
- `community-manager` - Community engagement
- `operator-manual-builder` - Documentation and processes
- `public-footprint-researcher` - Public information research

### Intelligence & Analysis
- `competitor-intelligence-lead` - Competitive analysis
- `social-dataset-auditor` - Data auditing and analysis

### Cybersecurity
- `cybersec-engineer` - Security audit & review (secrets, access, exposure)
- `pentester` - Penetration testing & offensive security
- `soc-analyst` - SOC operations & incident detection
- `threat-intel-analyst` - Threat intelligence & OSINT
- `osint-ctf-specialist` - CTF OSINT challenge solving, reconnaissance & tool orchestration
- `cloud-sec-engineer` - Cloud, container, K8s security, and Cloudflare management
- `security-architect` - Security architecture & compliance

### Specialized Domains
- `backend-developer` - Backend development
- `finance` - Financial operations
- `hr` - Human resources
- `doctor` - Medical/health domain
- `dentist` - Dental domain

## Using Agents

### In Copilot CLI
```
/agent                    # Browse all agents
/agent backend-developer  # Select specific agent
```

### Agent Features
Each agent includes:
- **Capabilities** - Specific skills and procedures
- **Tool Preferences** - Recommended tools and integrations
- **Knowledge Focus** - Domain expertise areas
- **Playbooks** - Multi-step workflows
- **Model Preferences** - Optimal model tiers

## Full Registry
See `REGISTRY.json` for complete machine-readable catalog with paths and manifests.
