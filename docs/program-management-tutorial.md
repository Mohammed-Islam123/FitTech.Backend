# Program Management — Frontend Integration Guide

Base URL (dev, via gateway): `http://localhost:5106`

Every request must include the JWT bearer token in the `Authorization` header:

```http
Authorization: Bearer <token>
```

> Token is obtained from the Identity service at `http://localhost:5051`.
> Token format: RS256 JWT with claims `sub` (user UUID), `email`, `role` (Admin | Member | Coach).

---

## Table of Contents

1. [Auth Setup & Roles](#1-auth-setup--roles)
2. [Coach — Create a Program](#2-coach--create-a-program)
3. [Admin — View Pending Programs](#3-admin--view-pending-programs)
4. [Admin — Accept / Reject a Program](#4-admin--accept--reject-a-program)
5. [Coach — View My Programs](#5-coach--view-my-programs)
6. [Coach — View Sessions for a Program](#6-coach--view-sessions-for-a-program)
7. [Coach — Mark Attendance](#7-coach--mark-attendance)
8. [Member — View Available Programs](#8-member--view-available-programs)
9. [Member — View Program Detail](#9-member--view-program-detail)
10. [Member — Purchase Program (Online)](#10-member--purchase-program-online)
11. [Member — View Enrolled Programs & Session History](#11-member--view-enrolled-programs--session-history)
12. [Error Handling](#12-error-handling)
13. [Endpoint Reference Card](#13-endpoint-reference-card)

---

## 1. Auth Setup & Roles

```tsx
const API = "http://localhost:5106";

function authHeaders(): HeadersInit {
  const token = localStorage.getItem("jwt_token");
  return { Authorization: `Bearer ${token}` };
}
```

| Role | What they can do |
|------|-----------------|
| **Coach** | Create programs, view own programs & sessions, mark attendance |
| **Admin** | View pending programs, accept/reject, view any program's sessions |
| **Member** | View available programs, enroll (purchase), view session history |

---

## 2. Coach — Create a Program

**User action:** Coach navigates to "Create Program" → fills in program details (name, description, level, exercise type, duration, start/end dates, price, max participants, image URL) → adds time slots (day, start time, end time) → clicks "Submit".

**Endpoint:** `POST /api/programs`

**Auth:** `CoachOnly`

**Request body (JSON):**
```json
{
  "name": "HIIT Training",
  "description": "High intensity interval training for all levels.",
  "level": "Intermediate",
  "exerciseType": "Cardio",
  "durationMinutes": 60,
  "startDate": "2026-06-01",
  "endDate": "2026-08-31",
  "totalPrice": 150.00,
  "maxParticipants": 20,
  "pictureUrl": "https://example.com/hiit.jpg",
  "timeSlots": [
    {
      "day": "Monday",
      "startTime": "08:00",
      "endTime": "09:00",
      "description": "Morning session"
    },
    {
      "day": "Wednesday",
      "startTime": "08:00",
      "endTime": "09:00",
      "description": "Morning session"
    }
  ]
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `name` | string | yes | Program title |
| `description` | string | no | |
| `level` | string | no | e.g. "Beginner", "Intermediate", "Advanced" |
| `exerciseType` | string | no | e.g. "Cardio", "Strength", "HIIT" |
| `durationMinutes` | int | yes | Session length in minutes |
| `startDate` | string (date) | yes | ISO format `YYYY-MM-DD` |
| `endDate` | string (date) | yes | ISO format `YYYY-MM-DD` |
| `totalPrice` | number | yes | Price in your currency unit |
| `maxParticipants` | int | yes | Maximum number of members |
| `pictureUrl` | string | no | External image URL |
| `timeSlots` | array | yes | At least 1 time slot |
| `timeSlots[].day` | string | yes | `Sunday` \| `Monday` \| `Tuesday` \| `Wednesday` \| `Thursday` \| `Friday` \| `Saturday` |
| `timeSlots[].startTime` | string | yes | `HH:mm` format |
| `timeSlots[].endTime` | string | yes | `HH:mm` format |

**Response (201):**
```json
{
  "programId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**React:**
```tsx
interface TimeSlotInput {
  day: string;
  startTime: string;
  endTime: string;
  description?: string;
}

interface CreateProgramInput {
  name: string;
  description?: string;
  level?: string;
  exerciseType?: string;
  durationMinutes: number;
  startDate: string;
  endDate: string;
  totalPrice: number;
  maxParticipants: number;
  pictureUrl?: string;
  timeSlots: TimeSlotInput[];
}

async function createProgram(data: CreateProgramInput) {
  const res = await fetch(`${API}/api/programs`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to create program");
  }

  return res.json(); // { programId: "guid" }
}
```

**UI flow after success:** Show a success message. The program is now `Pending` — the admin must accept it. The coach can see it in "My Programs" with status `Pending`.

---

## 3. Admin — View Pending Programs

**User action:** Admin opens the "Program Requests" dashboard → sees a list of all programs waiting for review (name, coach name, description).

**Endpoint:** `GET /api/programs/requests`

**Auth:** `AdminOnly`

**Response (200):**
```json
[
  {
    "programId": "550e8400-e29b-41d4-a716-446655440000",
    "programName": "HIIT Training",
    "description": "High intensity interval training for all levels.",
    "coachName": "Ahmed Ali"
  }
]
```

**React:**
```tsx
async function fetchPendingPrograms() {
  const res = await fetch(`${API}/api/programs/requests`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // ListProgramRequestsResponse[]
}
```

**Display each request as a card with:**
- Program name
- Coach name
- Description (truncated)
- **Approve** and **Reject** buttons

---

## 4. Admin — Accept / Reject a Program

### 4.1 Accept Program

**User action:** Admin clicks "Approve" on a pending program → a confirmation dialog appears → confirms.

**When accepted, the system generates Session entities for each date between StartDate and EndDate that matches a TimeSlot's day of week.**

**Endpoint:** `PATCH /api/programs/requests/{programId}/accept`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "programId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Accepted"
}
```

**React:**
```tsx
async function acceptProgram(programId: string) {
  const confirmed = window.confirm(
    "Accept this program? Sessions will be generated automatically."
  );
  if (!confirmed) return;

  const res = await fetch(`${API}/api/programs/requests/${programId}/accept`, {
    method: "PATCH",
    headers: authHeaders(),
  });

  if (res.status === 409) {
    alert("Program has already been reviewed.");
    return;
  }
  if (!res.ok) throw new Error((await res.json()).detail);

  // Remove the request from the pending list
  return res.json();
}
```

### 4.2 Reject Program

**User action:** Admin clicks "Reject" → confirmation → confirms.

**Endpoint:** `PATCH /api/programs/requests/{programId}/reject`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "programId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Rejected"
}
```

**React:**
```tsx
async function rejectProgram(programId: string) {
  const confirmed = window.confirm("Reject this program?");
  if (!confirmed) return;

  const res = await fetch(`${API}/api/programs/requests/${programId}/reject`, {
    method: "PATCH",
    headers: authHeaders(),
  });

  if (!res.ok) throw new Error((await res.json()).detail);
  return res.json();
}
```

---

## 5. Coach — View My Programs

**User action:** Coach opens "My Programs" → sees all their programs with their status (Pending, Accepted, Rejected) and enrollment counts.

**Endpoint:** `GET /api/coaches/{coachId}/programs`

**Auth:** `AdminOrCoach`

**Response (200):**
```json
[
  {
    "programId": "550e8400-e29b-41d4-a716-446655440000",
    "name": "HIIT Training",
    "description": "High intensity interval training for all levels.",
    "level": "Intermediate",
    "exerciseType": "Cardio",
    "startDate": "2026-06-01",
    "endDate": "2026-08-31",
    "totalPrice": 150.00,
    "status": "Accepted",
    "enrolledCount": 5
  }
]
```

> **How to get the coachId:** After login, the coach's profile is linked to their `UserId` (from the JWT). Look up the coach profile to get their `coachId`.

**React:**
```tsx
interface CoachProgram {
  programId: string;
  name: string;
  description: string | null;
  level: string | null;
  exerciseType: string | null;
  startDate: string;
  endDate: string;
  totalPrice: number;
  status: "Pending" | "Accepted" | "Rejected";
  enrolledCount: number;
}

async function fetchCoachPrograms(coachId: string) {
  const res = await fetch(`${API}/api/coaches/${coachId}/programs`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<CoachProgram[]>;
}
```

**UI suggestions:**
- Show a colored badge per program status:
  - `Pending` → yellow / "Awaiting Approval"
  - `Accepted` → green / "Active"
  - `Rejected` → red / "Not Approved"
- Clicking an `Accepted` program should navigate to the **sessions view** (Section 6).

---

## 6. Coach — View Sessions for a Program

> **This is the new endpoint that fills the gap in the attendance flow.**

**User action:** Coach clicks an Accepted program → sees a list of all generated sessions (past and future) with dates, times, enrolled members, and attendance status.

**Endpoint:** `GET /api/programs/{programId}/sessions`

**Auth:** `AdminOrCoach`

**Response (200):**
```json
{
  "sessions": [
    {
      "sessionId": "660e8400-e29b-41d4-a716-446655440001",
      "date": "2026-06-01",
      "startTime": "08:00",
      "endTime": "09:00",
      "isCompleted": true,
      "enrolledCount": 5,
      "members": [
        {
          "memberId": "770e8400-e29b-41d4-a716-446655440002",
          "fullName": "Member-770e8400",
          "attendanceStatus": "Present"
        },
        {
          "memberId": "880e8400-e29b-41d4-a716-446655440003",
          "fullName": "Member-880e8400",
          "attendanceStatus": "Absent"
        }
      ]
    },
    {
      "sessionId": "660e8400-e29b-41d4-a716-446655440004",
      "date": "2026-06-03",
      "startTime": "08:00",
      "endTime": "09:00",
      "isCompleted": false,
      "enrolledCount": 5,
      "members": [
        {
          "memberId": "770e8400-e29b-41d4-a716-446655440002",
          "fullName": "Member-770e8400",
          "attendanceStatus": null
        }
      ]
    }
  ]
}
```

**`attendanceStatus` values:**

| Value | Meaning |
|-------|---------|
| `null` | Attendance not yet marked (future session or past but unmarked) |
| `"Present"` | Member was marked present |
| `"Absent"` | Member was marked absent |
| `"NotMarked"` | Session was marked completed but this member has no record |

**Key logic:**
- **Future sessions** (`isCompleted: false`, date >= today): `attendanceStatus` is `null` for all members
- **Past completed sessions** (`isCompleted: true`): each member shows their actual attendance
- **Past sessions not yet marked** (`isCompleted: false`, date < today): `attendanceStatus` is `null` — coach needs to mark them

**React:**
```tsx
interface MemberSession {
  memberId: string;
  fullName: string;
  attendanceStatus: string | null;
}

interface ProgramSession {
  sessionId: string;
  date: string;
  startTime: string;
  endTime: string;
  isCompleted: boolean;
  enrolledCount: number;
  members: MemberSession[];
}

interface GetProgramSessionsResponse {
  sessions: ProgramSession[];
}

async function fetchProgramSessions(programId: string) {
  const res = await fetch(`${API}/api/programs/${programId}/sessions`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error((await res.json()).detail);
  return res.json() as Promise<GetProgramSessionsResponse>;
}
```

**UI suggestions:**
- Group sessions by month or week
- `isCompleted: true` → show colored attendance badges (green = Present, red = Absent, gray = NotMarked)
- `isCompleted: false` → show an **"Mark Attendance"** button on past/future sessions. The button should navigate to the attendance marking view (Section 7).
- Show `enrolledCount` as a summary indicator

---

## 7. Coach — Mark Attendance

**User action:** Coach clicks "Mark Attendance" on a session → sees the list of enrolled members → marks each as Present or Absent → clicks "Submit".

**Endpoint:** `POST /api/sessions/{sessionId}/attendance`

**Auth:** `AdminOrCoach`

**Request body (JSON):**
```json
{
  "attendance": [
    { "memberId": "770e8400-e29b-41d4-a716-446655440002", "status": "Present" },
    { "memberId": "880e8400-e29b-41d4-a716-446655440003", "status": "Absent" }
  ]
}
```

| Field | Type | Required | Values |
|-------|------|----------|--------|
| `attendance` | array | yes | Array of attendance entries |
| `attendance[].memberId` | string (guid) | yes | Member's UUID |
| `attendance[].status` | string | yes | `"Present"` or `"Absent"` |

**Response (200):**
```json
{
  "sessionId": "660e8400-e29b-41d4-a716-446655440001",
  "markedCount": 5
}
```

> ⚠️ Returns **409 Conflict** if attendance has already been marked for this session (prevents double-marking).

**React:**
```tsx
interface AttendanceEntry {
  memberId: string;
  status: "Present" | "Absent";
}

async function markAttendance(sessionId: string, attendance: AttendanceEntry[]) {
  const res = await fetch(`${API}/api/sessions/${sessionId}/attendance`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ attendance }),
  });

  if (res.status === 409) {
    alert("Attendance has already been marked for this session.");
    return;
  }
  if (!res.ok) throw new Error((await res.json()).detail);

  return res.json(); // { sessionId, markedCount }
}
```

**Complete attendance marking component:**

```tsx
function AttendanceSheet({
  session,
  onMarked,
}: {
  session: ProgramSession;
  onMarked: () => void;
}) {
  const [entries, setEntries] = useState<AttendanceEntry[]>(
    session.members.map((m) => ({
      memberId: m.memberId,
      status: "Present" as const,
    }))
  );
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit() {
    setSubmitting(true);
    try {
      await markAttendance(session.sessionId, entries);
      alert(`Attendance marked for ${entries.length} members`);
      onMarked(); // Refresh the sessions list
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to mark attendance");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div>
      <h3>Session: {session.date} ({session.startTime} - {session.endTime})</h3>
      <table>
        <thead>
          <tr><th>Member</th><th>Status</th></tr>
        </thead>
        <tbody>
          {session.members.map((member, i) => (
            <tr key={member.memberId}>
              <td>{member.fullName}</td>
              <td>
                <select
                  value={entries[i].status}
                  onChange={(e) => {
                    const updated = [...entries];
                    updated[i] = {
                      ...updated[i],
                      status: e.target.value as "Present" | "Absent",
                    };
                    setEntries(updated);
                  }}
                >
                  <option value="Present">Present</option>
                  <option value="Absent">Absent</option>
                </select>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <button onClick={handleSubmit} disabled={submitting}>
        {submitting ? "Saving..." : "Submit Attendance"}
      </button>
    </div>
  );
}
```

**After marking, the coach can:**
- Navigate back to the sessions list (Section 6)
- Refresh: the session will now show `isCompleted: true` and each member's `attendanceStatus` will be `"Present"` or `"Absent"`

---

## 8. Member — View Available Programs

**User action:** Member opens the "Programs" tab → sees a list of programs they can enroll in (Accepted programs they're not already enrolled in).

**Endpoint:** `GET /api/programs/available`

**Auth:** Any authenticated user (member, coach, admin)

**Response (200):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "HIIT Training",
    "imageUrl": "https://example.com/hiit.jpg",
    "price": 150.00,
    "coachId": "990e8400-e29b-41d4-a716-446655440005",
    "coachName": "Ahmed Ali",
    "description": "High intensity interval training for all levels."
  }
]
```

**React:**
```tsx
interface AvailableProgram {
  id: string;
  name: string;
  imageUrl: string | null;
  price: number;
  coachId: string;
  coachName: string | null;
  description: string | null;
}

async function fetchAvailablePrograms() {
  const res = await fetch(`${API}/api/programs/available`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<AvailableProgram[]>;
}
```

---

## 9. Member — View Program Detail

**User action:** Member clicks on a program card → sees full details: description, coach info, time slots, price, available spots.

**Endpoint:** `GET /api/programs/{programId}`

**Auth:** Any authenticated user

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "HIIT Training",
  "price": 150.00,
  "description": "High intensity interval training for all levels.",
  "spotsLeft": 15,
  "capacity": 20,
  "coachId": "990e8400-e29b-41d4-a716-446655440005",
  "coachName": "Ahmed Ali",
  "level": "Intermediate",
  "exerciseType": "Cardio",
  "durationMinutes": 60,
  "timeSlots": [
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440006",
      "day": "Monday",
      "startTime": "08:00",
      "endTime": "09:00",
      "description": "Morning session"
    }
  ]
}
```

**React:**
```tsx
interface ProgramDetail {
  id: string;
  name: string;
  price: number;
  description: string | null;
  spotsLeft: number;
  capacity: number;
  coachId: string;
  coachName: string | null;
  level: string | null;
  exerciseType: string | null;
  durationMinutes: number;
  timeSlots: ProgramTimeSlot[];
}

async function fetchProgramDetail(programId: string) {
  const res = await fetch(`${API}/api/programs/${programId}`, {
    headers: authHeaders(),
  });
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<ProgramDetail>;
}
```

**Show enrollment CTA:**
- If `spotsLeft > 0`: show **"Enroll Now"** button → calls purchase endpoint (Section 10)
- If `spotsLeft === 0`: show **"Program Full"** badge

---

## 10. Member — Purchase Program (Online)

**User action:** Member clicks "Enroll Now" → payment form appears → enters amount (or uses the program price) → submits → enrollment created immediately.

**Endpoint:** `POST /api/programs/{programId}/purchase/online`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "amount": 150.00,
  "notes": "Enrolling in HIIT Training"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `amount` | number | yes | Payment amount (should match program price) |
| `notes` | string | no | Optional note |

**Response (200):**
```json
{
  "enrollmentId": "bb0e8400-e29b-41d4-a716-446655440007",
  "programId": "550e8400-e29b-41d4-a716-446655440000",
  "programName": "HIIT Training",
  "paymentId": "cc0e8400-e29b-41d4-a716-446655440008",
  "amount": 150.00,
  "paymentMethod": "Online",
  "purchasedAt": "2026-06-03T10:30:00Z"
}
```

**React:**
```tsx
async function purchaseProgramOnline(programId: string, amount: number) {
  const res = await fetch(`${API}/api/programs/${programId}/purchase/online`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ amount }),
  });

  if (res.status === 409) {
    alert("You are already enrolled in this program.");
    return;
  }
  if (!res.ok) throw new Error((await res.json()).detail);

  return res.json(); // PurchaseOnlineResponse
}
```

> **Cash purchase flow:** There's also `POST /api/programs/{programId}/purchase/cash` which creates a pending request. The admin must approve it via `PATCH /api/programs/purchase/{requestId}/accept`. For simplicity, this tutorial focuses on the online flow.

---

## 11. Member — View Enrolled Programs & Session History

### 11.1 Enrolled Programs

**User action:** Member opens "My Programs" → sees programs they're enrolled in.

**Endpoint:** `GET /api/programs/enrolled`

**Auth:** `MemberOnly`

**Response (200):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "HIIT Training",
    "coachId": "990e8400-e29b-41d4-a716-446655440005",
    "coachName": "Ahmed Ali",
    "description": "High intensity interval training for all levels.",
    "enrolledAt": "2026-06-03T10:30:00Z"
  }
]
```

**React:**
```tsx
async function fetchEnrolledPrograms() {
  const res = await fetch(`${API}/api/programs/enrolled`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

### 11.2 Session History (with Attendance)

**User action:** Member opens a program → taps "My Sessions" → sees their attendance history for each session in a date range.

**Endpoint:** `GET /api/sessions/history?startDate=2026-06-01&endDate=2026-08-31`

**Auth:** `MemberOnly`

**Response (200):**
```json
{
  "sessions": [
    {
      "sessionId": "660e8400-e29b-41d4-a716-446655440001",
      "date": "2026-06-01",
      "startTime": "08:00",
      "endTime": "09:00",
      "isCompleted": true,
      "programId": "550e8400-e29b-41d4-a716-446655440000",
      "programName": "HIIT Training",
      "attendanceStatus": "Present"
    }
  ]
}
```

**`attendanceStatus` for the member:**
- `"NotMarked"` — attendance hasn't been taken yet
- `"Present"` — coach marked them present
- `"Absent"` — coach marked them absent

**React:**
```tsx
async function fetchSessionHistory(startDate: string, endDate: string) {
  const params = new URLSearchParams({ startDate, endDate });
  const res = await fetch(`${API}/api/sessions/history?${params}`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // GetSessionHistoryResponse
}
```

**UI suggestion:** Show a calendar view or list. Use colored indicators:
- Green dot = Present
- Red dot = Absent
- Gray dot = Not Marked

---

## 12. Error Handling

All error responses follow the RFC 9457 Problem Details format:

```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "The specified program does not exist."
}
```

| HTTP Status | Meaning | Common Causes |
|------------|---------|---------------|
| **200** | Success | GET, POST (purchase), PATCH (accept/reject) |
| **201** | Created | POST (create program) |
| **400** | Bad Request | Invalid JSON, missing required fields |
| **401** | Unauthorized | Missing or expired JWT |
| **403** | Forbidden | Valid JWT but wrong role |
| **404** | Not Found | Program/session ID doesn't exist |
| **409** | Conflict | Already enrolled, already reviewed, attendance already marked |

**React generic handler:**
```tsx
async function handleApiResponse(res: Response): Promise<any> {
  if (res.ok) {
    if (res.status === 204) return null;
    return res.json();
  }
  const body = await res.json().catch(() => ({ detail: res.statusText }));
  throw new ApiError(body.status || res.status, body.detail || "Unknown error");
}

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}
```

---

## 13. Endpoint Reference Card

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/programs` | CoachOnly | `CreateProgramRequest` | `{ programId }` (201) |
| `GET` | `/api/programs/requests` | AdminOnly | — | `ListProgramRequestsResponse[]` |
| `PATCH` | `/api/programs/requests/{id}/accept` | AdminOnly | — | `{ programId, status }` |
| `PATCH` | `/api/programs/requests/{id}/reject` | AdminOnly | — | `{ programId, status }` |
| `GET` | `/api/coaches/{coachId}/programs` | AdminOrCoach | — | `GetCoachProgramsResponse[]` |
| `GET` | `/api/programs/{programId}/sessions` | AdminOrCoach | — | `GetProgramSessionsResponse` |
| `POST` | `/api/sessions/{sessionId}/attendance` | AdminOrCoach | `MarkAttendanceRequest` | `{ sessionId, markedCount }` |
| `GET` | `/api/programs/available` | Any auth | — | `GetAvailableProgramsResponse[]` |
| `GET` | `/api/programs/{programId}` | Any auth | — | `GetProgramDetailResponse` |
| `POST` | `/api/programs/{programId}/purchase/online` | MemberOnly | `{ amount }` | `PurchaseOnlineResponse` |
| `GET` | `/api/programs/enrolled` | MemberOnly | — | `GetEnrolledProgramsResponse[]` |
| `GET` | `/api/sessions/history?startDate=...&endDate=...` | MemberOnly | — | `GetSessionHistoryResponse` |

### Quick Status Values Reference

**Program status:** `Pending` | `Accepted` | `Rejected`

**Attendance status (coach view):** `null` (not marked) | `"Present"` | `"Absent"` | `"NotMarked"`

**Attendance status (member view):** `"NotMarked"` | `"Present"` | `"Absent"`

**Auth policies:** `AdminOnly` | `CoachOnly` | `MemberOnly` | `AdminOrCoach`

---

### End-to-End Flow Summary

```
Coach              Admin              Member
  │                  │                  │
  ├─ POST /programs ─┤                  │
  │  (status: Pending)                  │
  │                  │                  │
  │        GET /programs/requests       │
  │                  │                  │
  │  PATCH .../accept │                  │
  │  (sessions auto-generated!)         │
  │                  │                  │
  ├─ GET /coaches/{id}/programs ────────┤
  │  (see Accepted status)              │
  │                                     │
  ├─ GET /programs/{id}/sessions ───────┤
  │  (see all session IDs, dates)       │
  │                                     │
  ├─ POST /sessions/{id}/attendance ────┤
  │  (mark Present/Absent per member)   │
  │                          │          │
  │               GET /programs/available│
  │               GET /programs/{id}    │
  │               POST .../purchase/online│
  │                          │          │
  │               GET /sessions/history │
  │               (see own attendance)  │
```
