# FitTech.Backend — API Testing Tutorial

Test all 30 API endpoints across Identity, Membership, and Payment services using **curl** (CLI) and **Scalar UI** (interactive browser).

---

## 1. Prerequisites

- **Docker Desktop** running (PostgreSQL, RabbitMQ containers)
- **Aspire AppHost** running via:
  ```powershell
  dotnet run --project FitTech.AppHost/FitTech.AppHost/FitTech.AppHost.csproj
  ```
- **Databases are fresh/empty** — first run auto-migrates (EF Core `MigrateAsync()`)

---

## 2. Find the Gateway URL

Open the **Aspire Dashboard** (usually http://localhost:15105 or check the terminal output).

Look for the `gateway` resource. Copy its endpoint — typically something like:
```
http://localhost:5000
```

All curl examples below assume `$GATEWAY` is set:
```powershell
$GATEWAY = "http://localhost:5000"
```

---

## 3. Run via Scalar UI (Browser)

Open your gateway URL in a browser, then append `/scalar/v1`:
```
http://localhost:5000/scalar/v1
```

You'll see three API documents in the dropdown (top-left):
- **Identity API**
- **Membership API**
- **Payment API**

For most endpoints you must first obtain a JWT token (see §4) and click **Authorize** in Scalar to paste it. Some endpoints (register, login) are unauthenticated.

---

## 4. Authentication

### 4.1 Seed Credentials

| Role   | Email                 | Password     |
|--------|-----------------------|--------------|
| Admin  | `admin@fitteck.com`   | `Admin@12345` |
| Member | `Member@fitteck.com`  | `Member@12345` |
| Coach  | `Coach@fitteck.com`   | `Coach@12345` |

### 4.2 Login (get JWT)

```powershell
curl -s -X POST "$GATEWAY/api/User/login" `
  -H "Content-Type: application/json" `
  -d '{\"emailOrUserName\": \"admin@fitteck.com\", \"password\": \"Admin@12345\", \"clientId\": \"fittech-webapp\"}'
```

**Save the token:**
```powershell
$TOKEN = (curl -s -X POST "$GATEWAY/api/User/login" `
  -H "Content-Type: application/json" `
  -d '{\"emailOrUserName\": \"admin@fitteck.com\", \"password\": \"Admin@12345\", \"clientId\": \"fittech-webapp\"}' | `
  ConvertFrom-Json).data.token

echo "Token: $TOKEN"
```

Response (`LoginResponseDTO`):
```json
{
  "succeeded": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "requiresTwoFactor": false
}
```

---

## 5. Identity Endpoints (15)

All under `$GATEWAY/api/User/`.

### 5.1 Register a new user

```powershell
curl -s -X POST "$GATEWAY/api/User/register" `
  -F "userName=testuser" `
  -F "email=testuser@example.com" `
  -F "password=Test@12345" `
  -F "confirmPassword=Test@12345" `
  -F "firstName=Test" `
  -F "lastName=User" `
  -F "phoneNumber=+213555000000"
```

### 5.2 Send confirmation email (get token)

```powershell
curl -s -X POST "$GATEWAY/api/User/send-confirmation-email" `
  -H "Content-Type: application/json" `
  -d '{\"email\": \"testuser@example.com\"}'
```

### 5.3 Verify email

```powershell
# Replace USER_ID and TOKEN with values from step 5.2
curl -s -X POST "$GATEWAY/api/User/verify-email" `
  -H "Content-Type: application/json" `
  -d '{\"userId\": \"GUID-FROM-STEP-5.2\", \"token\": \"TOKEN-FROM-STEP-5.2\"}'
```

### 5.4 Login (as new user)

```powershell
curl -s -X POST "$GATEWAY/api/User/login" `
  -H "Content-Type: application/json" `
  -d '{\"emailOrUserName\": \"testuser@example.com\", \"password\": \"Test@12345\", \"clientId\": \"fittech-webapp\"}'
```

### 5.5 Refresh token

```powershell
curl -s -X POST "$GATEWAY/api/User/refresh-token" `
  -H "Content-Type: application/json" `
  -d '{\"refreshToken\": \"REFRESH_TOKEN\", \"clientId\": \"fittech-webapp\"}'
```

### 5.6 Revoke token

```powershell
curl -s -X POST "$GATEWAY/api/User/revoke-token" `
  -H "Content-Type: application/json" `
  -d '{\"refreshToken\": \"REFRESH_TOKEN\", \"clientId\": \"fittech-webapp\"}'
```

### 5.7 Forgot password

