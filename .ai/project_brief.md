**Confidence score: 98/100.**

---

Here is the updated document. Confirm and I will proceed to generate the file.

---

## Project Overview
**FitTech** – a connected gym management system.
Composed of:
- Mobile app for members
- Web interface for coaches and administrators
- Hardware infrastructure: NFC readers, connected machines, sensors

---

## Core Business Logic & User Flows

### 1. Member Management & Subscriptions
- **Registration**: Personal details, subscription choice, online/on‑site payment.
- **Subscription types**: Monthly (auto‑renewal), Annual (12‑month commitment), Per‑session packs, Free trial (the admin can add more types).
- **Health profile** (optional): Goals, medical restrictions – visible to coaches.
- **NFC card**: Unique card activated with subscription, deactivated on expiration/cancellation.
- **Member space**: View personal info, session history, invoices, pause subscription (max 2 months/year).

### 2. Course Booking
- **Coaches** create classes (spinning, crossfit, etc.) with capacity, schedule, duration, level.
- **Members** book via mobile app up to 7 days in advance.
- **Waitlist**: Auto‑enrol when full; first in line auto‑booked when a spot frees (notification sent).
- **Cancellation**: Up to 2h before class; after that, session deducted from subscription/package.
- **Attendance**: Coach validates via tablet; no‑show → warning. After 3 warnings, account suspended for 1 week.

### 3. Access & Activity Tracking
- **Entry/Exit**: Member taps NFC card; system checks subscription validity, logs timestamps.
- **Connected machines**: Capture performance (distance, calories, heart rate) – associated to member via app or machine badge.
- **Performance dashboard**: Statistics (weekly sessions, calories, progress) and personalised recommendations.

### 4. Staff Management
- **Coaches**: Profile (name, photo, specialities, bio); manage own schedule; view health profiles of class participants.
- **Administrators**: Manage subscriptions, members (activate/deactivate, payment history), coaches; generate financial reports (revenue, attendance).

### 5. Notifications & Communication
- **Automatic notifications**: Class reminders (2h before), goal alerts, personalised offers, coach‑cancelled classes.
- **Internal messaging**: Members can message coaches for advice/questions.

### 6. Other Features
- **Equipment maintenance**: Members/coaches report issues → alerts for maintenance.
- **Online shop**: Product listing and on-site purchasing (admin-processed); price management and stock tracking. Future extension: member purchases via mobile app.

---

## Known Constraints & Technical Considerations
- **NFC card lifecycle**: Activation/deactivation must be real‑time and reliable.
- **Access control**: Entry/exit logging requires low‑latency checks.
- **Connected machines**: Integration with varied hardware, data ingestion and association to members.
- **Booking rules**: Strict 2h cancellation window, waitlist automation, penalty system.
- **Health data privacy**: Coaches see only necessary information; GDPR-like handling implied.
- **Multi‑platform clients**: Mobile (members) and web (coaches/admins) share same backend.
- **Reporting**: Financial and usage statistics must be generated efficiently.
- **Notifications**: Timely delivery (2h reminders, waitlist updates) is critical.

---

## Services

### 1. Payment Service — .NET 10
Handles all payment transactions across the platform regardless of the originating service.
- Accept and record cash payments confirmed by an admin.
- Accept and record online credit card payments (future — mobile only).
- Publish a `PaymentConfirmed` event to the message bus on successful payment creation.
- Expose an internal endpoint callable only by other services using a service token (`type: service` claim).

---

### 2. Membership & Subscription Service — .NET 10
Central service for member lifecycle and subscription management.
- Register new members: create user account in Identity Service via HTTP, create member record, assign subscription plan, optionally assign NFC card — all in one atomic admin-initiated operation.
- Manage member status: active, suspended, paused, cancelled.
- Manage subscription lifecycle: create, cancel, pause (max 2 months/year), renew.
- Track payment status per subscription (`Pending`, `Paid`); call Payment Service to confirm cash payments.
- Assign and deactivate NFC cards.
- Store and expose member health profiles (visible to coaches only).
- Expose a member checkout endpoint: validate NFC card and check subscription validity.
- Enforce no-show penalty: increment warning count; auto-suspend after 3 warnings for 1 week.
- Promote a coach to member via explicit admin action.
- Publish events: `MemberCreated`, `MemberSuspended`, `SubscriptionExpired`.

