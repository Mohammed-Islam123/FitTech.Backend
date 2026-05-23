# Feature: Get Active Subscription

## Overall Plan
Implement a secure and efficient endpoint to retrieve the current active subscription for a member.

## Feature Roadmap (Ordered)
- [x] **Infrastructure Setup:**
    - [x] Create `GetActiveSubscriptionQuery` record.
    - [x] Create `GetActiveSubscriptionResponse` record.
- [x] **Core Logic:**
    - [x] Implement `GetActiveSubscriptionHandler` with projections.
    - [x] Implement Admin/Member authorization logic.
    - [x] Implement date-range and status validation.
- [x] **Endpoint Registration:**
    - [x] Create `GetActiveSubscriptionEndpoint` (Carter Module).
    - [x] Add OpenAPI metadata and `TypedResults`.
- [x] **Verification:**
    - [x] Add manual test cases to `test.http`.
    - [x] Verify role-based access control.

## Completion Criteria
- [x] Endpoint `GET /api/members/active-subscription` is functional.
- [x] Returns 404 if no active subscription exists.
- [x] Returns 403 if memberId is used by non-admin.
- [x] Uses EF Core projections (no `.Include`).
- [x] Full OpenAPI documentation with examples.
