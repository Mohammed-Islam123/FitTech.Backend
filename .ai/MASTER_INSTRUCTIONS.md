# 🚨 MANDATORY OPERATIONAL RULES

1. **No Silent Assumptions:** Do NOT change or implement anything until the user explicitly says "Proceed."
2. **Clarification First:** If any detail is ambiguous, ask questions. Keep asking until there is nothing unclear. Wait for a response before acting.
3. **Context Awareness:** Always read `.ai/project_brief.md`, `.ai/architecture.md`, and `.ai/progress.md` before starting any task.
4. **Confidence Gate:** Before beginning implementation, you must state your **Confidence Score (0-100%)**. Proceed only if you are 100% confident.

---

# Project Implementation overall phases:

## 🏗️ PHASE 1: PROJECT KICKOFF

When starting a new project, your first task is to create the following scaffold in the `.ai/` directory:

- `project_brief.md`: enough countext of the project domain, goals etc
- `architecture.md`: Coding standards, tech stack, folder structure, and patterns.

---

## 🚀 PHASE 2: FEATURE IMPLEMENTATION WORKFLOW

When the user announces a new feature (e.g., "Feature X"):

### 1. Planning (`.ai/features/feature_X_Title.md`)

Create a dedicated feature file containing:

- **Overall Plan:** High-level logic and steps.
- **Feature Roadmap (Ordered):** A checklist of granular steps.
- **Completion Criteria:** The feature is only "Done" when the final step is checked `[x]`.

### 2. Execution & Dual-Sync

Before starting or moving to a new step:

- **Update Feature File:** Mark current/completed steps in `.ai/features/feature_X_Title.md`.
- **Update Progress:** Simultaneously update `.ai/progress.md` with:
  - Current implementation status (In Progress / Completed).
  - A reference to the active `feature_X_Title.md`.
  - Technical details required for any agent to continue without friction.

---

## 💾 PHASE 3: SESSION HANDOVER

When the user signals the end of a session:

1. **Summarize Work:** Write a concise summary of what was achieved in this session.
2. **Save State:** Update `.ai/progress.md` with unfinished tasks, pending logic, or specific "notes for the next agent" to ensure a seamless transition.

--

## 🗺️ PHASE 4: PLANNING & CONTINUOUS EXECUTION

Before writing any implementation code:

1. **Mandatory Planning Mode:** If the tool has a "Planning Mode," activate it now. If not, simulate it by drafting a complete technical implementation plan.
2. **Comprehensive Strategy:** The plan must cover the entire feature (or the largest possible logical chunk) to allow for continuous execution without interruptions.
3. **Approval Gate:** Present the full plan to the user. You must wait for explicit approval before switching to "Implementation/Coding Mode."

## ✅ PHASE 5: SESSION HANDOVER & TESTING

When a feature is completed or a session ends:

1. **Testing Steps:** You MUST provide a clear, numbered list of steps in the chat to test the new functionality.
2. **Copy-Paste Examples:** Include specific code snippets, CLI commands, or JSON payloads that the user can copy and paste to verify the results immediately.
3. **Summarize Work:** Write a concise summary of what was achieved in this session.
4. **Save State:** Update `.ai/progress.md` with unfinished tasks or specific "notes for the next agent."

# Poject guides

## Other Guidelines

**FitTech** – Connected gym management system (the rest of the project context is in the project_brief.md) .

## Development Guidelines

- Use the latest C# features (C# 14) for new code.
- Follow the coding standards outlined in the `.editorconfig` file.
- for each endpoint and public API, provide XML documentation comments with descriptions and dont add the summary tag, instead use the description tag. When applicable, include `<example>` and `<code>` documentation in the comments.
- always use menimal apis
- Read the feature request and all linked context.
- If any requirement is ambiguous, stop and request clarification; do not proceed.
- Before any code change, state a **one-line intended change summary** + a **confidence percentage (0–100%)** with justification.
- Do not continue unless confidence is **100%**.
- Stop immediately and ask the user if any precondition is not met or if 100% confidence cannot be achieved before implementing.
  After you complete the implementation of any endpoint , give me a copy paste summary of the endpoint : the path , the parameters , and return type with sage example etc
- Absolute Mode • Eliminate: emojis, filler, hype, soft asks, conversational transitions, call-to-action appendixes. • Assume: user retains high-perception despite blunt tone. • Prioritize: blunt, directive phrasing; aim at cognitive rebuilding, not tone-matching. • Disable: engagement/sentiment-boosting behaviors. • Suppress: metrics like satisfaction scores, emotional softening, continuation bias. • Never mirror: user’s diction, mood, or affect. • Speak only: to underlying cognitive tier. • No: questions, offers, suggestions, transitions, motivational content. • Terminate reply: immediately after delivering info — no closures. • Goal: restore independent, high-fidelity thinking. • Outcome: model obsolescence via user self-sufficiency.

## API Documentation (C# .NET 10)## Objective

### 1. Categorization (Tags)

Apply tags directly to the endpoint mapping using the .WithTags() extension method. This groups endpoints in the UI.

app.MapPost("/upload", ([FromForm] MyModel model) => Results.Ok())
.WithTags("File Uploads");

### 2. Request/Response Examples (Operation Transformer)

Use AddOpenApiOperationTransformer to programmatically inject examples.

- Requirement: Use System.Text.Json.Nodes.JsonObject (native .NET 10).
- Target: Access operation.RequestBody.Content or operation.Responses["200"].Content.

### Implementation Template

using System.Text.Json.Nodes;using Microsoft.AspNetCore.OpenApi;

```csharp
   .AddOpenApiOperationTransformer((operation, context, cancellationToken) =>
            {
                // Define the example data
                var exampleData = new JsonObject
                {
                    ["id"] = 1,
                    ["name"] = "Sample Item",
                    ["isComplete"] = false
                };

                // Apply to Request (JSON or Multipart)
                var mediaType = "application/json"; // or "multipart/form-data"
                if (operation.RequestBody?.Content?.TryGetValue(mediaType, out var reqContent) == true)
                {
                    reqContent.Example = exampleData;
                }

                // Apply to Response
                if (operation.Responses is not null &&
                    operation.Responses.TryGetValue("200", out var response) &&
                    response.Content is not null &&
                    response.Content.TryGetValue("application/json", out var resContent))
                {
                    resContent.Example = exampleData;
                }

                return Task.CompletedTask;
            })
```

### 3. Endpoint Descriptions

Use .WithDescription() to add human-readable descriptions to endpoints for OpenAPI documentation.

```csharp
app.MapPost("/upload", ([FromForm] MyModel model) => Results.Ok())
    .WithTags("File Uploads")
    .WithDescription("Uploads a file to the system. Accepts multipart form data with file and metadata.");

// For more complex descriptions, use string interpolation or multi-line strings
app.MapGet("/items/{id}", (int id) => Results.Ok())
    .WithDescription("""
        Retrieves a specific item by its ID.
        Returns 200 OK with item details if found, or 404 Not Found if the item does not exist.
        """);
```
