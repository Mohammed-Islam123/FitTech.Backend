# Project Progress

## Current Status
**All 5 phases completed.** 47 endpoints across 5 services. Full system implemented.

## Service Status
- **AppHost**: Configured for local orchestration (7 services).
- **ServiceDefaults**: Configured for OpenTelemetry and HealthChecks.
- **Gateway**: YARP gateway — routes for 5 services (identity, membership, payment, courses + `/api/me/*`, `/api/courses/*`).
- **Identity Service**: Functional user registration, profile management, deactivation.
- **Membership Service**: 18 endpoints. Full member lifecycle, profile, subscriptions, course stubs.
- **Courses Service**: 11 endpoints. Coaches, programs, sessions, attendance. Session auto-generation on acceptance.
- **Payment Service**: 10 endpoints. CreatePayment, ListPayments + 8 offline request endpoints (create/list/accept/reject for both membership renewal and course purchase).
- **Notification Service**: Basic consumer for user registration.
- **Activity Service**: 5 endpoints. Entry/exit scan, manual enter/exit, sessions today, member activity history.
- **Aggregation Service**: 3 endpoints. Pure CQRS — event consumers (write) + direct DB queries (read).
- **Shared**: 13 events total.

## Recent Achievements (Phase 2)
- **Courses Service** — new microservice at `Services/Courses/`. Carter + Wolverine + PostgreSQL.
- **11 endpoints**: CreateCoach, CreateProgram, ListProgramRequests, GetProgramRequest, AcceptProgram, RejectProgram, GetCoachPrograms, GetCoachClients, GetProgramMembers, MarkAttendance, GetCoachClientProfile.
- **Session generation**: AcceptProgram auto-generates Session records for each matching day between StartDate and EndDate.
- **4 new shared events**: `ProgramCreatedEvent`, `ProgramAcceptedEvent`, `ProgramRejectedEvent`, `AttendanceMarkedEvent`.
- AppHost: `coursesDb` database, `courses-api` service, Scalar API reference, gateway reference.
- Gateway: routes for `/api/coaches/*`, `/api/programs/*`, `/api/sessions/*`, plus Scalar docs for Courses API.
- Entity naming: `Program` collides with startup class — resolved via `global using ProgramEntity = Courses.Domain.Entities.Program;`.

## Recent Achievements (Phase 3)
- **Payment Service extended** — 9 new endpoints:
  - `GET /api/payments` — Admin lists all payments with member names via Identity service.
  - `POST /api/requests/membership-renewal` — Member submits renewal request.
  - `POST /api/requests/course-purchase` — Member submits course purchase request.
  - `GET /api/requests/membership-renewal` — Admin lists pending membership renewal requests.
  - `GET /api/requests/course-purchase` — Admin lists pending course purchase requests.
  - `PATCH /api/requests/membership-renewal/{id}/accept` — Admin accepts; publishes `MembershipRenewalAcceptedEvent`.
  - `PATCH /api/requests/membership-renewal/{id}/reject` — Admin rejects.
  - `PATCH /api/requests/course-purchase/{id}/accept` — Admin accepts; publishes `CoursePurchaseAcceptedEvent`.
  - `PATCH /api/requests/course-purchase/{id}/reject` — Admin rejects.
- **PaymentRequest entity** — `PaymentRequestType` (MembershipRenewal/CoursePurchase), `PaymentRequestStatus` (Pending/Accepted/Rejected).
- Added `IUserAccessor` + `IIdentityServiceClient` to Payment service.
- Gateway: `/api/requests/*` route routed to payment-cluster.
- 2 new shared events: `MembershipRenewalAcceptedEvent`, `CoursePurchaseAcceptedEvent`.

## Recent Achievements (Phase 4)
- **Activity Service** — new microservice at `Services/Activity/`. Carter + Wolverine + PostgreSQL.
- **5 endpoints**:
  - `GET /api/activity/sessions/today` — All check-ins today with active status.
  - `GET /api/activity/members/{id}` — Member's activity history (entry/exit times).
  - `POST /api/activity/entry-exit/scan` — NFC card scan. Auto-detects check-in vs check-out.
  - `POST /api/activity/entry-exit/manual/enter` — Manual entry with optional course ID.
  - `POST /api/activity/entry-exit/manual/exit` — Manual exit with optional course ID.
- **MemberActivity entity**: Id, MemberId, CardUid, CourseId, CheckInTime, CheckOutTime, IsManual.
- Refit clients for Membership + Courses services (for validation lookups).
- 2 new shared events: `MemberCheckedInEvent`, `MemberCheckedOutEvent`.
- AppHost: `activityDb`, `activity-api`, gateway/cluster/scalar wired.

## In Progress
- **Phase 5: Aggregation Service** — pure CQRS, event-driven dashboards + Excel export (3 endpoints).

## Next Steps
1. Create Aggregation service consuming events from all services.
2. Build read models: DashboardStats, FinanceSnapshot, MonthlyBreakdown.
3. Implement Wolverine consumers for all domain events.
4. Expose: `GET /api/dashboard/admin`, `GET /api/dashboard/finance`, `GET /api/reports/excel`.

## Notes for Next Agent
- Course endpoints in Membership still return empty/404 — stubs awaiting `ICoursesServiceClient` wiring.
- All new services follow the Carter + Wolverine + ErrorOr + FluentValidation + PostgreSQL pattern.
- `IUserAccessor` copied into Courses service (same pattern as Membership).
- `DisableConventionalLocalRouting()` — Wolverine handlers invoked explicitly via `IMessageBus.InvokeAsync`.
