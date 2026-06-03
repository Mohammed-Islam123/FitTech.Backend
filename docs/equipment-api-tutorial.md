# Equipment Service — Frontend Integration Guide

Base URL (dev): `http://localhost:5106`

All endpoints except `POST /api/issues` require the **Admin** role (`ROLE_Admin`).  
`POST /api/issues` requires any authenticated user (Member, Coach, or Admin).

## Authentication

Every request must include the JWT bearer token in the `Authorization` header:

```http
Authorization: Bearer <token>
```

> The token is obtained from the Identity service at `http://localhost:5051`.  
> Token format: RS256 JWT with claims `sub` (user UUID), `email`, `role` (Admin | Member | Coach).

---

## Table of Contents

1. [React — Equipment CRUD](#1-react--equipment-crud-admin-only)
2. [React — Issue Management](#2-react--issue-management-admin-only)
3. [Flutter — Report an Issue](#3-flutter--report-an-issue-any-authenticated-user)
4. [Error Handling](#4-error-handling)
5. [Image URLs](#5-image-urls)

---

## 1. React — Equipment CRUD (Admin only)

### Setup

```tsx
const API = "http://localhost:5106";

function authHeaders(): HeadersInit {
  const token = localStorage.getItem("jwt_token");
  return { Authorization: `Bearer ${token}` };
}
```

---

### 1.1 List All Equipment

**User action:** Admin opens the equipment management page.

**Endpoint:** `GET /api/equipments`

**Response (200):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Treadmill Pro X1",
    "description": "High-speed treadmill with incline settings",
    "category": "cardio",
    "status": "available",
    "imageUrl": "/uploads/550e8400-e29b-41d4-a716-446655440000.jpg",
    "createdAt": "2026-06-02T10:30:00Z",
    "updatedAt": null
  }
]
```

**React:**
```tsx
async function fetchEquipment() {
  const res = await fetch(`${API}/api/equipments`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // EquipmentResponse[]
}
```

**Display hint:** Use `imageUrl` prefixed with `API` as the `<img>` source:

```tsx
<img src={`${API}${item.imageUrl}`} alt={item.name} />
```

---

### 1.2 Get Equipment by ID

**User action:** Admin clicks on a specific equipment row to view details.

**Endpoint:** `GET /api/equipments/{id}`

**Response (200):** Same shape as a single list item above.

**Response (404):**
```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "Equipment not found"
}
```

**React:**
```tsx
async function getEquipment(id: string) {
  const res = await fetch(`${API}/api/equipments/${id}`, {
    headers: authHeaders(),
  });
  if (res.status === 404) {
    // Show "Equipment not found" to user
    return null;
  }
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

---

### 1.3 Create Equipment (with Image Upload)

**User action:** Admin clicks "Add Equipment" → fills in name, description, category, status → selects an image file → clicks "Save".

**Endpoint:** `POST /api/equipments`

**Request:** `multipart/form-data`

| Field        | Type   | Required | Example          |
|-------------|--------|----------|------------------|
| `name`       | string | yes      | "Treadmill Pro X1" |
| `description`| string | no       | "High-speed treadmill" |
| `category`   | string | no       | "cardio" |
| `status`     | string | no       | "available" (default) |
| `image`      | file   | no       | .jpg, .png, .webp |

**Status values:** `available`, `broken`, `maintenance`

**Category examples:** `cardio`, `strength`, `free_weights`, `cables`, `functional`

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Treadmill Pro X1",
  "description": "High-speed treadmill with incline settings",
  "category": "cardio",
  "status": "available",
  "imageUrl": "/uploads/550e8400-e29b-41d4-a716-446655440000.jpg",
  "createdAt": "2026-06-02T10:30:00Z",
  "updatedAt": null
}
```

**React (using `fetch` with `FormData`):**
```tsx
async function createEquipment(data: {
  name: string;
  description?: string;
  category?: string;
  status?: string;
  image?: File;
}) {
  const formData = new FormData();
  formData.append("name", data.name);
  if (data.description) formData.append("description", data.description);
  if (data.category)     formData.append("category", data.category);
  if (data.status)       formData.append("status", data.status);
  if (data.image)        formData.append("image", data.image);

  const res = await fetch(`${API}/api/equipments`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${localStorage.getItem("jwt_token")}`,
      // Do NOT set Content-Type — browser sets it automatically with boundary
    },
    body: formData,
  });

  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to create equipment");
  }

  return res.json();
}
```

> ⚠️ **Crucial:** Do NOT set `Content-Type` header manually when sending `FormData`. The browser must set it with the correct multipart boundary.

**File input component:**
```tsx
function EquipmentForm() {
  const [image, setImage] = useState<File | null>(null);

  return (
    <form onSubmit={handleSubmit}>
      <input name="name" placeholder="Name" required />
      <input name="description" placeholder="Description" />
      <select name="category">
        <option value="cardio">Cardio</option>
        <option value="strength">Strength</option>
        <option value="free_weights">Free Weights</option>
        <option value="cables">Cables</option>
        <option value="functional">Functional</option>
      </select>
      <select name="status">
        <option value="available">Available</option>
        <option value="broken">Broken</option>
        <option value="maintenance">Maintenance</option>
      </select>
      <input type="file" accept="image/*" onChange={e => setImage(e.target.files?.[0] ?? null)} />
      <button type="submit">Save</button>
    </form>
  );
}
```

---

### 1.4 Update Equipment

**User action:** Admin clicks "Edit" on an equipment → modifies fields → optionally changes image → clicks "Save".

**Endpoint:** `PUT /api/equipments/{id}`

**Request:** `multipart/form-data` (all fields optional — only send what changed)

| Field        | Type   | Required | Example |
|-------------|--------|----------|---------|
| `name`       | string | no       | "Treadmill Pro X2" |
| `description`| string | no       | "Updated description" |
| `category`   | string | no       | "strength" |
| `status`     | string | no       | "maintenance" |
| `image`      | file   | no       | new image |

**Response (200):** Updated `EquipmentResponse` JSON.

**React:**
```tsx
async function updateEquipment(id: string, data: {
  name?: string;
  description?: string;
  category?: string;
  status?: string;
  image?: File;
}) {
  const formData = new FormData();
  if (data.name)        formData.append("name", data.name);
  if (data.description) formData.append("description", data.description);
  if (data.category)    formData.append("category", data.category);
  if (data.status)      formData.append("status", data.status);
  if (data.image)       formData.append("image", data.image);

  const res = await fetch(`${API}/api/equipments/${id}`, {
    method: "PUT",
    headers: { Authorization: `Bearer ${localStorage.getItem("jwt_token")}` },
    body: formData,
  });

  if (!res.ok) throw new Error((await res.json()).detail);
  return res.json();
}
```

---

### 1.5 Delete Equipment (Soft Delete)

**User action:** Admin clicks "Delete" → confirmation dialog → confirms.

**Endpoint:** `DELETE /api/equipments/{id}`

**Response (204):** No content. The equipment is marked as inactive (soft delete) — it will no longer appear in the list but remains in the database.

**React:**
```tsx
async function deleteEquipment(id: string) {
  const confirmed = window.confirm(
    "Are you sure you want to delete this equipment?"
  );
  if (!confirmed) return;

  const res = await fetch(`${API}/api/equipments/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });

  if (res.status === 204) {
    // Success — remove from UI list
    return;
  }
  if (res.status === 404) {
    alert("Equipment not found — it may have been already deleted.");
    return;
  }
  throw new Error(await res.text());
}
```

---

## 2. React — Issue Management (Admin only)

### 2.1 List All Issues

**User action:** Admin opens the issues/maintenance dashboard.

**Endpoint:** `GET /api/issues`

**Response (200):**
```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "equipmentId": "550e8400-e29b-41d4-a716-446655440000",
    "reporterId": "770e8400-e29b-41d4-a716-446655440002",
    "description": "Treadmill belt is slipping at high speed",
    "status": "open",
    "createdAt": "2026-06-02T14:00:00Z",
    "updatedAt": null
  }
]
```

**React:**
```tsx
async function fetchIssues() {
  const res = await fetch(`${API}/api/issues`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // IssueResponse[]
}
```

**Status values:** `open`, `in_progress`, `resolved`

**UI suggestion:** Color-code status badges:
- `open` → 🔴 red
- `in_progress` → 🟡 amber  
- `resolved` → 🟢 green

---

### 2.2 Update Issue Status

**User action:** Admin reviews an issue → changes its status (e.g., marks as "in_progress" after assigning a technician, or "resolved" after repair).

**Endpoint:** `PATCH /api/issues/{id}/status`

**Request body (JSON):**
```json
{
  "status": "in_progress"
}
```

**Valid status values:** `open`, `in_progress`, `resolved`

**Response (200):** Updated `IssueResponse` JSON.

**React:**
```tsx
async function updateIssueStatus(issueId: string, newStatus: string) {
  const res = await fetch(`${API}/api/issues/${issueId}/status`, {
    method: "PATCH",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ status: newStatus }),
  });

  if (!res.ok) throw new Error((await res.json()).detail);
  return res.json();
}
```

**Usage example with a dropdown:**
```tsx
function IssueRow({ issue }: { issue: IssueResponse }) {
  const [status, setStatus] = useState(issue.status);

  async function handleStatusChange(newStatus: string) {
    const updated = await updateIssueStatus(issue.id, newStatus);
    setStatus(updated.status);
  }

  return (
    <tr>
      <td>{issue.equipmentId}</td>
      <td>{issue.description}</td>
      <td>
        <select value={status} onChange={e => handleStatusChange(e.target.value)}>
          <option value="open">Open</option>
          <option value="in_progress">In Progress</option>
          <option value="resolved">Resolved</option>
        </select>
      </td>
      <td>{new Date(issue.createdAt).toLocaleString()}</td>
    </tr>
  );
}
```

---

## 3. Flutter — Report an Issue (any authenticated user)

**User action:** A member or coach taps "Report Issue" → selects equipment from a list → types a description → taps "Submit".

### Setup

```dart
const String apiBase = "http://localhost:5106";

