CONTEXT:

You are working on FitTech, a gym management platform built with .NET 9 Minimal APIs 
and Vertical Slice Architecture. Before writing any code, do the following:

MANDATORY FIRST STEPS:
1. Read the existing FitTech.MembershipService project structure, Program.cs, 
   and one existing feature slice (Features/Members/CreateMember/) to understand 
   the exact setup, conventions, and patterns already in use.
2. Read the installed packages from FitTech.MembershipService.csproj to know 
   which packages are available.
3. Apply the same conventions, patterns, and package usage to everything you generate.
4. Use your C# skills to write idiomatic, modern .NET 9 C#.

---

PART 1 — FitTech.PaymentService (NEW SERVICE)

Create this service from scratch following the exact same structure and conventions 
as FitTech.MembershipService.

FOLDER STRUCTURE:
FitTech.PaymentService/
├── Domain/
│   ├── Entities/
│   │   └── Payment.cs
│   ├── Enums/
│   │   ├── PaymentStatus.cs
│   │   ├── PaymentMethod.cs
│   │   └── PaymentType.cs
│   └── Errors/
│       └── PaymentErrors.cs
├── Features/
│   └── Payments/
│       └── CreatePayment/
│           ├── CreatePaymentCommand.cs
│           ├── CreatePaymentHandler.cs
│           ├── CreatePaymentRequest.cs
│           ├── CreatePaymentResponse.cs
│           ├── CreatePaymentValidator.cs
│           └── CreatePaymentEndpoint.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── PaymentDbContext.cs
│   │   ├── Configurations/
│   │   │   └── PaymentConfiguration.cs
│   │   └── Migrations/
│   └── Messaging/
│       └── Events/
│           └── PaymentConfirmedEvent.cs
├── Common/
│   └── Extensions/
└── Program.cs

---

ENUMS:

PaymentStatus.cs
- Pending
- Paid
- Refunded
- Cancelled

PaymentMethod.cs
- Cash
- CreditCard

PaymentType.cs
- Subscription
- ECommerce
- Session

---

ENTITY: Payment.cs

Properties:
- Id (Guid, PK)
- UserId (Guid, not null) — logical reference to Identity Service, no DB FK constraint
- Amount (decimal, not null)
- Currency (string, not null, default: "DZD")
- Status (PaymentStatus, not null)
- PaymentMethod (PaymentMethod, not null)
- PaymentType (PaymentType, not null)
- ReferenceId (Guid, not null) — Id of the entity being paid for (SubscriptionId, OrderId, etc.)
- Notes (string?, nullable)
- CreatedAt (DateTime, UTC, not null)

Rules:
- Private setters on all properties
- Static Create() factory method
- No data annotations
- Namespace: FitTech.PaymentService.Domain.Entities

---

EF CONFIGURATION: PaymentConfiguration.cs

- Table name: "Payments"
- Id: PK
- UserId: required, no FK constraint at DB level
- Amount: required, precision(18,2)
- Currency: required, max length 10
- Status: required, stored as string
- PaymentMethod: required, stored as string
- PaymentType: required, stored as string
- ReferenceId: required
- Notes: max length 500, nullable
- CreatedAt: required

---

PaymentDbContext.cs
- DbSet<Payment> Payments
- ApplyConfigurationsFromAssembly

---

FEATURE SLICE: CreatePayment

This endpoint is internal — called only by other services (Membership, ECommerce, etc.) 
using a Client Credentials token. It is not exposed to end users.

CreatePaymentRequest.cs — record with:
- UserId (Guid)
- Amount (decimal)
- PaymentMethod (PaymentMethod enum)
- PaymentType (PaymentType enum)
- ReferenceId (Guid)
- Notes (string?)

CreatePaymentResponse.cs — record with:
- PaymentId (Guid)

CreatePaymentValidator.cs — rules:
- UserId: not empty
- Amount: greater than 0
- PaymentMethod: valid enum value
- PaymentType: valid enum value
- ReferenceId: not empty

CreatePaymentCommand.cs — record wrapping CreatePaymentRequest.
Return type: ErrorOr<CreatePaymentResponse>