```powershell
curl -s -X POST "$GATEWAY/api/User/forgot-password" `
  -H "Content-Type: application/json" `
  -d '{\"email\": \"testuser@example.com\"}'
```

### 5.8 Reset password

```powershell
curl -s -X POST "$GATEWAY/api/User/reset-password" `
  -H "Content-Type: application/json" `
  -d '{\"userId\": \"GUID\", \"token\": \"TOKEN\", \"newPassword\": \"NewPass@12345\"}'
```

### 5.9 Change password (authenticated)

```powershell
curl -s -X POST "$GATEWAY/api/User/change-password" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $TOKEN" `
  -d '{\"currentPassword\": \"Admin@12345\", \"newPassword\": \"NewAdmin@12345\"}'
```

### 5.10 Get profile

```powershell
curl -s -X GET "$GATEWAY/api/User/profile/USER_GUID"
```

### 5.11 Update profile

```powershell
curl -s -X PUT "$GATEWAY/api/User/profile" `
  -F "userId=USER_GUID" `
  -F "firstName=Updated" `
  -F "lastName=Name" `
  -F "phoneNumber=+213555111111" `
  -F "gender=Male"
```

### 5.12 Check if user exists

```powershell
curl -s -X GET "$GATEWAY/api/User/USER_GUID/exists"
```

### 5.13 Deactivate user

```powershell
curl -s -X PUT "$GATEWAY/api/User/USER_GUID/deactivate"
```

### 5.14 Upload medical file

```powershell
curl -s -X POST "$GATEWAY/api/User/medical-file" `
  -F "userId=USER_GUID" `
  -F "file=@C:\path\to\medical.pdf"
```

### 5.15 Get medical file (by userId)

```powershell
curl -s -X GET "$GATEWAY/api/User/USER_GUID/medical-file"
```

---

## 6. Membership Endpoints (14)

### 6.1 Plans CRUD (no auth required currently)

#### 6.1.1 Create a plan

```powershell
curl -s -X POST "$GATEWAY/api/plans" `
  -H "Content-Type: application/json" `
  -d '{\"name\": \"Monthly Premium\", \"description\": \"Full access to gym and classes\", \"price\": 99.99, \"durationValue\": 1, \"durationUnit\": \"Months\", \"sessionCount\": null, \"accessRules\": [\"gym\", \"classes\", \"sauna\"]}'
```

Save the plan ID:
```powershell
$PLAN_ID = (curl -s -X POST "$GATEWAY/api/plans" `
  -H "Content-Type: application/json" `
  -d '{\"name\": \"Monthly Premium\", \"description\": \"Full access\", \"price\": 99.99, \"durationValue\": 1, \"durationUnit\": \"Months\", \"sessionCount\": null, \"accessRules\": [\"gym\"]}' | `
  ConvertFrom-Json).planId
```

Also create a session-based plan:
```powershell
curl -s -X POST "$GATEWAY/api/plans" `
  -H "Content-Type: application/json" `
  -d '{\"name\": \"10 Sessions\", \"description\": \"Drop-in sessions\", \"price\": 49.99, \"durationValue\": 6, \"durationUnit\": \"Months\", \"sessionCount\": 10, \"accessRules\": [\"gym\"]}'

