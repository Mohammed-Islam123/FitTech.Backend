# Project Progress

## Current Status
Implementation of the **Membership Service** is ongoing. The core `CreateMember` feature is functional and integrated with the Identity Service and asynchronous messaging via Wolverine.

## Service Status
- **AppHost**: Configured for local orchestration.
- **ServiceDefaults**: Configured for OpenTelemetry and HealthChecks.
- **Gateway**: YARP gateway configured.
- **Identity Service**: Functional user registration, profile management, and deactivation.
- **Membership Service**: `CreateMember`, `GetMemberDetail`, `GetSubscriptionHistory`, `ListMembers`, `UpdateMember`, `DeleteMember` completed.
- **Notification Service**: Basic consumer for user registration.
- **Shared**: Common events (e.g., `MemberCreatedEvent`, `UserRegisteredEvent`) and wrappers.

## Recent Achievements
- Completed `DeleteMember` (Soft Delete) vertical slice.
- Restricted `DeleteMember` and administrative `UpdateMember` to the **Admin** role only.
- Implemented role-based privacy for **Health Profiles**: 
    - Only Members can create/update their own health data.
    - Coaches can view full health profiles (Objectives + Restrictions).
    - Admins can only view Medical Restrictions (Objectives are private).
- Enhanced **Identity Service** with `DeactivateUser` functionality (Sync via Refit).
- Implemented `UserAccessor` in `Common/Security` with role-based helper properties.
- Refined `IIdentityServiceClient` to use local DTOs/Requests, removing unnecessary cross-service project dependencies.

## In Progress
- Membership Service: Core lifecycle features.
- Active Feature: [Delete Member](features/feature_DeleteMember.md)

## Next Steps
1. Implement **Get Active Subscription** for members.
2. Begin **Health Profiles** management (Get/Update details).
3. Develop **Subscription Plan** CRUD management.
4. Implement Subscription assignment and pause logic.
5. Develop NFC Card hardware UID assignment.

## Notes for Next Agent
- Ensure all new features follow the Vertical Slice Architecture with Carter and Wolverine as documented in `.ai/features/membershipService.instructions.md`.
- Use `ErrorOr` for consistent functional error handling.
- Maintain XML documentation with `<description>` and `<example>` tags for all endpoints.
- Apply EF Core best practices (AsNoTracking, selective Includes).