Future<Map<String, String>> _authHeaders() async {
  final token = await storage.read(key: "jwt_token");
  return {
    "Authorization": "Bearer $token",
    "Content-Type": "application/json",
  };
}
```

> Token is obtained from the Identity service login flow.  
> Store it securely (e.g., `flutter_secure_storage`).

---

### Report an Issue

**Endpoint:** `POST /api/issues`

**Request body (JSON):**
```json
{
  "equipmentId": "550e8400-e29b-41d4-a716-446655440000",
  "description": "The treadmill belt is making a loud noise"
}
```

**Response (201):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "equipmentId": "550e8400-e29b-41d4-a716-446655440000",
  "reporterId": "770e8400-e29b-41d4-a716-446655440002",
  "description": "The treadmill belt is making a loud noise",
  "status": "open",
  "createdAt": "2026-06-02T14:00:00Z",
  "updatedAt": null
}
```

**Flutter:**
```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

class IssueService {
  final String baseUrl;
  final String Function() getToken; // returns JWT from secure storage

  IssueService({required this.baseUrl, required this.getToken});

  /// Report an issue for a specific equipment.
  /// Returns the created [IssueResponse].
  Future<IssueResponse> reportIssue({
    required String equipmentId,
    required String description,
  }) async {
    final uri = Uri.parse('$baseUrl/api/issues');
    final response = await http.post(
      uri,
      headers: {
        'Authorization': 'Bearer ${getToken()}',
        'Content-Type': 'application/json',
      },
      body: jsonEncode({
        'equipmentId': equipmentId,
        'description': description,
      }),
    );

    if (response.statusCode == 201) {
      return IssueResponse.fromJson(jsonDecode(response.body));
    }

    // Parse error as ProblemDetail
    final error = jsonDecode(response.body);
    throw IssueException(error['detail'] ?? 'Failed to report issue');
  }
}
```

