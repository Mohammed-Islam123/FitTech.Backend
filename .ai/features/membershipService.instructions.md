---
apply_to: "Services/Membership"
---

# Membership Service Guidelines

> [!IMPORTANT]
> **Maintenance Rule:** Update this section whenever a feature is started, completed, or in progress.
> 
> **Current Status (Last Updated: April 18, 2026):**
> - **Member Core:** `CreateMember` (Completed), `GetMember` (Completed), `GetSubscriptionHistory` (Completed), `ListMembers` (Completed), `UpdateMember` (Completed), `DeleteMember` (Completed).
> - **Health Profiles:** Partial (Integrated in Member updates).
> - **Subscription Plans:** CRUD Management (Completed).
> - **Subscriptions:** Not started.
> - **NFC Cards:** Basic creation in `CreateMember` (Done), Lifecycle management (Not started).

## Feature Roadmap (Ordered)

1.  **Member Management**
    - [x] **Create Member:** Integration with Identity Service (User creation, file uploads), Wolverine publishing `MemberCreatedEvent`.
    - [x] **Get Member Detail:** Fetch member info with active subscription and health profile summary.
    - [ ] **Get Active Subscription:** Member-specific view of their current subscription.
    - [x] **Get Subscription History:** Retrieve a list of all past and current subscriptions for a member.
    - [x] **List Members:** Paginated search for admins/coaches.
    - [x] **Update Member:** Update personal details and contact info.
    - [x] **Delete/Deactivate Member:** Soft delete (Inactive) and Identity Service deactivation.
2.  **Health Profiles**
    - [ ] **Get Health Profile:** Detailed view of goals and restrictions.
    - [ ] **Update Health Profile:** Allow members or coaches to update health data.
3.  **Subscription Plans**
    - [x] **Manage Plans:** CRUD for plans (Monthly, Annual, Packs, Trials).
    - [ ] **List Active Plans:** Public/Member view of available options.
4.  **Subscriptions**
    - [ ] **Assign Subscription:** Link a plan to a member, calculate expiration.
    - [ ] **Cancel/Pause Subscription:** Logic for the 2-month/year pause rule.
    - [ ] **Automated Expiration:** Background process to update statuses based on `EndOnUTC`.
5.  **NFC Cards**
    - [ ] **Assign/Replace Card:** Link hardware UID to member.
    - [ ] **Lifecycle Management:** Activate on subscription start, deactivate on expiration.
    - [ ] **NFC Entry/Exit Log:** Record and retrieve timestamped entry/exit events.

## Architecture: Vertical Slice + Carter Modules

### Folder Structure
```
Services/Membership/
├── Domain/ (Entities, Enums, DbContext)
├── Infrastructure/ (Messaging, Clients, Configurations)
├── Features/
│   └── {FeatureGroup}/
│       └── {Operation}/
│           ├── {Operation}Endpoint.cs
│           ├── {Operation}Handler.cs
│           ├── {Operation}Request.cs
│           ├── {Operation}Response.cs
│           ├── {Operation}Command.cs (or Query)
│           └── {Operation}Validator.cs
└── Common/ (Behaviors, Wrappers)
```

## Implementation Patterns

### Endpoint (ICarterModule)
Uses `IMessageBus` (Wolverine) for execution. Standardizes error mapping via `MapErrorsToResult`.

```csharp
public class {Operation}Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/{route}", Handle)
            .WithName("{Operation}")
            .Produces<{Operation}Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Handle(
        [FromForm/FromBody] {Operation}Request request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var result = await messageBus.InvokeAsync<ErrorOr<{Operation}Response>>(new {Operation}Command(request), ct);
        return result.Match(
            response => Results.Ok(response),
            errors => MapErrorsToResult(errors));
    }

    private static IResult MapErrorsToResult(List<Error> errors)
    {
        var firstError = errors[0];
        var statusCode = firstError.Type switch {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(statusCode: statusCode, title: firstError.Code, detail: firstError.Description);
    }
}
```

### Handler (Wolverine)
```csharp
public class {Operation}Handler(MembershipDbContext context, IMessageBus messageBus)
{
    public async Task<ErrorOr<{Operation}Response>> Handle({Operation}Command command, CancellationToken ct)
    {
        // Logic...
        // Publish shared events using: await messageBus.PublishAsync(new SomeSharedEvent(...));
        return new {Operation}Response(...);
    }
}
```

## Key Conventions
- **Events:** All cross-service events MUST reside in the `Shared` project under the `Shared.Events` namespace.
- **Validation:** Use `FluentValidation` with `ValidationBehavior` (Wolverine Middleware).
- **Communication:** Synchronous inter-service calls use `Refit` (Infrastructure). Asynchronous notifications use `Wolverine`.
