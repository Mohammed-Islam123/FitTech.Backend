---
apply_to: "Services/Membership"
---

# Membership Service Guidelines

## Architecture: Vertical Slice + Carter Modules

All features in the Membership service follow **vertical slice architecture** with **Carter modules** for endpoint registration.

### Folder Structure

```
Services/Membership/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── MembershipDbContext.cs
├── Infrastructure/
│   ├── Configurations/
│   └── Repositories/
├── Features/
│   ├── Members/
│   │   ├── CreateMember/
│   │   │   ├── CreateMemberEndpoint.cs
│   │   │   ├── CreateMemberHandler.cs
│   │   │   ├── CreateMemberRequest.cs
│   │   │   ├── CreateMemberResponse.cs
│   │   │   └── CreateMemberValidator.cs
│   │   ├── GetMember/
│   │   ├── UpdateMember/
│   │   └── DeleteMember/
│   ├── Subscriptions/
│   ├── SubscriptionPlans/
│   └── NfcCards/
├── Common/
│   ├── Extensions/
│   └── Utilities/
└── Program.cs
```

## File Patterns

### Endpoint (CartModule)

**File:** `Features/{Feature}/{Operation}/{Operation}Endpoint.cs`

Inherits `CarterModule`. Registers single operation with handler injection.

```csharp
using Carter;
using Membership.Features.{Feature}.{Operation};
using MediatR;

namespace Membership.Features.{Feature}.{Operation};

public class {Operation}Endpoint : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/members", Handle)
            .WithName("{Operation}")
            .WithOpenApi()
            .Produces<{Operation}Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    /// <description>
    /// {Feature description and details}
    /// </description>
    /// <example>
    /// POST /api/members
    /// Body: { "firstName": "John", "lastName": "Doe" }
    /// Response: { "id": "guid", ... }
    /// </example>
    private async Task<IResult> Handle(
        {Operation}Request request,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new {Operation}Command(request), ct);
        return result.IsSuccess
            ? Results.Created($"/api/members/{result.Value.Id}", result.Value)
            : Results.BadRequest(result.Error);
    }
}
```

### Handler (Wolverine)

**File:** `Features/{Feature}/{Operation}/{Operation}Handler.cs`

Implements Wolverine handler processing `ErrorOr`.

```csharp
using ErrorOr;
using Membership.Domain;

namespace Membership.Features.{Feature}.{Operation};

public class {Operation}Handler
{
    private readonly MembershipDbContext _context;

    public {Operation}Handler(MembershipDbContext context) => _context = context;

    public async Task<ErrorOr<{Operation}Response>> Handle(
        {Operation}Command command,
        CancellationToken ct)
    {
        // Implementation
        return new {Operation}Response { /* ... */ };
    }
}
```

### Request DTO

**File:** `Features/{Feature}/{Operation}/{Operation}Request.cs`

Plain class, no logic.

```csharp
namespace Membership.Features.{Feature}.{Operation};

public class {Operation}Request
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
```

### Response DTO

**File:** `Features/{Feature}/{Operation}/{Operation}Response.cs`

Plain class, mirrors entity shape or subset.

```csharp
namespace Membership.Features.{Feature}.{Operation};

public class {Operation}Response
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
```

### Validator

**File:** `Features/{Feature}/{Operation}/{Operation}Validator.cs`

Uses FluentValidation.

```csharp
using FluentValidation;

namespace Membership.Features.{Feature}.{Operation};

public class {Operation}Validator : AbstractValidator<{Operation}Request>
{
    public {Operation}Validator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required");
    }
}
```

## Wolverine Commands/Queries

**File:** `Features/{Feature}/{Operation}/{Operation}Command.cs`

```csharp
using ErrorOr;

namespace Membership.Features.{Feature}.{Operation};

public record {Operation}Command(
    {Operation}Request Request
);
```

## Conventions

- **Namespace:** `Membership.Features.{FeatureName}.{OperationName}`
- **Class names:** `{OperationName}Endpoint`, `{OperationName}Handler`, `{OperationName}Request`, `{OperationName}Response`, `{Operation}Command`/`{Operation}Query`
- **Operation naming:** PascalCase verb+noun (`CreateMember`, `GetMember`, `UpdateMember`, `DeleteMember`)
- **Routes:** Kebab-case `/api/members`, `/api/members/{id}`
- **HTTP verbs:** POST (create), GET (read), PUT/PATCH (update), DELETE (delete)

## Documentation

Every endpoint must include XML documentation with `<description>` tag (no `<summary>`). Include `<example>` with HTTP verb, path, sample request body, sample response.

## Error Handling

Use ErrorOr pattern for command/query handlers:

```csharp
return value;
// or
return Error.NotFound(code, description);
```

Return appropriate HTTP status codes from endpoints based on handler result.

## Inheritance from Root Guidelines

All items from root `.github/copilot-instructions.md` apply:

- C# 14 features for new code
- Follow `.editorconfig`
- Minimal APIs only
- Absolute Mode tone
