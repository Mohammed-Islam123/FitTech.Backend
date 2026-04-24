# Feature: Get Member Detail

## Overall Plan
Implement a GET endpoint to retrieve comprehensive member information, including their personal data, active subscription status, health profile, and current NFC card details.

## Feature Roadmap (Ordered)
- [x] **Data Model Design:** Define the `GetMemberResponse` DTO structure reflecting member, subscription, health profile, and NFC card.
- [x] **Request Definition:** Create `GetMemberQuery` as a Wolverine record for retrieving member details.
- [x] **Implementation of Logic (Handler):**
    - [x] Query `MembershipDbContext` using `AsNoTracking()` for performance.
    - [x] Apply direct projection via `.Select()` for better performance and reduced overhead.
    - [x] Handle the `NotFound` scenario using `ErrorOr`.
    - [x] Map retrieved entities to `GetMemberResponse`.
- [x] **API Endpoint (Carter Module):**
    - [x] Register `GET /api/members/{id}`.
    - [x] Implement XML documentation with `<description>` and `<example>`.
    - [x] Use `IMessageBus.InvokeAsync` to call the handler.
    - [x] Add OpenAPI tags and examples using transformers as per `MASTER_INSTRUCTIONS.md`.
- [x] **Validation:** Verify the endpoint retrieves all expected details correctly.

## Completion Criteria
- [x] Functional `GET /api/members/{id}` endpoint.
- [x] XML Documentation with descriptions and examples.
- [x] OpenAPI documentation with tags and request/response examples.
- [x] Correct retrieval of active subscription and health profile.
- [x] Standardized error response for non-existent IDs.
