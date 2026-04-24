# Feature: Update Member

## Overall Plan
Implement a comprehensive update feature for gym members. This feature supports two main flows:
1. **Self-Update (`/api/members/my-profile`):** Members can update their own personal details, profile picture, date of birth, health profile, and change their password.
2. **Administrative Update (`/api/members/{id}`):** Admins and Coaches can update any member's personal details, status, and health profile.

The implementation will use a single Wolverine handler to manage logic, performing role-based checks to ensure security and proper data access.

## Feature Roadmap (Ordered)
- [x] **Identity Service Enhancement:**
    - [x] Update `UserController.UpdateProfile` to support `[FromForm]` and handle file uploads for `ProfilePicture`.
    - [x] Ensure existing `ChangePassword` endpoint is accessible for internal service calls if needed, or use the existing one.
- [x] **Infrastructure Update (Membership Service):**
    - [x] Update `IIdentityServiceClient` to include `UpdateProfileAsync` (with multipart support) and `ChangePasswordAsync`.
- [x] **Data Model Design:**
    - [x] Define `UpdateMemberRequest` as a multipart form request containing all updatable fields.
    - [x] Define `UpdateMemberCommand`.
- [x] **Implementation of Logic (Handler):**
    - [x] Use `UserAccessor` to get current User ID and Roles.
    - [x] Retrieve Member from database, including `HealthProfile`.
    - [x] **Authorization Check:**
        - [x] If updating via `my-profile`, ensure the Member belongs to the current User.
        - [x] If updating via `{id}`, ensure user has `Admin` or `Coach` role.
    - [x] **Identity Sync:**
        - [x] Call Identity Service to update User profile data and files.
        - [x] If `OldPassword` and `NewPassword` are provided, call Identity Service to change password (only for self-update).
    - [x] **Membership Update:**
        - [x] Update `FirstName`, `LastName`, `Status` (Admin only), `DateOfBirth`, and `Gender`.
        - [x] Update or Create `MemberHealthProfile` (Objectives, MedicalRestrictions).
    - [x] Save changes to `MembershipDbContext`.
- [x] **API Endpoints (Carter Module):**
    - [x] Register `PUT /api/members/my-profile`.
    - [x] Register `PUT /api/members/{id}`.
    - [x] Add XML documentation and OpenAPI examples with transformers.
- [x] **Validation:**
    - [x] Create `UpdateMemberValidator` to enforce business rules (e.g., password length, valid date of birth).

## Completion Criteria
- [ ] Functional self-update endpoint.
- [ ] Functional administrative update endpoint.
- [ ] Successful profile picture and medical certificate replacement.
- [ ] Successful password change (with old password verification).
- [ ] Role-based access control strictly enforced.
- [ ] Proper error handling for `NotFound`, `Unauthorized`, and `Validation` errors.
- [ ] Comprehensive OpenAPI documentation.
