# Programs (Courses) Service — Frontend Integration Guide

Base URL (dev): `http://localhost:5104`

The Programs API is part of the **Courses** service.  
All endpoints are routed through the YARP Gateway at `http://localhost:5098`.

## Authentication

Every request must include the JWT bearer token in the `Authorization` header:

```http
Authorization: Bearer <token>
```

> The token is obtained from the Identity service at `http://localhost:5051`.  
> Token format: RS256 JWT with claims `sub` (user UUID), `email`, `role` (Admin | Member | Coach).

### Auth Policies per Endpoint

| Policy | Allowed Roles |
|--------|---------------|
| `AdminOnly` | Admin |
| `MemberOnly` | Member |
| `CoachOnly` | Coach |
| `AdminOrCoach` | Admin, Coach |
| `Authenticated` | Any logged-in user |

---

## Table of Contents

1. [Setup](#1-setup)
2. [React — Coach Creates a Program](#2-react--coach-creates-a-program)
3. [React — Admin Reviews & Accepts/Rejects Programs](#3-react--admin-reviews--acceptsrejects-programs)
4. [React — Admin Views Pending Cash Enrollments](#4-react--admin-views-pending-cash-enrollments)
5. [Flutter — Member Browses & Enrolls in Programs](#5-flutter--member-browses--enrolls-in-programs)
6. [Flutter — Member Views Program Detail & Coach Profile](#6-flutter--member-views-program-detail--coach-profile)
7. [React — Coach Marks Attendance](#7-react--coach-marks-attendance)
8. [Flutter — Member Views Session History](#8-flutter--member-views-session-history)
9. [Error Handling](#9-error-handling)
10. [Endpoint Reference Card](#10-endpoint-reference-card)

---

## 1. Setup

```tsx
const API = "http://localhost:5098"; // Gateway URL

function authHeaders(): HeadersInit {
  const token = localStorage.getItem("jwt_token");
  return { Authorization: `Bearer ${token}` };
}
```

```dart
const String apiGateway = "http://localhost:5098";

Future<Map<String, String>> _authHeaders() async {
  final token = await storage.read(key: "jwt_token");
  return {"Authorization": "Bearer $token"};
}
```

---

## 2. React — Coach Creates a Program

**User action:** Coach opens the "Create Program" page → fills in program name, description, level, exercise type, duration, start/end dates, price, max participants, and one or more weekly time slots → optionally uploads a picture → clicks "Create".

### 2.1 Create Program

**Endpoint:** `POST /api/programs`

**Auth:** `CoachOnly`

**Request body (JSON):**

| Field | Type | Required | Example |
|-------|------|----------|---------|
| `name` | string | yes | "Morning HIIT" |
| `description` | string | no | "High intensity interval training for all levels" |
| `level` | string | no | "Intermediate" |
| `exerciseType` | string | no | "HIIT" |
| `durationMinutes` | int | yes | 45 |
| `startDate` | string (date) | yes | "2026-06-10" |
| `endDate` | string (date) | yes | "2026-08-10" |
| `totalPrice` | number | yes | 12000.00 |
| `maxParticipants` | int | yes | 20 |
| `pictureUrl` | string | no | "https://example.com/program.jpg" |
| `timeSlots` | array | yes | See below |

**Time slot schema:**
```json
{
  "day": "Monday",
  "startTime": "08:00",
  "endTime": "09:00",
  "description": "Morning session"
}
```

**Valid day values:** `Sunday`, `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`, `Saturday`

**Full request example:**
```json
{
  "name": "Morning HIIT",
  "description": "High intensity interval training for all levels",
  "level": "Intermediate",
  "exerciseType": "HIIT",
  "durationMinutes": 45,
  "startDate": "2026-06-10",
  "endDate": "2026-08-10",
  "totalPrice": 12000.00,
  "maxParticipants": 20,
  "timeSlots": [
    { "day": "Monday", "startTime": "08:00", "endTime": "09:00" },
    { "day": "Wednesday", "startTime": "08:00", "endTime": "09:00" },
    { "day": "Friday", "startTime": "08:00", "endTime": "09:00" }
  ]
}
```

**Response (200):**
```json
{
  "id": "prog-aaaa-..."
}
```

> The program is created with `Status = "Pending"`. It will only appear to members after an admin accepts it.

**React:**
```tsx
async function createProgram(data: {
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
  timeSlots: { day: string; startTime: string; endTime: string; description?: string }[];
}) {
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
  return res.json();
}
```

---

## 3. React — Admin Reviews & Accepts/Rejects Programs

### 3.1 Admin — View Pending Program Requests

**User action:** Admin opens the "Program Requests" dashboard → sees all pending programs with coach name and creation date.

**Endpoint:** `GET /api/programs/requests`

**Auth:** `AdminOnly`

**Response (200):**
```json
[
  {
    "programId": "prog-aaaa-...",
    "programName": "Morning HIIT",
    "description": "High intensity interval training for all levels",
    "coachName": "Ahmed Coach"
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
  return res.json(); // PendingProgramRequest[]
}
```

### 3.2 Admin — View Program Request Detail

**User action:** Admin clicks on a specific request → sees full details including time slots, price, capacity, dates.

**Endpoint:** `GET /api/programs/requests/{programId}`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "programId": "prog-aaaa-...",
  "programName": "Morning HIIT",
  "description": "...",
  "coachName": "Ahmed Coach",
  "status": "Pending",
  "startDate": "2026-06-10",
  "endDate": "2026-08-10",
  "totalPrice": 12000.00,
  "maxParticipants": 20,
  "createdAt": "2026-06-03T08:00:00Z",
  "timeSlots": [
    { "day": "Monday", "startTime": "08:00", "endTime": "09:00" },
    { "day": "Wednesday", "startTime": "08:00", "endTime": "09:00" },
    { "day": "Friday", "startTime": "08:00", "endTime": "09:00" }
  ]
}
```

**React:**
```tsx
async function getProgramRequestDetail(programId: string) {
  const res = await fetch(`${API}/api/programs/requests/${programId}`, {
    headers: authHeaders(),
  });
  if (!res.ok) {
    if (res.status === 404) return null;
    throw new Error(await res.text());
  }
  return res.json();
}
```

### 3.3 Admin — Accept Program (Sessions Auto-Generated)

**User action:** Admin reviews the program and taps "Accept". The system automatically generates individual sessions for each time slot between the start and end dates.

> ⚠️ **Important:** Before accepting, ensure `startDate >= today`. Sessions are generated for all dates between `startDate` and `endDate`, including past dates if the program started earlier.

**Endpoint:** `PATCH /api/programs/requests/{programId}/accept`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "programId": "prog-aaaa-...",
  "status": "Accepted"
}
```

**React:**
```tsx
async function acceptProgram(programId: string) {
  const res = await fetch(`${API}/api/programs/requests/${programId}/accept`, {
    method: "PATCH",
    headers: authHeaders(),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to accept program");
  }
  return res.json();
}
```

### 3.4 Admin — Reject Program

**User action:** Admin taps "Reject" on a program request.

**Endpoint:** `PATCH /api/programs/requests/{programId}/reject`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "programId": "prog-aaaa-...",
  "status": "Rejected"
}
```

**React:**
```tsx
async function rejectProgram(programId: string) {
  const res = await fetch(`${API}/api/programs/requests/${programId}/reject`, {
    method: "PATCH",
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error((await res.json()).detail || "Failed to reject program");
  return res.json();
}
```

---

## 4. React — Admin Views Pending Cash Enrollments

**User action:** Admin opens the "Pending Course Purchases" dashboard → sees all members who requested to enroll with cash.

**Endpoint:** `GET /api/programs/purchase/pending`

**Auth:** `AdminOnly`

**Response (200):**
```json
[
  {
    "id": "req-bbbb-...",
    "memberId": "a1b2c3d4-...",
    "programId": "prog-aaaa-...",
    "programName": "Morning HIIT",
    "coachName": "Ahmed Coach",
    "amount": 12000.00,
    "paymentMethod": "Cash",
    "status": "Pending",
    "createdAt": "2026-06-03T10:00:00Z",
    "notes": "Will bring cash tomorrow"
  }
]
```

**React:**
```tsx
async function fetchPendingPurchaseRequests() {
  const res = await fetch(`${API}/api/programs/purchase/pending`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // PendingPurchaseRequest[]
}
```

### 4.1 Admin — Accept Cash Enrollment

**User action:** Admin receives cash from the member → taps "Accept" → enrollment is created, payment recorded.

**Endpoint:** `PATCH /api/programs/purchase/{requestId}/accept`

**Auth:** `AdminOnly`

**Request body (optional):**
```json
"Optional note about the payment"
```

> The body is a raw string (not a JSON object).

**Response (200):**
```json
{
  "requestId": "req-bbbb-...",
  "enrollmentId": "enr-aaaa-...",
  "status": "Accepted"
}
```

**React:**
```tsx
async function acceptPurchaseRequest(requestId: string, notes?: string) {
  const res = await fetch(`${API}/api/programs/purchase/${requestId}/accept`, {
    method: "PATCH",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(notes ?? ""),
  });
  if (!res.ok) throw new Error((await res.json()).detail || "Failed to accept");
  return res.json();
}
```

### 4.2 Admin — Reject Cash Enrollment

**User action:** Admin taps "Reject" → optionally provides a reason.

**Endpoint:** `PATCH /api/programs/purchase/{requestId}/reject`

**Auth:** `AdminOnly`

**Request body (optional):**
```json
{
  "reason": "Program is full - no spots available"
}
```

**Response (200):**
```json
{
  "requestId": "req-bbbb-...",
  "status": "Rejected"
}
```

**React:**
```tsx
async function rejectPurchaseRequest(requestId: string, reason?: string) {
  const res = await fetch(`${API}/api/programs/purchase/${requestId}/reject`, {
    method: "PATCH",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ reason }),
  });
  if (!res.ok) throw new Error((await res.json()).detail || "Failed to reject");
  return res.json();
}
```

---

## 5. Flutter — Member Browses & Enrolls in Programs

### 5.1 Member — Get Their Profile (for MemberId)

Before enrolling, the frontend needs the member's **MemberId** (returned by the JWT `sub` claim or `GET /api/me`).

**Endpoint:** `GET /api/me`

**Auth:** `MemberOnly`

**Response (200):**
```json
{
  "memberId": "a1b2c3d4-...",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com"
}
```

**Flutter:**
```dart
Future<String> getMyMemberId() async {
  final uri = Uri.parse('$apiGateway/api/me');
  final response = await http.get(uri, headers: await _authHeaders());
  if (response.statusCode == 200) {
    final body = jsonDecode(response.body);
    return body['memberId'];
  }
  throw ApiException('Failed to get profile');
}
```

### 5.2 Member — View Available Programs

**User action:** Member opens the "Programs" tab → sees a list of programs that are accepted and have available spots.

**Endpoint:** `GET /api/programs/available`

**Auth:** `Authenticated` (any logged-in user)

**Response (200):**
```json
[
  {
    "id": "prog-aaaa-...",
    "name": "Morning HIIT",
    "imageUrl": "https://example.com/program.jpg",
    "price": 12000.00,
    "coachId": "coach-1111-...",
    "coachName": "Ahmed Coach",
    "description": "High intensity interval training for all levels"
  }
]
```

**Flutter:**
```dart
class AvailableProgram {
  final String id;
  final String name;
  final String? imageUrl;
  final double price;
  final String coachId;
  final String coachName;
  final String description;

  AvailableProgram.fromJson(Map<String, dynamic> json)
    : id = json['id'],
      name = json['name'],
      imageUrl = json['imageUrl'],
      price = (json['price'] as num).toDouble(),
      coachId = json['coachId'],
      coachName = json['coachName'],
      description = json['description'] ?? '';
}

Future<List<AvailableProgram>> getAvailablePrograms() async {
  final uri = Uri.parse('$apiGateway/api/programs/available');
  final response = await http.get(uri, headers: await _authHeaders());
  if (response.statusCode == 200) {
    final List<dynamic> body = jsonDecode(response.body);
    return body.map((e) => AvailableProgram.fromJson(e)).toList();
  }
  throw ApiException('Failed to load programs');
}
```

### 5.3 Member — Enroll Online (Auto-Accepted, Credit Card)

**User action:** Member taps "Enroll" on a program → selects "Pay Online" → the system processes the payment (simulated) and creates the enrollment immediately.

> Free programs (price = 0) also use this endpoint — payment is skipped automatically.

**Endpoint:** `POST /api/programs/{programId}/purchase/online`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "amount": 12000.00,
  "notes": "Enrolling in Morning HIIT"
}
```

**Response (200):**
```json
{
  "enrollmentId": "enr-aaaa-...",
  "programId": "prog-aaaa-...",
  "programName": "Morning HIIT",
  "paymentId": "pay-cccc-...",
  "amount": 12000.00,
  "paymentMethod": "CreditCard",
  "purchasedAt": "2026-06-03T12:00:00Z"
}
```

**Flutter:**
```dart
class OnlineEnrollmentResponse {
  final String enrollmentId;
  final String programId;
  final String programName;
  final String? paymentId;
  final double amount;
  final String paymentMethod;
  final DateTime purchasedAt;

  OnlineEnrollmentResponse.fromJson(Map<String, dynamic> json)
    : enrollmentId = json['enrollmentId'],
      programId = json['programId'],
      programName = json['programName'],
      paymentId = json['paymentId'],
      amount = (json['amount'] as num).toDouble(),
      paymentMethod = json['paymentMethod'],
      purchasedAt = DateTime.parse(json['purchasedAt']);
}

Future<OnlineEnrollmentResponse> enrollOnline({
  required String programId,
  required double amount,
  String? notes,
}) async {
  final uri = Uri.parse('$apiGateway/api/programs/$programId/purchase/online');
  final response = await http.post(
    uri,
    headers: {
      ...await _authHeaders(),
      'Content-Type': 'application/json',
    },
    body: jsonEncode({ 'amount': amount, 'notes': notes }),
  );
  if (response.statusCode == 200) {
    return OnlineEnrollmentResponse.fromJson(jsonDecode(response.body));
  }
  final error = jsonDecode(response.body);
  throw ApiException(error['detail'] ?? 'Enrollment failed');
}
```

### 5.4 Member — Submit Cash Enrollment Request

**User action:** Member taps "Enroll" → selects "Pay with Cash" → submits a request. Admin will later approve after receiving the cash.

**Endpoint:** `POST /api/programs/{programId}/purchase/cash`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "amount": 12000.00,
  "notes": "Will bring cash tomorrow"
}
```

**Response (200):**
```json
{
  "requestId": "req-bbbb-...",
  "status": "Pending"
}
```

**Flutter:**
```dart
Future<CashEnrollmentResponse> enrollCash({
  required String programId,
  required double amount,
  String? notes,
}) async {
  final uri = Uri.parse('$apiGateway/api/programs/$programId/purchase/cash');
  final response = await http.post(
    uri,
    headers: {
      ...await _authHeaders(),
      'Content-Type': 'application/json',
    },
    body: jsonEncode({ 'amount': amount, 'notes': notes }),
  );
  if (response.statusCode == 200) {
    return CashEnrollmentResponse.fromJson(jsonDecode(response.body));
  }
  final error = jsonDecode(response.body);
  throw ApiException(error['detail'] ?? 'Request failed');
}

class CashEnrollmentResponse {
  final String requestId;
  final String status;
  CashEnrollmentResponse.fromJson(Map<String, dynamic> json)
    : requestId = json['requestId'],
      status = json['status'];
}
```

### 5.5 Screen Example — Program List & Enrollment

```dart
class ProgramsScreen extends StatefulWidget {
  @override
  State<ProgramsScreen> createState() => _ProgramsScreenState();
}

class _ProgramsScreenState extends State<ProgramsScreen> {
  List<AvailableProgram>? _programs;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadPrograms();
  }

  Future<void> _loadPrograms() async {
    try {
      final programs = await getAvailablePrograms();
      setState(() { _programs = programs; _loading = false; });
    } catch (e) {
      setState(() => _loading = false);
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('$e')));
    }
  }

  Future<void> _enrollOnline(AvailableProgram program) async {
    try {
      final result = await enrollOnline(
        programId: program.id,
        amount: program.price,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Enrolled in ${result.programName}!')),
      );
    } on ApiException catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(e.message)));
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Center(child: CircularProgressIndicator());

    return ListView.builder(
      itemCount: _programs?.length ?? 0,
      itemBuilder: (ctx, i) {
        final program = _programs![i];
        return Card(
          child: ListTile(
            title: Text(program.name),
            subtitle: Text('${program.coachName} · ${program.price} DZD'),
            trailing: ElevatedButton(
              onPressed: () => _enrollOnline(program),
              child: const Text('Enroll'),
            ),
          ),
        );
      },
    );
  }
}
```

---

## 6. Flutter — Member Views Program Detail & Coach Profile

### 6.1 Program Detail

**User action:** Member taps on a program card → sees full detail including time slots, spots remaining, and coach info.

**Endpoint:** `GET /api/programs/{programId}`

**Auth:** `Authenticated`

**Response (200):**
```json
{
  "id": "prog-aaaa-...",
  "name": "Morning HIIT",
  "description": "High intensity interval training for all levels",
  "price": 12000.00,
  "spotsLeft": 18,
  "maxParticipants": 20,
  "coachId": "coach-1111-...",
  "coachName": "Ahmed Coach",
  "level": "Intermediate",
  "exerciseType": "HIIT",
  "durationMinutes": 45,
  "timeSlots": [
    { "day": "Monday", "startTime": "08:00", "endTime": "09:00" },
    { "day": "Wednesday", "startTime": "08:00", "endTime": "09:00" },
    { "day": "Friday", "startTime": "08:00", "endTime": "09:00" }
  ]
}
```

### 6.2 Coach Profile

**User action:** Member taps the coach name → sees public coach profile with bio and specialties.

**Endpoint:** `GET /api/coaches/{coachId}`

**Auth:** `Authenticated`

**Response (200):**
```json
{
  "coachId": "coach-1111-...",
  "userId": "user-aaaa-...",
  "firstName": "Ahmed",
  "lastName": "Coach",
  "bio": "Certified personal trainer with 5 years of experience",
  "specialties": "HIIT, Strength Training, Weight Loss",
  "profilePhotoUrl": "https://example.com/coach.jpg"
}
```

**Flutter:**
```dart
class CoachProfile {
  final String coachId;
  final String firstName;
  final String lastName;
  final String? bio;
  final String? specialties;
  final String? profilePhotoUrl;

  CoachProfile.fromJson(Map<String, dynamic> json)
    : coachId = json['coachId'],
      firstName = json['firstName'],
      lastName = json['lastName'],
      bio = json['bio'],
      specialties = json['specialties'],
      profilePhotoUrl = json['profilePhotoUrl'];
}

Future<CoachProfile> getCoachProfile(String coachId) async {
  final uri = Uri.parse('$apiGateway/api/coaches/$coachId');
  final response = await http.get(uri, headers: await _authHeaders());
  if (response.statusCode == 200) {
    return CoachProfile.fromJson(jsonDecode(response.body));
  }
  throw ApiException('Failed to load coach profile');
}
```

---

## 7. React — Coach Marks Attendance

### 7.1 View Program Sessions

**User action:** Coach opens a specific program → sees all generated sessions with dates, times, and enrolled members.

**Endpoint:** `GET /api/programs/{programId}/sessions`

**Auth:** `AdminOrCoach`

**Response (200):**
```json
{
  "sessions": [
    {
      "sessionId": "sess-aaaa-...",
      "date": "2026-06-10",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "isCompleted": false,
      "enrolledCount": 15,
      "members": [
        { "memberId": "a1b2c3d4-...", "fullName": "Member-a1b2c3d4", "attendanceStatus": null },
        { "memberId": "e5f6g7h8-...", "fullName": "Member-e5f6g7h8", "attendanceStatus": null }
      ]
    },
    {
      "sessionId": "sess-bbbb-...",
      "date": "2026-06-12",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "isCompleted": false,
      "enrolledCount": 15,
      "members": [ /* ... */ ]
    }
  ]
}
```

> For past sessions where attendance has been marked, `attendanceStatus` will be `"Present"`, `"Absent"`, or `"NotMarked"`.  
> For future sessions (not yet completed), `attendanceStatus` is `null` for all members.

**React:**
```tsx
async function getProgramSessions(programId: string) {
  const res = await fetch(`${API}/api/programs/${programId}/sessions`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // { sessions: SessionWithMembers[] }
}
```

### 7.2 Mark Attendance for a Session

**User action:** Coach selects a session → sees the list of enrolled members → marks each as "Present" or "Absent" → taps "Submit".

**Endpoint:** `POST /api/sessions/{sessionId}/attendance`

**Auth:** `AdminOrCoach`

**Request body (JSON):**
```json
{
  "attendance": [
    { "memberId": "a1b2c3d4-...", "status": "Present" },
    { "memberId": "e5f6g7h8-...", "status": "Present" },
    { "memberId": "i9j0k1l2-...", "status": "Absent" }
  ]
}
```

> Every enrolled member must be included in the list (all present and absent members).  
> `status` values: `"Present"` or `"Absent"`.

**Response (200):**
```json
{
  "sessionId": "sess-aaaa-...",
  "markedCount": 3
}
```

> Attendance can only be marked **once** per session. A second attempt returns `409 Conflict`.

**React:**
```tsx
async function markAttendance(sessionId: string, attendance: { memberId: string; status: string }[]) {
  const res = await fetch(`${API}/api/sessions/${sessionId}/attendance`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ attendance }),
  });
  if (!res.ok) {
    const error = await res.json();
    if (res.status === 409) {
      throw new Error("Attendance already marked for this session");
    }
    throw new Error(error.detail || "Failed to mark attendance");
  }
  return res.json();
}
```

### 7.3 Attendance Marking Screen Example

```tsx
function AttendanceScreen({ programId }: { programId: string }) {
  const [sessions, setSessions] = useState<SessionWithMembers[]>([]);
  const [selectedSession, setSelectedSession] = useState<string | null>(null);
  const [attendance, setAttendance] = useState<Record<string, string>>({});

  useEffect(() => {
    fetchSessions(programId).then(setSessions);
  }, [programId]);

  async function handleSubmit() {
    if (!selectedSession) return;
    const entries = Object.entries(attendance).map(([memberId, status]) => ({
      memberId, status,
    }));
    try {
      await markAttendance(selectedSession, entries);
      alert("Attendance saved!");
      // Refresh sessions to update completion status
      fetchSessions(programId).then(setSessions);
    } catch (e: any) {
      alert(e.message);
    }
  }

  // Get members for the selected session
  const currentSession = sessions.find(s => s.sessionId === selectedSession);
  const enrolledMembers = currentSession?.members ?? [];

  return (
    <div>
      <h2>Select Session</h2>
      <select onChange={e => {
        setSelectedSession(e.target.value);
        setAttendance({});
      }}>
        <option value="">-- Select --</option>
        {sessions.filter(s => !s.isCompleted).map(s => (
          <option key={s.sessionId} value={s.sessionId}>
            {s.date.toString()} {s.startTime} - {s.endTime}
          </option>
        ))}
      </select>

      {selectedSession && (
        <>
          <h3>Mark Attendance</h3>
          {enrolledMembers.map(m => (
            <div key={m.memberId}>
              <span>{m.fullName}</span>
              <select
                value={attendance[m.memberId] ?? ""}
                onChange={e => setAttendance(prev => ({
                  ...prev, [m.memberId]: e.target.value,
                }))}
              >
                <option value="">--</option>
                <option value="Present">Present</option>
                <option value="Absent">Absent</option>
              </select>
            </div>
          ))}
          <button onClick={handleSubmit}>Submit Attendance</button>
        </>
      )}
    </div>
  );
}
```

---

## 8. Flutter — Member Views Session History

**User action:** Member opens "My Sessions" → sees all past and future sessions across all enrolled programs, filtered by date range.

**Endpoint:** `GET /api/sessions/history?startDate=2026-01-01T00:00:00Z&endDate=2026-12-31T23:59:59Z`

**Auth:** `MemberOnly`

**Response (200):**
```json
{
  "sessions": [
    {
      "sessionId": "sess-aaaa-...",
      "date": "2026-06-10",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "isCompleted": true,
      "programId": "prog-aaaa-...",
      "programName": "Morning HIIT",
      "attendanceStatus": "Present"
    }
  ]
}
```

**`attendanceStatus` values:** `"Present"` | `"Absent"` | `"NotMarked"`

**Flutter:**
```dart
class SessionHistoryItem {
  final String sessionId;
  final String date;
  final String startTime;
  final String endTime;
  final bool isCompleted;
  final String programName;
  final String attendanceStatus;

  SessionHistoryItem.fromJson(Map<String, dynamic> json)
    : sessionId = json['sessionId'],
      date = json['date'],
      startTime = json['startTime'],
      endTime = json['endTime'],
      isCompleted = json['isCompleted'],
      programName = json['programName'],
      attendanceStatus = json['attendanceStatus'];
}

Future<List<SessionHistoryItem>> getSessionHistory({
  required DateTime startDate,
  required DateTime endDate,
}) async {
  final uri = Uri.parse('$apiGateway/api/sessions/history').replace(
    queryParameters: {
      'startDate': startDate.toUtc().toIso8601String(),
      'endDate': endDate.toUtc().toIso8601String(),
    },
  );
  final response = await http.get(uri, headers: await _authHeaders());
  if (response.statusCode == 200) {
    final body = jsonDecode(response.body);
    final List<dynamic> sessions = body['sessions'];
    return sessions.map((e) => SessionHistoryItem.fromJson(e)).toList();
  }
  throw ApiException('Failed to load session history');
}
```

---

## 9. Error Handling

All error responses follow the RFC 9457 Problem Details format:

```json
{
  "type": "about:blank",
  "title": "Conflict",
  "status": 409,
  "detail": "Attendance has already been marked for this session."
}
```

| HTTP Status | Meaning | Common Causes |
|------------|---------|---------------|
| **200** | Success | GET, POST, PATCH |
| **400** | Bad Request | Invalid amount, missing fields |
| **401** | Unauthorized | Missing or expired JWT |
| **403** | Forbidden | Wrong role for the action |
| **404** | Not Found | Program/session/request doesn't exist |
| **409** | Conflict | Already enrolled, already marked, already reviewed |
| **500** | Server Error | Backend exception |

**React generic error handler:**
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

**Flutter generic error:**
```dart
class ApiException implements Exception {
  final String message;
  final int? statusCode;
  ApiException(this.message, {this.statusCode});
  @override
  String toString() => message;
}
```

---

## 10. Endpoint Reference Card

### Program Lifecycle (Coach → Admin → Members)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/programs` | Coach | Program JSON with time slots | `{ id }` |
| `GET` | `/api/programs/requests` | Admin | — | `PendingRequest[]` |
| `GET` | `/api/programs/requests/{id}` | Admin | — | `RequestDetail` |
| `PATCH` | `/api/programs/requests/{id}/accept` | Admin | — | `{ programId, status }` |
| `PATCH` | `/api/programs/requests/{id}/reject` | Admin | — | `{ programId, status }` |

### Member Browsing

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `GET` | `/api/programs/available` | Any auth | — | `AvailableProgram[]` |
| `GET` | `/api/programs/{id}` | Any auth | — | `ProgramDetail` |
| `GET` | `/api/programs/enrolled` | Member | — | `EnrolledProgram[]` |
| `GET` | `/api/coaches/{id}` | Any auth | — | `CoachProfile` |
| `GET` | `/api/me` | Member | — | `MyProfile` (includes `memberId`) |

### Enrollment (Online & Cash)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/programs/{id}/purchase/online` | Member | `{ amount, notes? }` | `OnlineEnrollmentResponse` |
| `POST` | `/api/programs/{id}/purchase/cash` | Member | `{ amount, notes? }` | `{ requestId, status }` |
| `GET` | `/api/programs/purchase/pending` | Admin | — | `PendingPurchase[]` |
| `PATCH` | `/api/programs/purchase/{id}/accept` | Admin | `"notes string"` | `{ requestId, enrollmentId, status }` |
| `PATCH` | `/api/programs/purchase/{id}/reject` | Admin | `{ reason? }` | `{ requestId, status }` |

### Attendance

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `GET` | `/api/programs/{id}/sessions` | Admin/Coach | — | `{ sessions: SessionWithMembers[] }` |
| `POST` | `/api/sessions/{id}/attendance` | Admin/Coach | `{ attendance: [{ memberId, status }] }` | `{ sessionId, markedCount }` |
| `GET` | `/api/sessions/history` | Member | `?startDate=&endDate=` | `{ sessions: SessionHistoryItem[] }` |

---

### Quick Status Values Reference

**Program status:** `Pending` | `Accepted` | `Rejected`

**Attendance status:** `Present` | `Absent`

**Attendance marking result:** `Present` | `Absent` | `NotMarked` | `null` (future session)

**CoursePurchaseRequest status:** `Pending` | `Accepted` | `Rejected`

**Roles:** `Admin` | `Member` | `Coach` (from JWT role claim)
