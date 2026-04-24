# Feature: Manage Subscription Plans

## Overall Plan
Implement a complete CRUD (Create, Read, Update, Delete) management system for gym subscription plans. This feature is strictly restricted to Administrators. Deletion will be handled as a soft-delete to preserve data integrity for existing subscriptions.

## Feature Roadmap (Ordered)
- [x] **Data Model Audit:**
    - [x] Ensure `SubscriptionPlan` entity has an `IsActive` property for soft deletion.
- [x] **Create Plan:**
    - [x] Define `CreatePlanRequest`, `Command`, `Handler`, and `Validator`.
    - [x] Endpoint: `POST /api/plans`.
- [x] **Get Plan(s):**
    - [x] Define `ListPlansQuery` and `GetPlanQuery`.
    - [x] Endpoint: `GET /api/plans` (List all active).
- [x] **Update Plan:**
    - [x] Define `UpdatePlanRequest`, `Command`, and `Handler`.
    - [x] Endpoint: `PUT /api/plans/{id}`.
- [x] **Delete Plan (Soft Delete):**
    - [x] Define `DeletePlanCommand` and `Handler`.
    - [x] Endpoint: `DELETE /api/plans/{id}` (Set `IsActive = false`).
- [x] **Authorization:**
    - [x] Restrict all endpoints to the `Admin` role.

## Completion Criteria
- [x] Admins can create new plans with name, price, and duration.
- [x] Admins can list and view plan details.
- [x] Admins can update plan details (affects new subscriptions only).
- [x] Admins can soft-delete plans.
- [x] Non-admin users are strictly forbidden from these endpoints.
- [x] Full OpenAPI documentation with examples.