---

### 3. Booking & Scheduling Service — .NET 10
Manages all class-related operations.
- Store and manage coach profiles (name, photo, specialities, bio, schedule).
- Allow coaches to create and manage classes (type, capacity, schedule, duration, level).
- Allow members to book classes up to 7 days in advance.
- Manage waitlists: auto-enrol when class is full; auto-book first in line when a spot frees.
- Enforce cancellation rules: free cancellation up to 2h before class; late cancellation deducts a session.
- Record attendance: coach marks members as present or absent via tablet.
- Publish events: `BookingConfirmed`, `BookingCancelled`, `ClassCancelled`, `WaitlistPromoted`.

---

### 4. Activity Service — .NET 10
Handles physical access control and machine session tracking.
- Log member entry and exit timestamps when NFC card is tapped.
- Validate subscription status in real time via Membership Service before granting access.
- Receive and store machine performance data (distance, calories, heart rate) per session.
- Associate machine sessions to members via mobile app or machine badge scan.
- Publish events: `MemberCheckedIn`, `MemberCheckedOut`, `MachineSessionRecorded`.

---

### 5. Telemetry Service — .NET 10
Tracks member performance and generates personalised insights.
- Aggregate performance data per member per session (received from Activity Service events).
- Compute statistics: weekly session count, total calories, progress over time.
- Generate personalised recommendations based on goals defined in health profile.
- Expose performance dashboard data to the mobile app.

---

### 6. Identity Service — .NET 10
Manages user accounts and issues authentication tokens.
- Store user accounts using ASP.NET Core Identity (`AspNetUsers`).
- Issue short-lived JWT access tokens (15 min) for authenticated users carrying `sub`, `email`, `role`, and `type: user` claims.
- Issue short-lived service tokens (5 min) for internal service-to-service calls carrying `type: service` claim.
- Expose a JWKS endpoint so downstream services can validate token signatures without calling Identity Service on every request.
- Expose a service token endpoint (`POST /auth/service-token`) authenticated by `ClientId` + `ClientSecret`.

---

### 7. Communication Service — .NET 10
Handles real-time and persistent messaging between members and coaches.
- Manage chat sessions between a member and a coach.
- Deliver messages in real time using SignalR.
- Persist chat history in MongoDB.
- Expose chat history retrieval endpoints.

---

### 8. Notification Service — .NET 10
Consumes events from the message bus and delivers notifications to users.
- Listen for events: `MemberCreated`, `PaymentConfirmed`, `BookingConfirmed`, `BookingCancelled`, `WaitlistPromoted`, `ClassCancelled`, `MemberSuspended`, `SubscriptionExpired`.
- Deliver notifications via push notification, email, and SMS depending on event type and user preferences.
- Send class reminders 2 hours before a booked class.
- Send welcome email with generated password on `MemberCreated`.

---

### 9. E-Commerce Service — Spring Boot
Manages the gym's on-site product shop.
- Maintain product catalogue: name, description, price, category, images.
- Track stock levels and update inventory on purchase.
- Allow admin to process on-site purchases; call Payment Service to register the transaction.
- Future extension: allow members to browse and purchase via mobile app.

---

### 10. Equipment & Maintenance Service — Spring Boot
Manages gym equipment inventory and issue reporting.
- Maintain an inventory of machines: name, type, location, optional QR code.
- Allow members and coaches to scan a QR code or select a machine to report a malfunction or issue.
- Track issue status: open, in progress, resolved.
- Publish alerts to maintenance staff when a new issue is reported.

---

### 11. Reporting Service — .NET 10
Provides aggregated financial and operational analytics using CQRS.
- Listen to events from all services and maintain a local read-optimised database (write side via event consumption).
- Expose query endpoints for: revenue reports, subscription statistics, attendance rates, no-show rates, class popularity (read side).
- Never calls other services directly — data is built exclusively from consumed events.
- Serve report data to the admin dashboard.

---

Confirm this is correct and I will generate the updated PDF file.