CreatePaymentHandler.cs:
STEP 1 — Create Payment entity using Payment.Create() with:
- UserId, Amount, PaymentMethod, PaymentType, ReferenceId, Notes from request
- Currency = "DZD"
- Status = PaymentStatus.Paid (cash payments are confirmed at point of creation)
- CreatedAt = DateTime.UtcNow

STEP 2 — Add to DbContext, call SaveChangesAsync.

STEP 3 — Publish PaymentConfirmedEvent via Wolverine IMessageBus.

STEP 4 — Return ErrorOr<CreatePaymentResponse> with new PaymentId.

CreatePaymentEndpoint.cs — Carter module:
- Route: POST /api/payments
- Authorization policy: "InternalServiceOnly" 
  (only accepts Client Credentials tokens issued to internal services)
- On success: Results.Created with location /api/payments/{PaymentId} and 
  CreatePaymentResponse body
- On error: map ErrorOr errors to appropriate HTTP results

---

PaymentConfirmedEvent.cs:
A record with:
- PaymentId (Guid)
- UserId (Guid)
- Amount (decimal)
- Currency (string)
- PaymentMethod (PaymentMethod)
- PaymentType (PaymentType)
- ReferenceId (Guid)
- CreatedAt (DateTime)

---

Program.cs:
Follow the exact same setup as FitTech.MembershipService Program.cs including:
- Serilog
- EF Core + Npgsql (connection string key: "Postgres")
- Wolverine with RabbitMQ (exchange: "fittech.events")
- Carter
- FluentValidation assembly scan
- JWT Bearer authentication (Authority from config, Audience: "payment-service")
- Authorization policy "InternalServiceOnly": requires authenticated user + 
  claim "client_id" with any value (any valid service token is accepted)
- OpenAPI + Scalar

---

PART 2 — FitTech.MembershipService (ADDITIONS ONLY)

Do NOT modify any existing file except the ones explicitly listed below.

---

ADDITION 1: Subscription entity — add two properties:
- PaymentId (Guid?, nullable)
- PaymentStatus (enum: Pending, Paid, Refunded, Cancelled)

Add PaymentStatus enum under Domain/Enums/PaymentStatus.cs if it does not 
already exist there. Do not duplicate it if it is already present.

Update the existing EF configuration for Subscription to map these two new columns:
- PaymentId: nullable
- PaymentStatus: required, stored as string, default value: "Pending"

Create a new EF migration for these changes.

---

ADDITION 2: Refit client for Payment Service

Create Infrastructure/Http/IPaymentServiceClient.cs:

public interface IPaymentServiceClient
{
    [Post("/api/payments")]
    Task<ApiResponse<CreatePaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request);
}

Where CreatePaymentRequest and CreatePaymentResponse mirror the records defined 
in Part 1. Define them as DTOs local to this file — do not reference the 
PaymentService project directly.

Register in Program.cs (add these lines, do not remove anything existing):
- AddRefitClient<IPaymentServiceClient>() with base address from 
  config key "Services:PaymentService"
- Add ClientCredentialsHandler to the Refit client (already built and registered)

---

ADDITION 3: CreateSubscription feature slice

Path: Features/Subscriptions/CreateSubscription/

CreateSubscriptionRequest.cs — record with:
- MemberId (Guid)
- PlanId (Guid)
- PaymentMethod (PaymentMethod enum — Cash or CreditCard)
- Notes (string?, nullable)

CreateSubscriptionResponse.cs — record with:
- SubscriptionId (Guid)
- PaymentStatus (PaymentStatus)

CreateSubscriptionValidator.cs — rules:
- MemberId: not empty, must exist in Members table and Status = Active
- PlanId: not empty, must exist in SubscriptionPlans table and IsActive = true
- PaymentMethod: valid enum value

CreateSubscriptionCommand.cs — record wrapping CreateSubscriptionRequest.
Return type: ErrorOr<CreateSubscriptionResponse>

CreateSubscriptionHandler.cs:
STEP 1 — Validate MemberId exists and Status = Active. 
If not: Error.NotFound("Member.NotFound", "Member does not exist or is inactive.")

STEP 2 — Validate PlanId exists and IsActive = true.
If not: Error.NotFound("SubscriptionPlan.NotFound", "Plan does not exist or is inactive.")

