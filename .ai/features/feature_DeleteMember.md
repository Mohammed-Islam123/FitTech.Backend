# Feature: Delete Member (Soft Delete)

## Overall Plan
Implement a DELETE endpoint to soft-delete gym members. This action is restricted to Administrators. Soft deletion involves:
1. Updating the member's status to `Inactive` in the Membership database.
2. Deactivating the corresponding user account in the Identity Service (`IsActive = false`).
3. (Future) Canceling future bookings and active subscriptions.

## Feature Roadmap (Ordered)
- [x] **Identity Service Enhancement:**
    - [x] Add `DeactivateUser(Guid userId)` method to `IUserService` and `UserService`.
    - [x] Add `PUT /api/User/{userId}/deactivate` endpoint to `UserController`.
- [x] **Infrastructure Update (Membership Service):**
    - [x] Add `DeactivateUserAsync(Guid userId)` to `IIdentityServiceClient`.
- [x] **Data Model Design:**
    - [x] Define `DeleteMemberCommand(Guid Id)`.
- [x] **Implementation of Logic (Handler):**
    - [x] Use `UserAccessor` to verify `IsAdmin` role.
    - [x] Retrieve Member from `MembershipDbContext`.
    - [x] Call Identity Service to deactivate the user.
    - [x] Update Member status to `Inactive`.
    - [x] Save changes to `MembershipDbContext`.
- [x] **API Endpoint (Carter Module):**
    - [x] Register `DELETE /api/members/{id}`.
    - [x] Add XML documentation and OpenAPI tags.
- [x] **Validation:**
    - [x] Verify that the member is marked as `Inactive` and the user account is disabled.

## Completion Criteria
- [x] Functional `DELETE /api/members/{id}` endpoint.
- [x] Restrict access to `Admin` role only.
- [x] Successful deactivation of the user in Identity Service.
- [x] Status updated to `Inactive` in Membership database.
- [x] Standardized error handling for non-existent IDs.