**Data model:**
```dart
class IssueResponse {
  final String id;
  final String equipmentId;
  final String reporterId;
  final String description;
  final String status;
  final DateTime createdAt;
  final DateTime? updatedAt;

  IssueResponse({
    required this.id,
    required this.equipmentId,
    required this.reporterId,
    required this.description,
    required this.status,
    required this.createdAt,
    this.updatedAt,
  });

  factory IssueResponse.fromJson(Map<String, dynamic> json) {
    return IssueResponse(
      id: json['id'],
      equipmentId: json['equipmentId'],
      reporterId: json['reporterId'],
      description: json['description'],
      status: json['status'],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: json['updatedAt'] != null ? DateTime.parse(json['updatedAt']) : null,
    );
  }
}

class IssueException implements Exception {
  final String message;
  IssueException(this.message);
  @override
  String toString() => message;
}
```

**Screen example:**
```dart
class ReportIssueScreen extends StatefulWidget {
  final List<EquipmentSummary> equipmentList;

  @override
  State<ReportIssueScreen> createState() => _ReportIssueScreenState();
}

class _ReportIssueScreenState extends State<ReportIssueScreen> {
  final _formKey = GlobalKey<FormState>();
  String? _selectedEquipmentId;
  final _descriptionController = TextEditingController();
  bool _isSubmitting = false;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSubmitting = true);
    try {
      final service = IssueService(
        baseUrl: "http://localhost:5106",
        getToken: () => storage.read(key: "jwt_token") ?? "",
      );

      await service.reportIssue(
        equipmentId: _selectedEquipmentId!,
        description: _descriptionController.text.trim(),
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Issue reported successfully")),
      );
      Navigator.pop(context, true); // return true = issue created
    } on IssueException catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.message)),
      );
    } finally {
      setState(() => _isSubmitting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Report an Issue")),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            DropdownButtonFormField<String>(
              decoration: const InputDecoration(labelText: "Equipment"),
              items: widget.equipmentList.map((e) {
                return DropdownMenuItem(value: e.id, child: Text(e.name));
              }).toList(),
              onChanged: (v) => _selectedEquipmentId = v,
              validator: (v) => v == null ? "Select equipment" : null,
            ),
            TextFormField(
              controller: _descriptionController,
              decoration: const InputDecoration(
                labelText: "Description",
                hintText: "Describe the issue...",
              ),
              maxLines: 4,
              validator: (v) => v == null || v.trim().isEmpty
                  ? "Description is required"
                  : null,
            ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: _isSubmitting ? null : _submit,
              child: _isSubmitting
                  ? const CircularProgressIndicator()
                  : const Text("Submit"),
            ),
          ],
        ),
      ),
    );
  }
}
```