STEP 3 — Check no active subscription already exists for this member.
If exists: Error.Conflict("Subscription.AlreadyActive", "Member already has an active subscription.")

STEP 4 — Calculate EndOnUTC:
- If DurationUnit = Months: StartOnUTC.AddMonths(DurationValue)
- If DurationUnit = Days: StartOnUTC.AddDays(DurationValue)
- If both null (per-session plan): EndOnUTC = null
- StartOnUTC = DateTime.UtcNow

STEP 5 — Create Subscription entity with:
- MemberId, PlanId from request
- StartOnUTC, EndOnUTC from step 4
- RemainingSessions = plan.SessionCount (nullable)
- Status = SubscriptionStatus.Active
- AutoRenew = false
- PaymentId = null
- PaymentStatus = PaymentStatus.Pending

STEP 6 — Add to DbContext, call SaveChangesAsync.

STEP 7 — Return ErrorOr<CreateSubscriptionResponse> with SubscriptionId and 
PaymentStatus = Pending.

CreateSubscriptionEndpoint.cs — Carter module, added to MembersModule or a new 
SubscriptionsModule:
- Route: POST /api/subscriptions
- Authorization policy: "AdminOnly"
- On success: Results.Created with location /api/subscriptions/{SubscriptionId}
- On error: map ErrorOr errors to appropriate HTTP results

---

ADDITION 4: ConfirmCashPayment feature slice

Path: Features/Subscriptions/ConfirmCashPayment/

ConfirmCashPaymentRequest.cs — record with:
- SubscriptionId (Guid)
- AmountReceived (decimal)
- PaymentMethod (PaymentMethod enum)
- Notes (string?, nullable)

ConfirmCashPaymentResponse.cs — record with:
- SubscriptionId (Guid)
- PaymentId (Guid)
- PaymentStatus (PaymentStatus)

ConfirmCashPaymentValidator.cs — rules:
- SubscriptionId: not empty
- AmountReceived: greater than 0
- PaymentMethod: valid enum value

ConfirmCashPaymentCommand.cs — record wrapping ConfirmCashPaymentRequest.
Return type: ErrorOr<ConfirmCashPaymentResponse>

ConfirmCashPaymentHandler.cs:
Constructor-inject MembershipDbContext and IPaymentServiceClient.

STEP 1 — Load Subscription by SubscriptionId including related Member.
If not found: Error.NotFound("Subscription.NotFound", "Subscription does not exist.")

STEP 2 — Verify PaymentStatus = Pending.
If not: Error.Conflict("Subscription.AlreadyPaid", "This subscription has already been paid.")

STEP 3 — Load SubscriptionPlan to get plan.Price for amount verification.
If AmountReceived does not equal plan.Price: 
Error.Validation("Payment.AmountMismatch", "Amount received does not match the subscription plan price.")

STEP 4 — Call IPaymentServiceClient.CreatePaymentAsync with:
- UserId = member.UserId
- Amount = AmountReceived
- PaymentMethod from request
- PaymentType = PaymentType.Subscription
- ReferenceId = SubscriptionId
- Notes from request
If call fails: Error.Failure("Payment.Failed", "Failed to register payment with Payment Service.")

STEP 5 — Update Subscription:
- PaymentId = returned PaymentId from Payment Service
- PaymentStatus = PaymentStatus.Paid

STEP 6 — Call SaveChangesAsync.

STEP 7 — Return ErrorOr<ConfirmCashPaymentResponse> with SubscriptionId, 
PaymentId, PaymentStatus = Paid.

ConfirmCashPaymentEndpoint.cs — Carter module added to SubscriptionsModule:
- Route: POST /api/subscriptions/confirm-payment
- Authorization policy: "AdminOnly"
- On success: Results.Ok with ConfirmCashPaymentResponse body
- On error: map ErrorOr errors to appropriate HTTP results

---

FINAL RULES:
- Do not modify any file in MembershipService except those explicitly listed above.
- Do not create any shared/common project between services — each service is fully independent.
- All namespaces must match folder structure precisely.
- No XML doc comments.
- No inline comments unless a step is non-obvious.
- Do not reference FitTech.PaymentService project from FitTech.MembershipService — 
  communication is HTTP only via Refit.
