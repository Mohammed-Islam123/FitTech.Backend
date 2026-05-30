# Graph Report - Services\Notification  (2026-05-29)

## Corpus Check
- 11 files · ~1,429 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 53 nodes · 44 edges · 10 communities (7 shown, 3 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `0bf45bde`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]

## God Nodes (most connected - your core abstractions)
1. `Email` - 8 edges
2. `Notification.Api` - 4 edges
3. `EmailTemplateService` - 4 edges
4. `LogLevel` - 3 edges
5. `LogLevel` - 3 edges
6. `Worker` - 3 edges
7. `EmailConfirmationRequestedConsumer` - 3 edges
8. `EmailService` - 3 edges
9. `IEmailService` - 3 edges
10. `IEmailTemplateService` - 3 edges

## Surprising Connections (you probably didn't know these)
- `EmailService` --inherits--> `IEmailService`  [EXTRACTED]
  Notification.Api/Services/EmailService.cs → Notification.Api/Services/Interfaces/IEmailService.cs
- `EmailTemplateService` --inherits--> `IEmailTemplateService`  [EXTRACTED]
  Notification.Api/Services/EmailTemplateService.cs → Notification.Api/Services/Interfaces/IEmailTemplateService.cs

## Communities (10 total, 3 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.22
Nodes (8): Frontend, BaseUrl, Identity, BaseUrl, Logging, LogLevel, Default, Microsoft.Hosting.Lifetime

### Community 1 - "Community 1"
Cohesion: 0.25
Nodes (8): Email, Host, Password, Port, SenderEmail, SenderName, UserName, UseTLS

### Community 2 - "Community 2"
Cohesion: 0.25
Nodes (7): DOTNET_ENVIRONMENT, commandName, dotnetRunMessages, environmentVariables, profiles, Notification.Api, $schema

### Community 3 - "Community 3"
Cohesion: 0.29
Nodes (3): Dictionary, IEmailTemplateService, EmailTemplateService

### Community 5 - "Community 5"
Cohesion: 0.40
Nodes (4): Logging, LogLevel, Default, Microsoft.Hosting.Lifetime

## Knowledge Gaps
- **18 isolated node(s):** `Default`, `Microsoft.Hosting.Lifetime`, `Default`, `Microsoft.Hosting.Lifetime`, `Host` (+13 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Email` connect `Community 1` to `Community 0`?**
  _High betweenness centrality (0.063) - this node is a cross-community bridge._
- **What connects `Default`, `Microsoft.Hosting.Lifetime`, `Default` to the rest of the system?**
  _18 weakly-connected nodes found - possible documentation gaps or missing edges._