**Pre-requisite for the screen:** Fetch the equipment list first so the user can pick which equipment has the issue:

```dart
Future<List<EquipmentSummary>> fetchEquipmentList() async {
  final token = await storage.read(key: "jwt_token");
  final res = await http.get(
    Uri.parse("http://localhost:5106/api/equipments"),
    headers: {"Authorization": "Bearer $token"},
  );
  if (res.statusCode == 200) {
    final List<dynamic> body = jsonDecode(res.body);
    return body.map((e) => EquipmentSummary(
      id: e['id'],
      name: e['name'],
    )).toList();
  }
  throw Exception("Failed to load equipment");
}

class EquipmentSummary {
  final String id;
  final String name;
  EquipmentSummary({required this.id, required this.name});
}
```

---

## 4. Error Handling

All error responses follow the RFC 9457 Problem Details format:

```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "Equipment not found"
}
```

| HTTP Status | Meaning | Common Causes |
|------------|---------|---------------|
| **200** | Success | GET, PUT, PATCH |
| **201** | Created | POST (equipment, issue) |
| **204** | No Content | DELETE |
| **400** | Bad Request | Invalid JSON, missing required field, wrong format |
| **401** | Unauthorized | Missing or expired JWT |
| **403** | Forbidden | Valid JWT but missing Admin role |
| **404** | Not Found | Equipment/issue ID doesn't exist or is soft-deleted |
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

---

## 5. Image URLs

The `imageUrl` field contains a **relative path** like `/uploads/uuid.jpg`.

To display the image, prepend the API base URL:

**React:**
```tsx
<img src={`http://localhost:5106${item.imageUrl}`} alt={item.name} />
```

**Flutter:**
```dart
Image.network('http://localhost:5106${equipment.imageUrl}')
```

The images are served as **static files** directly from the Java service's file system — no additional endpoint needed.

---

## Endpoint Reference Card

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `GET` | `/api/equipments` | Admin | — | `EquipmentResponse[]` |
| `GET` | `/api/equipments/{id}` | Admin | — | `EquipmentResponse` |
| `POST` | `/api/equipments` | Admin | multipart form | `EquipmentResponse` (201) |
| `PUT` | `/api/equipments/{id}` | Admin | multipart form | `EquipmentResponse` |
| `DELETE` | `/api/equipments/{id}` | Admin | — | (204) |
| `GET` | `/api/issues` | Admin | — | `IssueResponse[]` |
| `PATCH` | `/api/issues/{id}/status` | Admin | `{ "status": "..." }` | `IssueResponse` |
| `POST` | `/api/issues` | Any auth | `{ "equipmentId": "...", "description": "..." }` | `IssueResponse` (201) |

---

### Quick Status Values Reference

**Equipment status:** `available` | `broken` | `maintenance`

**Issue status:** `open` | `in_progress` | `resolved`

**Roles:** `Admin` | `Member` | `Coach` (from JWT role claim)
