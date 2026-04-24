# Feature: Get Subscription History

## Overall Plan
Implement a GET endpoint to retrieve the full history of subscriptions for a specific member, including past, current, and future (if any) subscriptions.

## Feature Roadmap (Ordered)
- [x] **Data Model Design:** Define `SubscriptionHistoryResponse` and `SubscriptionHistoryItem` DTOs.
- [x] **Request Definition:** Create `GetSubscriptionHistoryQuery` as a Wolverine record.
- [x] **Implementation of Logic (Handler):**
    - [x] Create `UserAccessor` in `Common/Security` to retrieve `UserId` and roles.
    - [x] Query `MembershipDbContext` for all subscriptions belonging to `MemberId`.
    - [x] Use `AsNoTracking()` and project directly to `SubscriptionHistoryItem`.
    - [x] Sort by `StartOnUTC` descending.
    - [x] Implement resource-based authorization (Admin/Coach or Self).
    - [x] Handle `Member.NotFound` if the member doesn't exist.
- [x] **API Endpoint (Carter Module):**
    - [x] Register `GET /api/members/{id}/subscriptions`.
    - [x] Require authorization.
    - [x] Add XML documentation and OpenAPI examples.
- [x] **Validation:** Verify the list includes all subscriptions for a known member.

## Completion Criteria
- [x] Functional `GET /api/members/{id}/subscriptions` endpoint.
- [x] Correct sorting and projection.
- [x] Proper error handling for missing members.
- [x] Comprehensive OpenAPI documentation.
