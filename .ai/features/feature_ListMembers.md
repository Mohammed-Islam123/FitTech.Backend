# Feature: List Members

## Overall Plan
Implement a paginated GET endpoint for administrators and coaches to search and list gym members. This will include basic filtering (e.g., by name or status).

## Feature Roadmap (Ordered)
- [x] **Data Model Design:** Define `ListMembersRequest` (pagination/filters) and `ListMembersResponse` (paginated result).
- [x] **Request Definition:** Create `ListMembersQuery` as a Wolverine record.
- [x] **Implementation of Logic (Handler):**
    - [x] Inject `MembershipDbContext`.
    - [x] Use `AsNoTracking()` for performance.
    - [x] Implement keyword search (FirstName/LastName) and status filtering.
    - [x] Implement pagination (`Skip`/`Take`).
    - [x] Project directly to `MemberSummaryDto` to avoid over-fetching.
- [x] **API Endpoint (Carter Module):**
    - [x] Register `GET /api/members`.
    - [x] Require `Admin` or `Coach` roles using the `UserAccessor`.
    - [x] Add XML documentation and OpenAPI examples with transformers.
- [x] **Validation:** Verify pagination and filtering work as expected.

## Completion Criteria
- [x] Functional `GET /api/members` endpoint with pagination.
- [x] Search functionality by name.
- [x] Status filtering.
- [x] Role-based access control enforced.
- [x] Comprehensive OpenAPI documentation.
