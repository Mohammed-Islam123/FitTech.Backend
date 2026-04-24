## Project Overview

**FitTech** – a connected gym management system.  
Composed of:

- Mobile app for members
- Web interface for coaches and administrators
- Hardware infrastructure: NFC readers, connected machines, sensors

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
- **Online shop**: Supplements, clothing integrated into the app. (not a full ecommerce flow, just product listing and purchase linking to external site)

## Known Constraints & Technical Considerations

- **NFC card lifecycle**: Activation/deactivation must be real‑time and reliable.
- **Access control**: Entry/exit logging requires low‑latency checks.
- **Connected machines**: Integration with varied hardware, data ingestion and association to members.
- **Booking rules**: Strict 2h cancellation window, waitlist automation, penalty system.
- **Health data privacy**: Coaches see only necessary information; GDPR-like handling implied.
- **Multi‑platform clients**: Mobile (members) and web (coaches/admins) share same backend.
- **Reporting**: Financial and usage statistics must be generated efficiently.
- **Notifications**: Timely delivery (2h reminders, waitlist updates) is critical.