$SESSION_PLAN_ID = (curl -s -X POST "$GATEWAY/api/plans" `
  -H "Content-Type: application/json" `
  -d '{\"name\": \"10 Sessions\", \"description\": \"Drop-in\", \"price\": 49.99, \"durationValue\": 6, \"durationUnit\": \"Months\", \"sessionCount\": 10, \"accessRules\": [\"gym\"]}' | `
  ConvertFrom-Json).planId
```

#### 6.1.2 List all plans

```powershell
curl -s -X GET "$GATEWAY/api/plans" | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

#### 6.1.3 Update a plan

```powershell
curl -s -X PUT "$GATEWAY/api/plans/$PLAN_ID" `
  -H "Content-Type: application/json" `
  -d '{\"name\": \"Monthly Premium Plus\", \"description\": \"Premium access + personal training\", \"price\": 149.99, \"durationValue\": 1, \"durationUnit\": \"Months\", \"sessionCount\": 4, \"accessRules\": [\"gym\", \"classes\", \"sauna\", \"pt\"], \"isActive\": true}'
```

#### 6.1.4 Delete a plan

```powershell
curl -s -X DELETE "$GATEWAY/api/plans/$PLAN_ID"
```

### 6.2 Members CRUD

#### 6.2.1 Create a member (no auth currently)

Log in as admin first:
```powershell
$ADMIN_TOKEN = (curl -s -X POST "$GATEWAY/api/User/login" `
  -H "Content-Type: application/json" `
  -d '{\"emailOrUserName\": \"admin@fitteck.com\", \"password\": \"Admin@12345\", \"clientId\": \"fittech-webapp\"}' | `
  ConvertFrom-Json).data.token

# Recreate plan if you deleted it
$PLAN_ID = (curl -s -X POST "$GATEWAY/api/plans" -H "Content-Type: application/json" -d '{\"name\": \"Monthly Premium\", \"description\": \"Full access\", \"price\": 99.99, \"durationValue\": 1, \"durationUnit\": \"Months\"}' | ConvertFrom-Json).planId

curl -s -X POST "$GATEWAY/api/members" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -F "firstName=John" `
  -F "lastName=Doe" `
  -F "email=john.doe@example.com" `
  -F "phoneNumber=+213555123456" `
  -F "dateOfBirth=1990-01-15" `
  -F "gender=Male" `
  -F "planId=$PLAN_ID"
```

Save the member ID:
```powershell
$MEMBER_ID = (curl -s -X POST "$GATEWAY/api/members" -H "Authorization: Bearer $ADMIN_TOKEN" -F "firstName=John" -F "lastName=Doe" -F "email=john.doe@example.com" -F "phoneNumber=+213555123456" -F "dateOfBirth=1990-01-15" -F "gender=Male" -F "planId=$PLAN_ID" | ConvertFrom-Json).memberId
```

#### 6.2.2 Get member by ID

```powershell
curl -s -X GET "$GATEWAY/api/members/$MEMBER_ID" `
  -H "Authorization: Bearer $ADMIN_TOKEN" | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

#### 6.2.3 List members (paginated)

```powershell
curl -s -X GET "$GATEWAY/api/members?page=1&pageSize=10" `
  -H "Authorization: Bearer $ADMIN_TOKEN" | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

#### 6.2.4 Update member

```powershell
curl -s -X PUT "$GATEWAY/api/members/$MEMBER_ID" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -F "firstName=John" `
  -F "lastName=Smith" `
  -F "phoneNumber=+213555999999" `
  -F "gender=Male"
```

#### 6.2.5 Update my profile (member self-service)

```powershell
# Log in as a member
$MEMBER_TOKEN = (curl -s -X POST "$GATEWAY/api/User/login" `
  -H "Content-Type: application/json" `
  -d '{\"emailOrUserName\": \"Member@fitteck.com\", \"password\": \"Member@12345\", \"clientId\": \"fittech-webapp\"}' | `
  ConvertFrom-Json).data.token

curl -s -X PUT "$GATEWAY/api/members/my-profile" `
  -H "Authorization: Bearer $MEMBER_TOKEN" `
  -F "firstName=Member" `
  -F "lastName=Updated" `
  -F "phoneNumber=+213555888888"
```

#### 6.2.6 Get active subscription

```powershell
curl -s -X GET "$GATEWAY/api/members/active-subscription?memberId=$MEMBER_ID" `
  -H "Authorization: Bearer $ADMIN_TOKEN" | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

#### 6.2.7 Get subscription history

```powershell
curl -s -X GET "$GATEWAY/api/members/$MEMBER_ID/subscriptions" `
  -H "Authorization: Bearer $ADMIN_TOKEN" | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

#### 6.2.8 Delete member

```powershell
curl -s -X DELETE "$GATEWAY/api/members/$MEMBER_ID" `
  -H "Authorization: Bearer $ADMIN_TOKEN"
```

Re-create a member if you deleted it:
```powershell
$MEMBER_ID = (curl -s -X POST "$GATEWAY/api/members" -H "Authorization: Bearer $ADMIN_TOKEN" -F "firstName=Jane" -F "lastName=Doe" -F "email=jane.doe@example.com" -F "phoneNumber=+213555123456" -F "dateOfBirth=1992-06-20" -F "gender=Female" -F "planId=$PLAN_ID" | ConvertFrom-Json).memberId
```

### 6.3 Subscriptions

#### 6.3.1 Create subscription (Admin only)

```powershell
curl -s -X POST "$GATEWAY/api/subscriptions" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"memberId\": \"$MEMBER_ID\", \"planId\": \"$PLAN_ID\", \"paymentMethod\": \"Cash\", \"notes\": \"First subscription\"}"
```

Save the subscription ID:
```powershell
$SUBSCRIPTION_ID = (curl -s -X POST "$GATEWAY/api/subscriptions" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"memberId\": \"$MEMBER_ID\", \"planId\": \"$PLAN_ID\", \"paymentMethod\": \"Cash\", \"notes\": \"First sub\"}" | `
  ConvertFrom-Json).subscriptionId
```

#### 6.3.2 Confirm cash payment (Admin only)

```powershell
curl -s -X POST "$GATEWAY/api/subscriptions/confirm-payment" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"subscriptionId\": \"$SUBSCRIPTION_ID\", \"amountReceived\": 99.99, \"paymentMethod\": \"Cash\", \"notes\": \"Payment confirmed at reception\"}" | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

---

## 7. Payment Endpoints (1)

### 7.1 Create a payment

Requires authentication. Use admin token.

```powershell
# Need a reference ID from a subscription or other domain object
# If you don't have one, use the subscription created above
curl -s -X POST "$GATEWAY/api/payments" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"userId\": \"USER_GUID\", \"amount\": 99.99, \"paymentMethod\": \"Cash\", \"paymentType\": \"Subscription\", \"referenceId\": \"$SUBSCRIPTION_ID\", \"notes\": \"Payment via API\"}"
```

---

## 8. End-to-End Flow: Complete Membership Onboarding

This is the realistic workflow from user registration through payment.

### Step 1: Register a new user
```powershell
curl -s -X POST "$GATEWAY/api/User/register" `
  -F "userName=janedoe" `
  -F "email=janedoe@example.com" `
  -F "password=Test@12345" `
  -F "confirmPassword=Test@12345" `
  -F "firstName=Jane" `
  -F "lastName=Doe" `
  -F "phoneNumber=+213555777777"
```

### Step 2: Login as newly registered user
```powershell
$USER_TOKEN = (curl -s -X POST "$GATEWAY/api/User/login" `
  -H "Content-Type: application/json" `
  -d '{\"emailOrUserName\": \"janedoe@example.com\", \"password\": \"Test@12345\", \"clientId\": \"fittech-webapp\"}' | `
  ConvertFrom-Json).data.token

$USER_GUID = (curl -s -X GET "$GATEWAY/api/User/janedoe@example.com/exists" | ConvertFrom-Json).data.userId
```

### Step 3: Admin creates a plan
```powershell
$PLAN_ID = (curl -s -X POST "$GATEWAY/api/plans" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d '{\"name\": \"Monthly Premium\", \"description\": \"Full access to all amenities\", \"price\": 89.99, \"durationValue\": 1, \"durationUnit\": \"Months\"}' | `
  ConvertFrom-Json).planId
```

### Step 4: Admin creates the member record
```powershell
$MEMBER_ID = (curl -s -X POST "$GATEWAY/api/members" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -F "firstName=Jane" `
  -F "lastName=Doe" `
  -F "email=janedoe@example.com" `
  -F "phoneNumber=+213555777777" `
  -F "dateOfBirth=1995-03-10" `
  -F "gender=Female" `
  -F "planId=$PLAN_ID" | `
  ConvertFrom-Json).memberId
```

### Step 5: Admin creates a subscription for the member
```powershell
$SUBSCRIPTION_ID = (curl -s -X POST "$GATEWAY/api/subscriptions" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"memberId\": \"$MEMBER_ID\", \"planId\": \"$PLAN_ID\", \"paymentMethod\": \"Cash\", \"notes\": \"Walk-in registration\"}" | `
  ConvertFrom-Json).subscriptionId
```

### Step 6: Confirm payment via membership (cash at reception)
```powershell
curl -s -X POST "$GATEWAY/api/subscriptions/confirm-payment" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"subscriptionId\": \"$SUBSCRIPTION_ID\", \"amountReceived\": 89.99, \"paymentMethod\": \"Cash\", \"notes\": \"Paid at front desk\"}"
```

### Step 7: Create payment record in Payment service
```powershell
# Get the user's actual GUID
$PROFILE = curl -s -X GET "$GATEWAY/api/User/profile/$MEMBER_ID" | ConvertFrom-Json
$USER_GUID = $PROFILE.data.userId

curl -s -X POST "$GATEWAY/api/payments" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $ADMIN_TOKEN" `
  -d "{\"userId\": \"$USER_GUID\", \"amount\": 89.99, \"paymentMethod\": \"Cash\", \"paymentType\": \"Subscription\", \"referenceId\": \"$SUBSCRIPTION_ID\", \"notes\": \"Full payment recorded\"}"
```

### Step 8: Verify everything
```powershell
# Check member profile + active subscription
curl -s -X GET "$GATEWAY/api/members/$MEMBER_ID" `
  -H "Authorization: Bearer $ADMIN_TOKEN" | ConvertFrom-Json | ConvertTo-Json -Depth 10

# Check subscription history
curl -s -X GET "$GATEWAY/api/members/$MEMBER_ID/subscriptions" `
  -H "Authorization: Bearer $ADMIN_TOKEN" | ConvertFrom-Json | ConvertTo-Json -Depth 5
```

---

## 9. Quick Reference

### 9.1 Auth token variable (PowerShell)
```powershell
$TOKEN = (curl -s -X POST "$GATEWAY/api/User/login" -H "Content-Type: application/json" -d '{\"emailOrUserName\": \"admin@fitteck.com\", \"password\": \"Admin@12345\", \"clientId\": \"fittech-webapp\"}' | ConvertFrom-Json).data.token
```

### 9.2 All routes summary

| Method | Route | Auth | Service |
|--------|-------|------|---------|
| POST | `/api/User/register` | None | Identity |
| POST | `/api/User/send-confirmation-email` | None | Identity |
| POST | `/api/User/verify-email` | None | Identity |
| POST | `/api/User/login` | None | Identity |
| POST | `/api/User/refresh-token` | None | Identity |
| POST | `/api/User/revoke-token` | None | Identity |
| POST | `/api/User/forgot-password` | None | Identity |
| POST | `/api/User/reset-password` | None | Identity |
| POST | `/api/User/change-password` | Bearer | Identity |
| GET | `/api/User/profile/{userId}` | None | Identity |
| PUT | `/api/User/profile` | None | Identity |
| GET | `/api/User/{userId}/exists` | None | Identity |
| PUT | `/api/User/{userId}/deactivate` | None | Identity |
| POST | `/api/User/medical-file` | None | Identity |
| GET | `/api/User/{userId}/medical-file` | None | Identity |
| POST | `/api/plans` | None* | Membership |
| GET | `/api/plans` | None* | Membership |
| PUT | `/api/plans/{id}` | None* | Membership |
| DELETE | `/api/plans/{id}` | None* | Membership |
| POST | `/api/members` | None* | Membership |
| GET | `/api/members/{id}` | Bearer (Admin/Coach) | Membership |
| GET | `/api/members` | Bearer (Admin/Coach) | Membership |
| PUT | `/api/members/{id}` | None* | Membership |
| PUT | `/api/members/my-profile` | Bearer* | Membership |
| DELETE | `/api/members/{id}` | None* | Membership |
| GET | `/api/members/active-subscription` | Bearer | Membership |
| GET | `/api/members/{id}/subscriptions` | Bearer | Membership |
| POST | `/api/subscriptions` | Bearer (Admin) | Membership |
| POST | `/api/subscriptions/confirm-payment` | Bearer (Admin) | Membership |
| POST | `/api/payments` | Bearer | Payment |

*\* No auth guard in code — will be added later.*

### 9.3 Scalar docs URLs

| Service | OpenAPI URL |
|---------|-------------|
| Identity | `$GATEWAY/docs/identity/openapi/v1.json` |
| Membership | `$GATEWAY/docs/membership/openapi/v1.json` |
| Payment | `$GATEWAY/docs/payment/openapi/v1.json` |
| All-in-one | `$GATEWAY/scalar/v1` |

### 9.4 Shared enums (frequently used)

```json
PaymentMethod:  Cash = 0,  CreditCard = 1
PaymentStatus:  Pending = 0,  Paid = 1,  Refunded = 2,  Cancelled = 3
PaymentType:    Subscription = 0,  ECommerce = 1,  Session = 2
MemberStatus:   Active = 0,  Suspended = 1,  Paused = 2,  Cancelled = 3
DurationUnit:   Days = 0,  Months = 1
Gender:         Male = 0,  Female = 1
```

---

## 10. Troubleshooting

### 10.1 Gateway returns 503/connection refused
The target service is still starting. Wait for its Aspire health check to turn green.

### 10.2 Database errors / relation not found
EF Core auto-migrates on startup in Development. Restart the service via the Aspire dashboard.

### 10.3 Scalar UI shows no docs
The gateway needs both the routing and the Scalar middleware — check that `app.MapScalarApiReference()` is called in `Gateway/Program.cs`.

### 10.4 "Already running" build errors
Aspire locks running binaries. To rebuild a service, stop it from the Aspire dashboard first, then rebuild.

### 10.5 "Payment" namespace errors (CS0118)
If you see `'Payment' is a namespace but used like a type`, add this alias to the top of your file:
```csharp
using PaymentEntity = Payment.Domain.Entities.Payment;
```
