# Workout Logs — Frontend Integration Guide

Base URL (dev, via gateway): `http://localhost:5106`

Every request must include the JWT bearer token in the `Authorization` header:

```http
Authorization: Bearer <token>
```

> Token is obtained from the Identity service at `http://localhost:5051`.
> Token format: RS256 JWT with claims `sub` (user UUID), `email`, `role` (Admin | Member | Coach).

---

## Table of Contents

1. [Auth Setup](#1-auth-setup)
2. [Flow Overview](#2-flow-overview)
3. [Step 1 — Get Your MemberId](#3-step-1--get-your-memberid)
4. [Step 2 — Get Session History](#4-step-2--get-session-history)
5. [Step 3 — Create a Workout Log](#5-step-3--create-a-workout-log)
6. [Step 4 — List Workout Logs](#6-step-4--list-workout-logs)
7. [Step 5 — Get a Single Workout Log](#7-step-5--get-a-single-workout-log)
8. [Step 6 — Update a Workout Log](#8-step-6--update-a-workout-log)
9. [Complete User Journey Example](#9-complete-user-journey-example)
10. [Error Handling](#10-error-handling)
11. [Endpoint Reference Card](#11-endpoint-reference-card)

---

## 1. Auth Setup

```tsx
const API = "http://localhost:5106";

function authHeaders(): HeadersInit {
  const token = localStorage.getItem("jwt_token");
  return { Authorization: `Bearer ${token}` };
}
```

---

## 2. Flow Overview

The workout-logs service lets members log exercises they performed during a gym session. The data flow is:

```
1. GET /api/me               → get your memberId (Membership internal ID)
2. GET /api/me/sessions      → get session history (get SessionId from a past session)
3. POST /api/workout-logs     → create workout log with memberId + activitySessionId + exercises
```

**Important — Two IDs you need to know:**

| ID | What it is | Where to get it |
|----|-----------|-----------------|
| `memberId` | Your Membership-internal ID (`Member.Id`) | `GET /api/me` → `memberId` |
| `activitySessionId` | A gym session ID (`MemberActivity.Id`) | `GET /api/me/sessions` → `sessionId` |

> ⚠️ `memberId` is **NOT** the same as your JWT `sub` (UserId). Use the value returned by `GET /api/me`, not the JWT claim.

---

## 3. Step 1 — Get Your MemberId

**User action:** After login, the app fetches the member's profile to display their name and other info — and to extract the `memberId`.

**Endpoint:** `GET /api/me`

**Auth:** `MemberOnly`

**Response (200):**
```json
{
  "memberId": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "Ahmed",
  "lastName": "Ali",
  "gender": "Male",
  "dateOfBirth": "1995-06-15",
  "phoneNumber": "+201234567890",
  "email": "ahmed@example.com",
  "emailConfirmed": true,
  "accountCreationDate": "2026-01-15T10:30:00Z",
  "membershipDurationYears": 1,
  "isActive": true,
  "profilePictureUrl": "https://example.com/avatar.jpg",
  "goals": "Build muscle and improve cardio",
  "medicalFileUrl": null
}
```

**React:**
```tsx
interface MemberProfile {
  memberId: string;          // ✅ Use this for workout-logs
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  // ... other fields
}

async function fetchMyProfile(): Promise<MemberProfile> {
  const res = await fetch(`${API}/api/me`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

**Store the `memberId`** — you'll need it for every workout-logs request.

---

## 4. Step 2 — Get Session History

**User action:** Member opens the "My Workouts" screen → sees a list of their past gym sessions. They pick one to log exercises for.

**Endpoint:** `GET /api/me/sessions`

**Auth:** `MemberOnly`

**Response (200):**
```json
[
  {
    "sessionId": "660e8400-e29b-41d4-a716-446655440001",
    "checkInTime": "2026-06-03T08:15:00Z",
    "checkOutTime": "2026-06-03T09:45:00Z",
    "courseId": null,
    "isManual": false
  },
  {
    "sessionId": "770e8400-e29b-41d4-a716-446655440002",
    "checkInTime": "2026-06-01T10:00:00Z",
    "checkOutTime": null,
    "courseId": null,
    "isManual": true
  }
]
```

**React:**
```tsx
interface SessionHistoryItem {
  sessionId: string;          // ✅ Use this as activitySessionId in workout-logs
  checkInTime: string;
  checkOutTime: string | null;
  courseId: string | null;
  isManual: boolean;
}

async function fetchSessionHistory(): Promise<SessionHistoryItem[]> {
  const res = await fetch(`${API}/api/me/sessions`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

**Which session to pick?** — Let the member select a past session from this list. The `sessionId` becomes the `activitySessionId` when creating a workout log.

---

## 5. Step 3 — Create a Workout Log

**User action:** Member selects a session → taps "Add Workout" → fills in exercises (name, description, calories burned, duration) → taps "Save".

**Endpoint:** `POST /api/workout-logs`

**Auth:** None (gateway passthrough — the memberId in the body identifies the user)

**Request body (JSON):**
```json
{
  "memberId": "550e8400-e29b-41d4-a716-446655440000",
  "activitySessionId": "660e8400-e29b-41d4-a716-446655440001",
  "notes": "Felt strong today!",
  "exercises": [
    {
      "name": "Bench Press",
      "description": "4 sets of 8 reps at 80kg",
      "caloriesBurned": 120.5,
      "durationMinutes": 20
    },
    {
      "name": "Squat",
      "description": "4 sets of 10 reps at 100kg",
      "caloriesBurned": 150.0,
      "durationMinutes": 25
    }
  ]
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `memberId` | string (uuid) | **yes** | From `GET /api/me` → `memberId` |
| `activitySessionId` | string (uuid) | **yes** | From `GET /api/me/sessions` → `sessionId` |
| `notes` | string | no | Free text |
| `exercises` | array | yes | At least 1 exercise |
| `exercises[].name` | string | **yes** | Exercise name |
| `exercises[].description` | string | no | e.g. sets/reps/weight |
| `exercises[].caloriesBurned` | number | no | Decimal |
| `exercises[].durationMinutes` | integer | no | Minutes spent on this exercise |

**Response (200):**
```json
{
  "id": "880e8400-e29b-41d4-a716-446655440003",
  "memberId": "550e8400-e29b-41d4-a716-446655440000",
  "activitySessionId": "660e8400-e29b-41d4-a716-446655440001",
  "notes": "Felt strong today!",
  "exercises": [
    {
      "id": "990e8400-e29b-41d4-a716-446655440004",
      "name": "Bench Press",
      "description": "4 sets of 8 reps at 80kg",
      "caloriesBurned": 120.5,
      "durationMinutes": 20
    },
    {
      "id": "aa0e8400-e29b-41d4-a716-446655440005",
      "name": "Squat",
      "description": "4 sets of 10 reps at 100kg",
      "caloriesBurned": 150.0,
      "durationMinutes": 25
    }
  ],
  "createdAt": "2026-06-03T10:00:00Z",
  "updatedAt": "2026-06-03T10:00:00Z"
}
```

**React:**
```tsx
interface ExerciseInput {
  name: string;
  description?: string;
  caloriesBurned?: number;
  durationMinutes?: number;
}

interface CreateWorkoutLogInput {
  memberId: string;
  activitySessionId: string;
  notes?: string;
  exercises: ExerciseInput[];
}

interface WorkoutLogResponse {
  id: string;
  memberId: string;
  activitySessionId: string;
  notes: string | null;
  exercises: ExerciseResponse[];
  createdAt: string;
  updatedAt: string;
}

async function createWorkoutLog(data: CreateWorkoutLogInput) {
  const res = await fetch(`${API}/api/workout-logs`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!res.ok) {
    const error = await res.json().catch(() => ({ detail: res.statusText }));
    throw new Error(error.detail || "Failed to create workout log");
  }

  return res.json() as Promise<WorkoutLogResponse>;
}
```

**UI example — Exercise form:**
```tsx
function WorkoutLogForm({
  memberId,
  selectedSessionId,
  onSaved,
}: {
  memberId: string;
  selectedSessionId: string;
  onSaved: () => void;
}) {
  const [exercises, setExercises] = useState<ExerciseInput[]>([
    { name: "", description: "", caloriesBurned: 0, durationMinutes: 0 },
  ]);
  const [notes, setNotes] = useState("");
  const [submitting, setSubmitting] = useState(false);

  function addExercise() {
    setExercises([...exercises, { name: "", description: "", caloriesBurned: 0, durationMinutes: 0 }]);
  }

  function updateExercise(index: number, field: string, value: any) {
    const updated = [...exercises];
    (updated[index] as any)[field] = value;
    setExercises(updated);
  }

  function removeExercise(index: number) {
    setExercises(exercises.filter((_, i) => i !== index));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitting(true);
    try {
      await createWorkoutLog({
        memberId,
        activitySessionId: selectedSessionId,
        notes: notes || undefined,
        exercises: exercises.filter((ex) => ex.name.trim()),
      });
      alert("Workout logged successfully!");
      onSaved();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to save workout");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      {exercises.map((ex, i) => (
        <div key={i} style={{ border: "1px solid #ddd", padding: 12, marginBottom: 8 }}>
          <input
            placeholder="Exercise name"
            value={ex.name}
            onChange={(e) => updateExercise(i, "name", e.target.value)}
            required
          />
          <input
            placeholder="Description (e.g., 4x8 at 80kg)"
            value={ex.description}
            onChange={(e) => updateExercise(i, "description", e.target.value)}
          />
          <input
            type="number"
            placeholder="Calories burned"
            value={ex.caloriesBurned || ""}
            onChange={(e) => updateExercise(i, "caloriesBurned", parseFloat(e.target.value) || 0)}
          />
          <input
            type="number"
            placeholder="Duration (minutes)"
            value={ex.durationMinutes || ""}
            onChange={(e) => updateExercise(i, "durationMinutes", parseInt(e.target.value) || 0)}
          />
          {exercises.length > 1 && (
            <button type="button" onClick={() => removeExercise(i)}>Remove</button>
          )}
        </div>
      ))}
      <button type="button" onClick={addExercise}>+ Add Exercise</button>
      <br />
      <textarea
        placeholder="Notes (optional)"
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
      />
      <br />
      <button type="submit" disabled={submitting}>
        {submitting ? "Saving..." : "Save Workout"}
      </button>
    </form>
  );
}
```

---

## 6. Step 4 — List Workout Logs

**User action:** Member opens "My Workouts" → sees a list of their workout logs filtered by memberId.

**Endpoint:** `GET /api/workout-logs?memberId={memberId}`

**Auth:** None (gateway passthrough)

**Query parameters:**

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `memberId` | uuid | **yes** | Your `memberId` from `GET /api/me` |
| `from` | datetime | no | Filter by start date (ISO 8601) |
| `to` | datetime | no | Filter by end date (ISO 8601) |
| `activitySessionId` | uuid | no | Filter by a specific session |

**Response (200):**
```json
[
  {
    "id": "880e8400-e29b-41d4-a716-446655440003",
    "memberId": "550e8400-e29b-41d4-a716-446655440000",
    "activitySessionId": "660e8400-e29b-41d4-a716-446655440001",
    "notes": "Felt strong today!",
    "exercises": [
      { "id": "990e8400-...", "name": "Bench Press", "description": "4x8 at 80kg", "caloriesBurned": 120.5, "durationMinutes": 20 },
      { "id": "aa0e8400-...", "name": "Squat", "description": "4x10 at 100kg", "caloriesBurned": 150.0, "durationMinutes": 25 }
    ],
    "createdAt": "2026-06-03T10:00:00Z",
    "updatedAt": "2026-06-03T10:00:00Z"
  }
]
```

**React:**
```tsx
async function fetchWorkoutLogs(memberId: string, from?: string, to?: string) {
  const params = new URLSearchParams({ memberId });
  if (from) params.append("from", from);
  if (to) params.append("to", to);

  const res = await fetch(`${API}/api/workout-logs?${params}`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json() as Promise<WorkoutLogResponse[]>;
}
```

**UI suggestion:** Group logs by date, show exercise count and total calories per log.

---

## 7. Step 5 — Get a Single Workout Log

**User action:** Member taps a workout log → sees full details including all exercises.

**Endpoint:** `GET /api/workout-logs/{id}`

**Auth:** None (gateway passthrough)

**Response (200):** Same shape as a single item in the list above.

**React:**
```tsx
async function getWorkoutLog(id: string): Promise<WorkoutLogResponse | null> {
  const res = await fetch(`${API}/api/workout-logs/${id}`, {
    headers: authHeaders(),
  });
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

---

## 8. Step 6 — Update a Workout Log

**User action:** Member edits a workout log → changes exercises or notes → taps "Save".

**Endpoint:** `PUT /api/workout-logs/{id}`

**Auth:** None (gateway passthrough)

**Request body (JSON):**
```json
{
  "notes": "Updated notes — pushed harder today!",
  "exercises": [
    {
      "name": "Bench Press",
      "description": "5 sets of 5 reps at 90kg",
      "caloriesBurned": 140.0,
      "durationMinutes": 25
    }
  ]
}
```

> Note: The `memberId` and `activitySessionId` are **not** included in the update request — they cannot be changed after creation.

**Response (200):** Updated `WorkoutLogResponse` JSON.

**React:**
```tsx
async function updateWorkoutLog(id: string, data: {
  notes?: string;
  exercises: ExerciseInput[];
}) {
  const res = await fetch(`${API}/api/workout-logs/${id}`, {
    method: "PUT",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (!res.ok) throw new Error((await res.json()).detail);
  return res.json() as Promise<WorkoutLogResponse>;
}
```

---

## 9. Complete User Journey Example

Here's the full flow a member goes through, from login to saving a workout:

```tsx
// Step 1: After login, fetch profile to get memberId
const profile = await fetchMyProfile();
const memberId = profile.memberId;

// Step 2: Show session history for the member to pick from
const sessions = await fetchSessionHistory();
// Member picks one — for example, the most recent session
const selectedSession = sessions[0];

// Step 3: Member fills in exercises and saves
const newLog = await createWorkoutLog({
  memberId,
  activitySessionId: selectedSession.sessionId,
  notes: "Great leg day!",
  exercises: [
    { name: "Squat", description: "4x10 at 100kg", caloriesBurned: 150, durationMinutes: 25 },
    { name: "Leg Press", description: "3x12 at 150kg", caloriesBurned: 100, durationMinutes: 15 },
  ],
});

// newLog.id is the workout log ID — you can now navigate to the detail view
console.log(`Workout log created: ${newLog.id}`);
```

**Remember:** The IDs come from different endpoints — don't confuse them:

```
memberId             ← GET /api/me                → Member.Id (Membership)
activitySessionId    ← GET /api/me/sessions       → SessionId (Activity service)
```

---

## 10. Error Handling

All error responses follow the RFC 9457 Problem Details format:

```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "detail": "name is required"
}
```

| HTTP Status | Meaning | Common Causes |
|------------|---------|---------------|
| **200** | Success | GET, POST, PUT |
| **400** | Bad Request | Missing required field (`name`), invalid UUID format |
| **404** | Not Found | Workout log ID doesn't exist |

> Note: The workout-logs service does not enforce authentication at the service level. Requests reach it through the gateway which passes the JWT but does not validate it for workout-logs routes. The `memberId` in the request body identifies the owner. This is acceptable for development/MVP.

---

## 11. Endpoint Reference Card

| Method | Path | Body | Returns |
|--------|------|------|---------|
| `GET` | `/api/me` | — | `GetMyProfileResponse` (includes `memberId`) |
| `GET` | `/api/me/sessions` | — | `MeSessionResponse[]` |
| `POST` | `/api/workout-logs` | `CreateWorkoutLogRequest` | `WorkoutLogResponse` |
| `GET` | `/api/workout-logs?memberId=...` | — | `WorkoutLogResponse[]` |
| `GET` | `/api/workout-logs/{id}` | — | `WorkoutLogResponse` |
| `PUT` | `/api/workout-logs/{id}` | `UpdateWorkoutLogRequest` | `WorkoutLogResponse` |

### Data Model Reference

**`CreateWorkoutLogRequest`**
```json
{
  "memberId": "uuid (required, from GET /api/me)",
  "activitySessionId": "uuid (required, from GET /api/me/sessions)",
  "notes": "string (optional)",
  "exercises": [
    {
      "name": "string (required)",
      "description": "string (optional)",
      "caloriesBurned": "number (optional)",
      "durationMinutes": "integer (optional)"
    }
  ]
}
```

**`WorkoutLogResponse`**
```json
{
  "id": "uuid",
  "memberId": "uuid",
  "activitySessionId": "uuid",
  "notes": "string | null",
  "exercises": [ { "id": "uuid", "name": "string", ... } ],
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### Full Integration Flow

```
Member clicks "My Workouts"
        ↓
1. GET /api/me
        ↓  { memberId: "550e...", ... }
   Store memberId in app state
        ↓
2. GET /api/me/sessions
        ↓  [{ sessionId: "660e...", checkInTime: "...", ... }, ...]
   Show session list → member picks one
        ↓
3. Show "Add Workout" form (pre-filled with memberId + selected sessionId)
   Member fills in exercises → taps Save
        ↓
4. POST /api/workout-logs
   { memberId, activitySessionId, exercises }
        ↓  { id: "880e...", ... }
   Navigate to workout detail / refresh list
```
