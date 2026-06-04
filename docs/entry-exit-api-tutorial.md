# Entry / Exit — Frontend Integration Guide

Base URL (dev): `http://localhost:<port>`

| Service | Port | Endpoints |
|---------|------|-----------|
| **Activity API** | `5104` | Scan, Manual Entry, Manual Exit |
| **Membership API** | (internal) | Card resolution, session tracking |

All scan/entry/exit endpoints require the **Admin** role.

---

## Authentication

```http
Authorization: Bearer <token>
```

Token from Identity service (`http://localhost:5051`).  
Required claims: `sub` (user UUID), `role` (must include `Admin`).

---

## Table of Contents

1. [NFC Card Scan — Auto Entry/Exit](#1-nfc-card-scan--entryexit)
2. [Manual Entry — Admin Selects Member](#2-manual-entry-admin-selects-member)
3. [Manual Exit — Admin Selects Member](#3-manual-exit-admin-selects-member)
4. [Test Data for Development](#4-test-data-for-development)
5. [Error Reference](#5-error-reference)
6. [Complete Test Flow Walkthrough](#6-complete-test-flow-walkthrough)

---

## 1. NFC Card Scan — Entry/Exit

**User action:** Member taps NFC card on the kiosk reader at the gym entrance.

The system auto-detects whether this is an **entry** or **exit**:
- **No active session** → member is entering → eligibility checks run → session created
- **Active session exists** → member is exiting → session closed

**Endpoint:** `POST /api/activity/entry-exit/scan`

**Request:**
```json
{
  "cardUid": "CARD-0001"
}
```

---

### ✅ Entry Response (200)

```json
{
  "success": true,
  "verificationType": "Entering",
  "memberName": "John Doe",
  "remainingSessions": 29,
  "activeMembership": {
    "membershipId": "550e8400-e29b-41d4-a716-446655440000",
    "planName": "Standard Monthly",
    "endDate": "2026-07-02T10:30:00Z"
  },
  "activeCourses": []
}
```

| Field | Meaning |
|-------|---------|
| `verificationType` | `"Entering"` |
| `remainingSessions` | `null` = unlimited plan, `0` = exhausted, `>0` = remaining |
| `activeMembership` | Current active subscription info |
| `activeCourses` | Reserved for future use (course attendance) |

---

### ✅ Exit Response (200)

```json
{
  "success": true,
  "verificationType": "Exiting",
  "memberName": "John Doe",
  "remainingSessions": null,
  "activeMembership": null,
  "activeCourses": []
}
```

---

### ❌ Error Responses

**403 — Ineligible member:**
```json
{
  "type": "about:blank",
  "title": "Forbidden",
  "status": 403,
  "detail": "Member is Suspended. Only active members can enter."
}
```

**Possible `detail` values:**
- `"Member is Suspended. Only active members can enter."`
- `"No active subscription found. The member must have an active subscription to enter."`
- `"Subscription is Expired."`
- `"All sessions have been used. The subscription has no remaining sessions."`

**404 — Card not found:**
```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "No active member found for card UID 'INVALID-CARD'. The card may not be assigned."
}
```

---

### React Implementation

```tsx
const ACTIVITY_API = "http://localhost:5104";

function authHeaders(): HeadersInit {
  const token = localStorage.getItem("jwt_token");
  return { Authorization: `Bearer ${token}`, "Content-Type": "application/json" };
}

/** Scan an NFC card. Returns { success, verificationType, memberName, remainingSessions, activeMembership } */
async function scanCard(cardUid: string) {
  const res = await fetch(`${ACTIVITY_API}/api/activity/entry-exit/scan`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ cardUid }),
  });

  if (res.status === 200) {
    return { ok: true, data: await res.json() } as const;
  }

  const error = await res.json().catch(() => ({ detail: res.statusText }));
  return { ok: false, status: res.status, error: error.detail } as const;
}
```

**Kiosk screen example:**
```tsx
function NfcKiosk() {
  const [cardUid, setCardUid] = useState("");
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleScan(uid: string) {
    setLoading(true);
    setError(null);
    setResult(null);

    const response = await scanCard(uid);

    if (response.ok) {
      setResult(response.data);
    } else {
      setError(response.error);
    }

    setLoading(false);
  }

  // Render based on result
  if (result) {
    return (
      <div className={`result ${result.verificationType === "Entering" ? "enter" : "exit"}`}>
        <h1>{result.verificationType === "Entering" ? "🚪 Welcome" : "👋 Goodbye"}</h1>
        <p className="member-name">{result.memberName}</p>
        {result.remainingSessions !== null && (
          <p>Sessions remaining: {result.remainingSessions}</p>
        )}
        {result.activeMembership && (
          <p>Plan: {result.activeMembership.planName}</p>
        )}
      </div>
    );
  }

  return (
    <div className="kiosk">
      <h1>Tap your NFC card</h1>
      <input
        autoFocus
        placeholder="Card UID (dev)"
        value={cardUid}
        onChange={e => setCardUid(e.target.value)}
        onKeyDown={e => e.key === "Enter" && handleScan(cardUid)}
        disabled={loading}
      />
      {error && <div className="error">{error}</div>}
    </div>
  );
}
```

---

## 2. Manual Entry — Admin Selects Member

**User action:** Admin opens the member management page → searches for a member → clicks "Mark Entry".

**Endpoint:** `POST /api/activity/entry-exit/manual/enter`

**Request:**
```json
{
  "memberId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (201):**
```json
{
  "sessionId": "660e8400-e29b-41d4-a716-446655440001",
  "checkInTime": "2026-06-03T08:30:00Z"
}
```

**Error — Already checked in (409):**
```json
{
  "type": "about:blank",
  "title": "Conflict",
  "status": 409,
  "detail": "This member already has an active session. Check them out first before creating a new entry."
}
```

**React:**
```tsx
async function manualEntry(memberId: string) {
  const res = await fetch(`${ACTIVITY_API}/api/activity/entry-exit/manual/enter`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ memberId }),
  });

  if (res.status === 201) {
    return { ok: true, data: await res.json() } as const;
  }

  if (res.status === 409) {
    return { ok: false, status: 409, error: "Member already checked in" } as const;
  }

  const error = await res.json().catch(() => ({ detail: res.statusText }));
  return { ok: false, status: res.status, error: error.detail } as const;
}
```

---

## 3. Manual Exit — Admin Selects Member

**User action:** Admin finds a member who is currently checked in → clicks "Mark Exit".

**Endpoint:** `POST /api/activity/entry-exit/manual/exit`

**Request:**
```json
{
  "memberId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (200):**
```json
{
  "sessionId": "660e8400-e29b-41d4-a716-446655440001",
  "checkOutTime": "2026-06-03T10:15:00Z"
}
```

**React:**
```tsx
async function manualExit(memberId: string) {
  const res = await fetch(`${ACTIVITY_API}/api/activity/entry-exit/manual/exit`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ memberId }),
  });

  if (res.status === 200) {
    return { ok: true, data: await res.json() } as const;
  }

  const error = await res.json().catch(() => ({ detail: res.statusText }));
  return { ok: false, status: res.status, error: error.detail } as const;
}
```

---

## 4. Test Data for Development

The Membership seeder creates **50 test members** with NFC cards and subscriptions.

| Card UID | Member Email | Password | Plan Type | Test Scenario |
|----------|-------------|----------|-----------|---------------|
| `CARD-0001` | `seed.member1@fitteck.com` | `Member@12345` | Monthly (unlimited) | Normal entry/exit, no session limits |
| `CARD-0007` | `seed.member7@fitteck.com` | `Member@12345` | 10-Session Pack | Session-limited — test decrement + auto-expire |
| `CARD-0008` | `seed.member8@fitteck.com` | `Member@12345` | 10-Session Pack | Same plan, different member |

**Admin login:** `admin` / `Admin@12345` (from Identity seeder)

---

## 5. Error Reference

| HTTP Status | Error Code | Meaning | When |
|------------|-----------|---------|------|
| **401** | — | Missing/expired JWT | No token or token expired |
| **403** | `EntryExit.Unauthorized` | Not an admin | User role is not Admin |
| **403** | `Member.NotActive` | Member suspended/paused | `member.Status != "Active"` |
| **403** | `Subscription.None` | No subscription | Member has no active subscription |
| **403** | `Subscription.NotActive` | Subscription expired | `subscription.Status != "Active"` |
| **403** | `Subscription.NoSessions` | Session pack empty | `RemainingSessions <= 0` |
| **403** | `EntryExit.EligibilityFailed` | Generic eligibility failure | Manual entry track-entry failed |
| **404** | `Card.NotRegistered` | Card not found | Card UID doesn't match any active card |
| **404** | `Session.NotFound` | No active session | Manual exit but member isn't checked in |
| **409** | `EntryExit.AlreadyCheckedIn` | Already inside | Manual entry but member already has active session |

---

## 6. Complete Test Flow Walkthrough

### Test 1 — Unlimited Plan (CARD-0001)

```
Step 1: Login as admin
         POST /auth/login  { email: "admin", password: "Admin@12345" }

Step 2: Scan entry
         POST /api/activity/entry-exit/scan  { cardUid: "CARD-0001" }
         → 200 { verificationType: "Entering", memberName: "...", remainingSessions: null }

Step 3: Scan exit (same card)
         POST /api/activity/entry-exit/scan  { cardUid: "CARD-0001" }
         → 200 { verificationType: "Exiting", memberName: "..." }

Result: Member entered and exited successfully.
        remainingSessions: null → unlimited plan, no decrement.
```

### Test 2 — Session-Limited Plan (CARD-0007 — 10 sessions)

```
Step 1: Scan entry → exit → repeat 9 more times
         Each entry decrements remainingSessions: 9, 8, 7, ..., 1, 0

Step 2: On the 10th entry (sessions reach 0):
         Subscription auto-expires → status changes to Expired

Step 3: 11th scan attempt:
         → 403 "All sessions have been used. The subscription has no remaining sessions."
```

### Test 3 — Manual Entry + Exit

```
Step 1: Admin dashboard → search member by name/ID
         GET /api/members/{id} → get memberId

Step 2: Click "Mark Entry"
         POST /api/activity/entry-exit/manual/enter  { memberId: "..." }
         → 201 { sessionId: "...", checkInTime: "..." }

Step 3: Click "Mark Exit"
         POST /api/activity/entry-exit/manual/exit  { memberId: "..." }
         → 200 { sessionId: "...", checkOutTime: "..." }
```

---

## Endpoint Reference Card

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/activity/entry-exit/scan` | Admin | `{ "cardUid": "..." }` | `ScanEntryExitResponse` |
| `POST` | `/api/activity/entry-exit/manual/enter` | Admin | `{ "memberId": "guid" }` | `{ sessionId, checkInTime }` (201) |
| `POST` | `/api/activity/entry-exit/manual/exit` | Admin | `{ "memberId": "guid" }` | `{ sessionId, checkOutTime }` |
