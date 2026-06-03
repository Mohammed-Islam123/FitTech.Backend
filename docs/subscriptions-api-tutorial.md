# Subscriptions Service — Frontend Integration Guide

Base URL (dev): `http://localhost:5103`

The Subscriptions API is part of the **Membership** service.  
All subscription endpoints are routed through the YARP Gateway at `http://localhost:5098`.

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
| `Authenticated` | Any logged-in user |

---

## Table of Contents

1. [Setup](#1-setup)
2. [React — Member Submits Cash Renewal Request](#2-react--member-submits-cash-renewal-request)
3. [React — Admin Reviews & Accepts/Rejects Cash Renewals](#3-react--admin-reviews--acceptsrejects-cash-renewals)
4. [Flutter — Member Submits Online Renewal](#4-flutter--member-submits-online-renewal-auto-accepted)
5. [Flutter — Member Views Subscription History](#5-flutter--member-views-subscription-history)
6. [React — Member Browses Plans & Buys with Cash](#6-react--member-browses-plans--buys-with-cash)
7. [React — Admin Reviews & Accepts/Rejects Cash Purchases](#7-react--admin-reviews--acceptsrejects-cash-purchases)
8. [Flutter — Member Buys a New Plan Online](#8-flutter--member-buys-a-new-plan-online-auto-accepted)
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

## 2. React — Member Submits Cash Renewal Request

**User action:** Member's subscription has expired. They go to the subscription page, see "Renew with Cash" button, tap it, confirm the amount, and submit.

**Backend flow:** Creates a `PaymentApprovalRequest` with `Status=Pending`. Admin will later view and accept/reject it.

### 2.1 Member — Get Own Profile (to get MemberId)

Before submitting, the frontend needs the member's **MemberId** (returned by `GET /api/me`).

**Endpoint:** `GET /api/me`

**Auth:** `MemberOnly`

**Response (200) — relevant fields:**
```json
{
  "memberId": "a1b2c3d4-...",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com"
}
```

**React:**
```tsx
async function getMyProfile() {
  const res = await fetch(`${API}/api/me`, { headers: authHeaders() });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

### 2.2 Member — View Their Subscriptions

**User action:** Member opens the subscriptions page to see which subscription can be renewed.

**Endpoint:** `GET /api/me/subscriptions`

**Auth:** `MemberOnly`

**Response (200):**
```json
[
  {
    "id": "sub-1111-...",
    "planName": "Monthly Premium",
    "planPrice": 5000,
    "status": "Expired",
    "startDate": "2026-01-01T00:00:00Z",
    "endDate": "2026-02-01T00:00:00Z",
    "remainingSessions": null,
    "paymentStatus": "Paid",
    "autoRenew": false
  }
]
```

**React:**
```tsx
async function getMySubscriptions() {
  const res = await fetch(`${API}/api/me/subscriptions`, { headers: authHeaders() });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // MeSubscriptionResponse[]
}
```

### 2.3 Member — Submit Cash Renewal Request

**User action:** Member taps "Renew with Cash" on an expired subscription → a confirmation dialog shows the plan price → taps "Submit Request".

**Endpoint:** `POST /api/subscriptions/renew`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "subscriptionId": "sub-1111-...",
  "amount": 5000,
  "notes": "Will pay at front desk tomorrow"
}
```

> `amount` must equal the plan's price (validated server-side).

**Response (200):**
```json
{
  "requestId": "req-aaaa-..."
}
```

**React:**
```tsx
async function submitCashRenewal(subscriptionId: string, amount: number, notes?: string) {
  const res = await fetch(`${API}/api/subscriptions/renew`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ subscriptionId, amount, notes }),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to submit renewal request");
  }
  return res.json();
}
```

**Flutter:**
```dart
Future<RequestRenewalResponse> submitCashRenewal({
  required String subscriptionId,
  required double amount,
  String? notes,
}) async {
  final uri = Uri.parse('$apiGateway/api/subscriptions/renew');
  final response = await http.post(
    uri,
    headers: {
      ...await _authHeaders(),
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'subscriptionId': subscriptionId,
      'amount': amount,
      'notes': notes,
    }),
  );
  if (response.statusCode == 200) {
    return RequestRenewalResponse.fromJson(jsonDecode(response.body));
  }
  final error = jsonDecode(response.body);
  throw ApiException(error['detail'] ?? 'Failed to submit request');
}
```

---

## 3. React — Admin Reviews & Accepts/Rejects Cash Renewals

### 3.1 Admin — View Pending Renewal Requests

**User action:** Admin opens the "Pending Renewals" dashboard → sees a list of member requests with amounts and dates.

**Endpoint:** `GET /api/subscriptions/renew/pending`

**Auth:** `AdminOnly`

**Response (200):**
```json
[
  {
    "requestId": "req-aaaa-...",
    "memberId": "a1b2c3d4-...",
    "memberName": "John Doe",
    "planName": "Monthly Premium",
    "amount": 5000,
    "status": "Pending",
    "createdAt": "2026-06-02T10:00:00Z",
    "notes": "Will pay at front desk tomorrow"
  }
]
```

**React:**
```tsx
async function fetchPendingRenewals() {
  const res = await fetch(`${API}/api/subscriptions/renew/pending`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // PendingRenewalRequest[]
}
```

### 3.2 Admin — Accept a Cash Renewal

**User action:** Admin receives cash from the member → taps "Accept" on the request → the system creates a payment record and activates the subscription.

**Endpoint:** `PATCH /api/subscriptions/renew/{requestId}/accept`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "requestId": "req-aaaa-...",
  "paymentId": "pay-aaaa-...",
  "status": "Accepted"
}
```

**React:**
```tsx
async function acceptCashRenewal(requestId: string) {
  const res = await fetch(`${API}/api/subscriptions/renew/${requestId}/accept`, {
    method: "PATCH",
    headers: authHeaders(),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to accept renewal");
  }
  return res.json();
}
```

### 3.3 Admin — Reject a Cash Renewal

**User action:** Admin decides to reject the request (e.g., member has outstanding issues) → taps "Reject" → optionally provides a reason.

**Endpoint:** `PATCH /api/subscriptions/renew/{requestId}/reject`

**Auth:** `AdminOnly`

**Request body (JSON) — optional:**
```json
{
  "reason": "Payment method not accepted for frozen accounts"
}
```

**Response (200):**
```json
{
  "requestId": "req-aaaa-...",
  "status": "Rejected"
}
```

**React:**
```tsx
async function rejectCashRenewal(requestId: string, reason?: string) {
  const res = await fetch(`${API}/api/subscriptions/renew/${requestId}/reject`, {
    method: "PATCH",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ reason }),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to reject renewal");
  }
  return res.json();
}
```

---

## 4. Flutter — Member Submits Online Renewal (Auto-Accepted)

**User action:** Member taps "Renew Online" on an expired subscription → confirms payment amount → the system processes the payment (simulated) and activates the subscription immediately.

### 4.1 Member — Renew Online

**Endpoint:** `POST /api/subscriptions/renew/online`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "subscriptionId": "sub-1111-...",
  "amount": 5000,
  "notes": null
}
```

**Response (200):**
```json
{
  "subscriptionId": "sub-2222-...",
  "paymentId": "pay-bbbb-...",
  "planName": "Monthly Premium",
  "amount": 5000,
  "paymentMethod": "CreditCard",
  "renewedAt": "2026-06-02T12:00:00Z",
  "validUntil": "2026-07-02T00:00:00Z"
}
```

**Flutter:**
```dart
class OnlineRenewalResponse {
  final String subscriptionId;
  final String paymentId;
  final String planName;
  final double amount;
  final String paymentMethod;
  final DateTime renewedAt;
  final DateTime? validUntil;

  OnlineRenewalResponse({
    required this.subscriptionId,
    required this.paymentId,
    required this.planName,
    required this.amount,
    required this.paymentMethod,
    required this.renewedAt,
    this.validUntil,
  });

  factory OnlineRenewalResponse.fromJson(Map<String, dynamic> json) {
    return OnlineRenewalResponse(
      subscriptionId: json['subscriptionId'],
      paymentId: json['paymentId'],
      planName: json['planName'],
      amount: (json['amount'] as num).toDouble(),
      paymentMethod: json['paymentMethod'],
      renewedAt: DateTime.parse(json['renewedAt']),
      validUntil: json['validUntil'] != null
          ? DateTime.parse(json['validUntil'])
          : null,
    );
  }
}

Future<OnlineRenewalResponse> renewOnline({
  required String subscriptionId,
  required double amount,
  String? notes,
}) async {
  final uri = Uri.parse('$apiGateway/api/subscriptions/renew/online');
  final response = await http.post(
    uri,
    headers: {
      ...await _authHeaders(),
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'subscriptionId': subscriptionId,
      'amount': amount,
      'notes': notes,
    }),
  );

  if (response.statusCode == 200) {
    return OnlineRenewalResponse.fromJson(jsonDecode(response.body));
  }

  final error = jsonDecode(response.body);
  throw ApiException(error['detail'] ?? 'Renewal failed');
}
```

### 4.2 Screen Example

```dart
class RenewSubscriptionScreen extends StatefulWidget {
  final List<SubscriptionItem> expiredSubscriptions;

  @override
  State<RenewSubscriptionScreen> createState() =>
      _RenewSubscriptionScreenState();
}

class _RenewSubscriptionScreenState extends State<RenewSubscriptionScreen> {
  bool _isProcessing = false;

  Future<void> _renewOnline(SubscriptionItem sub) async {
    setState(() => _isProcessing = true);
    try {
      final response = await renewOnline(
        subscriptionId: sub.id,
        amount: sub.planPrice,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            '${sub.planName} renewed! Valid until ${response.validUntil?.toLocal().toString().substring(0, 10) ?? "N/A"}'),
        ),
      );
      Navigator.pop(context, true);
    } on ApiException catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(e.message)));
    } finally {
      setState(() => _isProcessing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Renew Subscription")),
      body: ListView.builder(
        itemCount: widget.expiredSubscriptions.length,
        itemBuilder: (ctx, i) {
          final sub = widget.expiredSubscriptions[i];
          return Card(
            child: ListTile(
              title: Text(sub.planName),
              subtitle: Text("${sub.planPrice} DZD - Expired"),
              trailing: _isProcessing
                  ? const CircularProgressIndicator()
                  : ElevatedButton(
                      onPressed: () => _renewOnline(sub),
                      child: const Text("Renew Online"),
                    ),
            ),
          );
        },
      ),
    );
  }
}
```

---

## 5. Flutter — Member Views Subscription History

**User action:** Member opens "My Subscriptions" to see all past and current subscriptions.

**Endpoint:** `GET /api/me/subscriptions`

**Auth:** `MemberOnly`

**Response (200):**
```json
[
  {
    "id": "sub-1111-...",
    "planName": "Monthly Premium",
    "planPrice": 5000,
    "status": "Active",
    "startDate": "2026-06-02T12:00:00Z",
    "endDate": "2026-07-02T12:00:00Z",
    "remainingSessions": null,
    "paymentStatus": "Paid",
    "autoRenew": false
  }
]
```

**Flutter:**
```dart
class MeSubscriptionResponse {
  final String id;
  final String planName;
  final double planPrice;
  final String status; // Active | Expired | Cancelled | Paused
  final DateTime startDate;
  final DateTime? endDate;
  final int? remainingSessions;
  final String paymentStatus;
  final bool autoRenew;

  MeSubscriptionResponse({
    required this.id,
    required this.planName,
    required this.planPrice,
    required this.status,
    required this.startDate,
    this.endDate,
    this.remainingSessions,
    required this.paymentStatus,
    required this.autoRenew,
  });

  factory MeSubscriptionResponse.fromJson(Map<String, dynamic> json) {
    return MeSubscriptionResponse(
      id: json['id'],
      planName: json['planName'],
      planPrice: (json['planPrice'] as num).toDouble(),
      status: json['status'],
      startDate: DateTime.parse(json['startDate']),
      endDate: json['endDate'] != null ? DateTime.parse(json['endDate']) : null,
      remainingSessions: json['remainingSessions'],
      paymentStatus: json['paymentStatus'],
      autoRenew: json['autoRenew'],
    );
  }

  bool get isActive => status == 'Active';
  bool get isExpired => status == 'Expired';
}

Future<List<MeSubscriptionResponse>> getMySubscriptions() async {
  final uri = Uri.parse('$apiGateway/api/me/subscriptions');
  final response = await http.get(uri, headers: await _authHeaders());
  if (response.statusCode == 200) {
    final List<dynamic> body = jsonDecode(response.body);
    return body.map((e) => MeSubscriptionResponse.fromJson(e)).toList();
  }
  throw ApiException('Failed to load subscriptions');
}
```

---

## 6. React — Member Browses Plans & Buys with Cash

**User action:** Member has no active subscription (or wants a different plan). They browse available plans, pick one, and submit a cash purchase request for admin approval.

**Backend flow:** Creates a `PaymentApprovalRequest` with `RequestType=PlanPurchase` and `Status=Pending`. Admin will later view and accept/reject it.

**Key differences from renewal:**
- Request sends `planId` instead of `subscriptionId`
- No `amount` field — the server resolves the plan price automatically
- The member must NOT have an active subscription (409 if they do)

### 6.1 Member — Browse Available Plans

**User action:** Member opens the "Plans" page to see what plans are available to purchase.

**Endpoint:** `GET /api/plans`

**Auth:** `Authenticated`

**Response (200):**
```json
[
  {
    "id": "plan-9999-...",
    "name": "Monthly Premium",
    "description": "Full gym access + classes",
    "price": 5000,
    "durationValue": 1,
    "durationUnit": "Months",
    "sessionCount": null,
    "isActive": true
  },
  {
    "id": "plan-8888-...",
    "name": "10 Session Pack",
    "description": "10 gym sessions, no expiry",
    "price": 2000,
    "durationValue": null,
    "durationUnit": null,
    "sessionCount": 10,
    "isActive": true
  }
]
```

**React:**
```tsx
async function getAvailablePlans() {
  const res = await fetch(`${API}/api/plans`, { headers: authHeaders() });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // PlanItem[]
}
```

### 6.2 Member — Submit Cash Purchase Request

**User action:** Member selects a plan → taps "Buy with Cash" → a confirmation dialog shows the plan name and price → taps "Submit Request".

**Endpoint:** `POST /api/subscriptions/purchase`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "planId": "plan-9999-...",
  "notes": "Will pay in cash tomorrow at the front desk"
}
```

> The server auto-resolves the plan price. No need to send `amount`.

**Response (201):**
```json
{
  "requestId": "req-bbbb-..."
}
```

**React:**
```tsx
async function submitCashPurchase(planId: string, notes?: string) {
  const res = await fetch(`${API}/api/subscriptions/purchase`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ planId, notes }),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to submit purchase request");
  }
  return res.json();
}
```

---

## 7. React — Admin Reviews & Accepts/Rejects Cash Purchases

### 7.1 Admin — View Pending Purchase Requests

**User action:** Admin opens the "Pending Purchases" dashboard → sees a list of member purchase requests with plan names, amounts, and dates.

**Endpoint:** `GET /api/subscriptions/purchase/pending`

**Auth:** `AdminOnly`

**Response (200):**
```json
[
  {
    "requestId": "req-bbbb-...",
    "memberId": "a1b2c3d4-...",
    "memberName": "Jane Doe",
    "planId": "plan-9999-...",
    "planName": "Monthly Premium",
    "amount": 5000,
    "status": "Pending",
    "createdAt": "2026-06-03T14:00:00Z",
    "notes": "Will pay in cash tomorrow at the front desk"
  }
]
```

**React:**
```tsx
async function fetchPendingPurchases() {
  const res = await fetch(`${API}/api/subscriptions/purchase/pending`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // PendingPurchaseRequest[]
}
```

### 7.2 Admin — Accept a Cash Purchase

**User action:** Admin receives cash from the member → taps "Accept" → the system creates a subscription, records the payment, and activates the plan.

**Endpoint:** `PATCH /api/subscriptions/purchase/{requestId}/accept`

**Auth:** `AdminOnly`

**Response (200):**
```json
{
  "requestId": "req-bbbb-...",
  "paymentId": "pay-cccc-...",
  "status": "Accepted"
}
```

**React:**
```tsx
async function acceptCashPurchase(requestId: string) {
  const res = await fetch(`${API}/api/subscriptions/purchase/${requestId}/accept`, {
    method: "PATCH",
    headers: authHeaders(),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to accept purchase");
  }
  return res.json();
}
```

### 7.3 Admin — Reject a Cash Purchase

**User action:** Admin decides to reject the request → optionally provides a reason.

**Endpoint:** `PATCH /api/subscriptions/purchase/{requestId}/reject`

**Auth:** `AdminOnly`

**Request body (JSON) — optional:**
```json
{
  "reason": "Selected plan is no longer available"
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
async function rejectCashPurchase(requestId: string, reason?: string) {
  const res = await fetch(`${API}/api/subscriptions/purchase/${requestId}/reject`, {
    method: "PATCH",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ reason }),
  });
  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to reject purchase");
  }
  return res.json();
}
```

---

## 8. Flutter — Member Buys a New Plan Online (Auto-Accepted)

**User action:** Member browses plans → taps "Buy Online" → confirms purchase → the system processes the payment (simulated) and activates the subscription immediately.

### 8.1 Member — Purchase Online

**Endpoint:** `POST /api/subscriptions/purchase/online`

**Auth:** `MemberOnly`

**Request body (JSON):**
```json
{
  "planId": "plan-9999-...",
  "notes": null
}
```

> The server auto-resolves the plan price. No need to send `amount`.

**Response (200):**
```json
{
  "subscriptionId": "sub-3333-...",
  "paymentId": "pay-dddd-...",
  "planName": "Monthly Premium",
  "amount": 5000,
  "paymentMethod": "CreditCard",
  "purchasedAt": "2026-06-03T15:00:00Z",
  "validUntil": "2026-07-03T00:00:00Z"
}
```

**Flutter:**
```dart
class OnlinePurchaseResponse {
  final String subscriptionId;
  final String paymentId;
  final String planName;
  final double amount;
  final String paymentMethod;
  final DateTime purchasedAt;
  final DateTime? validUntil;

  OnlinePurchaseResponse({
    required this.subscriptionId,
    required this.paymentId,
    required this.planName,
    required this.amount,
    required this.paymentMethod,
    required this.purchasedAt,
    this.validUntil,
  });

  factory OnlinePurchaseResponse.fromJson(Map<String, dynamic> json) {
    return OnlinePurchaseResponse(
      subscriptionId: json['subscriptionId'],
      paymentId: json['paymentId'],
      planName: json['planName'],
      amount: (json['amount'] as num).toDouble(),
      paymentMethod: json['paymentMethod'],
      purchasedAt: DateTime.parse(json['purchasedAt']),
      validUntil: json['validUntil'] != null
          ? DateTime.parse(json['validUntil'])
          : null,
    );
  }
}

Future<OnlinePurchaseResponse> purchaseOnline({
  required String planId,
  String? notes,
}) async {
  final uri = Uri.parse('$apiGateway/api/subscriptions/purchase/online');
  final response = await http.post(
    uri,
    headers: {
      ...await _authHeaders(),
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'planId': planId,
      'notes': notes,
    }),
  );

  if (response.statusCode == 200) {
    return OnlinePurchaseResponse.fromJson(jsonDecode(response.body));
  }

  final error = jsonDecode(response.body);
  throw ApiException(error['detail'] ?? 'Purchase failed');
}
```

### 8.2 Screen Example

```dart
class PlansScreen extends StatefulWidget {
  @override
  State<PlansScreen> createState() => _PlansScreenState();
}

class _PlansScreenState extends State<PlansScreen> {
  late Future<List<PlanItem>> _plansFuture;
  bool _isProcessing = false;
  String? _processingPlanId;

  @override
  void initState() {
    super.initState();
    _plansFuture = getAvailablePlans();
  }

  Future<void> _purchaseOnline(PlanItem plan) async {
    setState(() {
      _isProcessing = true;
      _processingPlanId = plan.id;
    });
    try {
      final response = await purchaseOnline(planId: plan.id);

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            '${plan.name} activated! Valid until ${response.validUntil?.toLocal().toString().substring(0, 10) ?? "N/A"}'),
        ),
      );
      Navigator.pop(context, true);
    } on ApiException catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text(e.message)));
    } finally {
      setState(() {
        _isProcessing = false;
        _processingPlanId = null;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Choose a Plan")),
      body: FutureBuilder<List<PlanItem>>(
        future: _plansFuture,
        builder: (ctx, snapshot) {
          if (!snapshot.hasData) {
            return const Center(child: CircularProgressIndicator());
          }
          final plans = snapshot.data!;
          return ListView.builder(
            itemCount: plans.length,
            itemBuilder: (ctx, i) {
              final plan = plans[i];
              final isProcessingThis = _isProcessing && _processingPlanId == plan.id;
              return Card(
                child: ListTile(
                  title: Text(plan.name),
                  subtitle: Text("${plan.price} DZD${plan.description != null ? ' - ${plan.description}' : ''}"),
                  trailing: isProcessingThis
                      ? const CircularProgressIndicator()
                      : ElevatedButton(
                          onPressed: () => _purchaseOnline(plan),
                          child: const Text("Buy Online"),
                        ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
```

## 9. Error Handling

All error responses follow the RFC 9457 Problem Details format:

```json
{
  "type": "about:blank",
  "title": "Conflict",
  "status": 409,
  "detail": "This request has already been resolved."
}
```

| HTTP Status | Meaning | Common Causes |
|------------|---------|---------------|
| **200** | Success | GET, POST, PATCH |
| **400** | Bad Request | Amount doesn't match plan price, subscription not expired |
| **401** | Unauthorized | Missing or expired JWT |
| **403** | Forbidden | Valid JWT but wrong role (e.g., Member tries admin action) |
| **404** | Not Found | Subscription, plan, member, or request ID doesn't exist |
| **409** | Conflict | Request already resolved, member already has an active subscription |
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

### Subscription Management

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `GET` | `/api/me` | Member | — | `GetMyProfileResponse` (includes `memberId`) |
| `GET` | `/api/me/subscriptions` | Member | — | `MeSubscriptionResponse[]` |
| `GET` | `/api/me/subscription` | Member | — | Active subscription or 404 |
| `GET` | `/api/members/{id}/subscriptions` | Admin/Coach | — | `SubscriptionHistoryResponse[]` |

### Cash Renewal Flow (Member → Admin)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/subscriptions/renew` | Member | `{ subscriptionId, amount, notes? }` | `{ requestId }` |
| `GET` | `/api/subscriptions/renew/pending` | Admin | — | `PendingRequest[]` |
| `PATCH` | `/api/subscriptions/renew/{id}/accept` | Admin | — | `{ requestId, paymentId, status }` |
| `PATCH` | `/api/subscriptions/renew/{id}/reject` | Admin | `{ reason? }` | `{ requestId, status }` |

### Online Renewal Flow (Member, Auto-Accepted)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/subscriptions/renew/online` | Member | `{ subscriptionId, amount, notes? }` | `OnlineRenewalResponse` |

### Plans (Browse Available Plans)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `GET` | `/api/plans` | Auth'd | — | `PlanItem[]` |

### Cash Purchase Flow — Buy New Plan (Member → Admin)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/subscriptions/purchase` | Member | `{ planId, notes? }` | `{ requestId }` |
| `GET` | `/api/subscriptions/purchase/pending` | Admin | — | `PendingPurchaseRequest[]` |
| `PATCH` | `/api/subscriptions/purchase/{id}/accept` | Admin | `{ notes? }` | `{ requestId, paymentId, status }` |
| `PATCH` | `/api/subscriptions/purchase/{id}/reject` | Admin | `{ reason? }` | `{ requestId, status }` |

### Online Purchase Flow — Buy New Plan (Member, Auto-Accepted)

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/subscriptions/purchase/online` | Member | `{ planId, notes? }` | `OnlinePurchaseResponse` |

### Admin Direct Creation

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `POST` | `/api/subscriptions` | Admin | `{ memberId, planId }` | `{ subscriptionId, paymentStatus }` |
| `POST` | `/api/subscriptions/confirm-payment` | Admin | `{ subscriptionId, amountReceived, paymentMethod, notes? }` | `{ subscriptionId, paymentId, paymentStatus }` |

---

### Quick Status Values Reference

**Subscription status:** `Active` | `Expired` | `Cancelled` | `Paused`

**Payment status:** `Pending` | `Paid` | `Refunded` | `Cancelled`

**PaymentApprovalRequest status:** `Pending` | `Accepted` | `Rejected`

**Roles:** `Admin` | `Member` | `Coach` (from JWT role claim)
