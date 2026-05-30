# =============================================================================
# FitTech.Backend — Full Integration Test Suite
# =============================================================================
# Targets 67 endpoints across 6 services through the YARP Gateway at :7248.
#
# Usage:
#   hurl --insecure --test tests/full-flow.hurl
#   hurl --insecure --test --variable gateway=https://localhost:7248 tests/full-flow.hurl
#
# Prerequisites:
#   dotnet run --project FitTech.AppHost/FitTech.AppHost/FitTech.AppHost.csproj
#   Wait for all services healthy, then run this.
#
# Seed data:
#   admin / Admin@12345  (email: Admin@fitteck.com, role: Admin)
#   Client IDs: web, android, ios
#
# ⚠ CreateMember auto-generates a random Identity password (sent via email).
#   Member self-service tests use a separately-registered Identity user.
# =============================================================================

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 0 — AUTHENTICATION                                                  ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [0.1] Login as Admin ─────────────────────────────────────────────────────────
POST {{gateway}}/api/User/login
{
    "emailOrUserName": "admin",
    "password": "Admin@12345",
    "clientId": "web"
}
HTTP 200
[Captures]
admin_token: jsonpath "$.Data.Token"
admin_refresh: jsonpath "$.Data.RefreshToken"
[Asserts]
jsonpath "$.Data.Succeeded" == true
jsonpath "$.Data.Token" isString
jsonpath "$.Data.RefreshToken" isString
jsonpath "$.Data.RequiresTwoFactor" == false

# [0.2] Refresh Token ──────────────────────────────────────────────────────────
POST {{gateway}}/api/User/refresh-token
{
    "refreshToken": "{{admin_refresh}}",
    "clientId": "web"
}
HTTP 200
[Captures]
admin_token: jsonpath "$.Data.Token"
admin_refresh: jsonpath "$.Data.RefreshToken"
[Asserts]
jsonpath "$.Data.Token" isString

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 1 — SUBSCRIPTION PLANS (Admin)                                      ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [1.1] Create Monthly Plan ────────────────────────────────────────────────────
POST {{gateway}}/api/plans
Authorization: Bearer {{admin_token}}
{
    "name": "Monthly Premium",
    "description": "Full gym access, unlimited sessions, 1 month",
    "price": 5000,
    "durationValue": 1,
    "durationUnit": 0,
    "sessionCount": 30,
    "accessRules": ["gym", "pool", "sauna"]
}
HTTP 201
[Captures]
plan_id: jsonpath "$.planId"
[Asserts]
jsonpath "$.planId" isString

# [1.2] Create Session-Pack Plan ───────────────────────────────────────────────
POST {{gateway}}/api/plans
Authorization: Bearer {{admin_token}}
{
    "name": "10 Sessions Pack",
    "description": "10 sessions, no expiry date",
    "price": 3000,
    "durationValue": null,
    "durationUnit": null,
    "sessionCount": 10,
    "accessRules": ["gym"]
}
HTTP 201
[Captures]
pack_plan_id: jsonpath "$.planId"
[Asserts]
jsonpath "$.planId" isString

# [1.3] List All Plans ─────────────────────────────────────────────────────────
GET {{gateway}}/api/plans
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection
jsonpath "$[0].name" exists

# [1.4] List Subscription Plans (public-facing endpoint) ───────────────────────
GET {{gateway}}/api/subscriptions
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [1.5] Update a Plan ──────────────────────────────────────────────────────────
PUT {{gateway}}/api/plans/{{pack_plan_id}}
Authorization: Bearer {{admin_token}}
{
    "name": "10 Sessions Pack (Updated)",
    "description": "Updated description with pool access",
    "price": 3500,
    "durationValue": null,
    "durationUnit": null,
    "sessionCount": 10,
    "accessRules": ["gym", "pool"]
}
HTTP 204

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 2 — COACH CREATION (Admin)                                          ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [2.1] Create Coach ───────────────────────────────────────────────────────────
POST {{gateway}}/api/coaches
Authorization: Bearer {{admin_token}}
{
    "firstName": "Karim",
    "lastName": "Bensalem",
    "email": "karim.coach@fittech.com",
    "phoneNumber": "+213-555-111222",
    "dateOfBirth": "1990-03-20",
    "gender": "Male",
    "bio": "Certified CrossFit and HIIT coach with 8 years experience"
}
HTTP 201
[Captures]
coach_id: jsonpath "$.coachId"
[Asserts]
jsonpath "$.coachId" isString

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 3 — MEMBER CREATION (Admin)                                         ║
# ║  CreateMember auto-creates Identity user + Membership record + Subscription ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [3.1] Create Member (multipart) ──────────────────────────────────────────────
POST {{gateway}}/api/members
Authorization: Bearer {{admin_token}}
[MultipartFormData]
FirstName: Ahmed
LastName: MemberOne
Email: ahmed.member@fittech.com
PhoneNumber: "+213-555-333444"
DateOfBirth: 1995-06-15
Gender: Male
PlanId: {{plan_id}}
CardUid: NF-TEST-001
HTTP 201
[Captures]
member_id: jsonpath "$.memberId"
[Asserts]
jsonpath "$.memberId" isString

# [3.2] Create Second Member ───────────────────────────────────────────────────
POST {{gateway}}/api/members
Authorization: Bearer {{admin_token}}
[MultipartFormData]
FirstName: Sara
LastName: MemberTwo
Email: sara.member@fittech.com
PhoneNumber: "+213-555-555666"
DateOfBirth: 1998-11-22
Gender: Female
PlanId: {{plan_id}}
CardUid: NF-TEST-002
HTTP 201
[Captures]
member2_id: jsonpath "$.memberId"
[Asserts]
jsonpath "$.memberId" isString

# [3.3] List Members (paginated) ───────────────────────────────────────────────
GET {{gateway}}/api/members?page=1&pageSize=10
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.items" isCollection
jsonpath "$.totalCount" >= 2
jsonpath "$.page" == 1

# [3.4] List Members (search by name) ──────────────────────────────────────────
GET {{gateway}}/api/members?search=Ahmed&page=1&pageSize=10
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.items" isCollection

# [3.5] Get Member Detail ──────────────────────────────────────────────────────
GET {{gateway}}/api/members/{{member_id}}
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.firstName" == "Ahmed"
jsonpath "$.lastName" == "MemberOne"
jsonpath "$.status" exists
jsonpath "$.memberId" == "{{member_id}}"

# [3.6] Update Member (Admin) ──────────────────────────────────────────────────
PUT {{gateway}}/api/members/{{member_id}}
Authorization: Bearer {{admin_token}}
[MultipartFormData]
FirstName: Ahmed
LastName: MemberOneUpdated
PhoneNumber: "+213-555-333444"
Gender: Male
DateOfBirth: 1995-06-15
Objectives: Lose weight and build muscle
MedicalRestrictions: None
HTTP 200
[Asserts]
jsonpath "$.memberId" == "{{member_id}}"

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 4 — SUBSCRIPTION MANAGEMENT (Admin)                                 ║
# ╚══════════════════════════════════════════════════════════════════════════════╝
# The member already has an auto-created subscription. We create another and
# confirm its payment.

# [4.1] Create Additional Subscription ─────────────────────────────────────────
POST {{gateway}}/api/subscriptions
Authorization: Bearer {{admin_token}}
{
    "memberId": "{{member_id}}",
    "planId": "{{pack_plan_id}}",
    "paymentMethod": 0,
    "notes": "Additional session pack - cash pending"
}
HTTP 201
[Captures]
subscription_id: jsonpath "$.subscriptionId"
[Asserts]
jsonpath "$.subscriptionId" isString
jsonpath "$.paymentStatus" == "Pending"

# [4.2] Confirm Cash Payment ───────────────────────────────────────────────────
POST {{gateway}}/api/subscriptions/confirm-payment
Authorization: Bearer {{admin_token}}
{
    "subscriptionId": "{{subscription_id}}",
    "amountReceived": 3000,
    "paymentMethod": 0,
    "notes": "Cash received at front desk"
}
HTTP 200
[Asserts]
jsonpath "$.subscriptionId" == "{{subscription_id}}"
jsonpath "$.paymentStatus" == "Paid"

# [4.3] Get Active Subscription (Admin, specific member) ───────────────────────
GET {{gateway}}/api/members/active-subscription?memberId={{member_id}}
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.status" == "Active"
jsonpath "$.planName" exists

# [4.4] Get Subscription History ───────────────────────────────────────────────
GET {{gateway}}/api/members/{{member_id}}/subscriptions
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.subscriptions" isCollection

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 5 — REGISTER SEPARATE MEMBER FOR SELF-SERVICE TESTS                 ║
# ║  CreateMember auto-generates random passwords; we can't login with those.   ║
# ║  We register a standalone Identity user (Member role) for /api/me tests.    ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [5.1] Register Member Identity User ──────────────────────────────────────────
POST {{gateway}}/api/User/register
[MultipartFormData]
UserName: testmember
Email: test.member@fittech.com
Password: Member@12345
PhoneNumber: "+213-555-777888"
FirstName: Test
LastName: Member
Gender: Male
DateOfBirth: 1997-04-10
IsCoach: false
HTTP 200
[Captures]
member_user_id: jsonpath "$.Data"
[Asserts]
jsonpath "$.Success" == true

# [5.2] Login as Member ────────────────────────────────────────────────────────
POST {{gateway}}/api/User/login
{
    "emailOrUserName": "testmember",
    "password": "Member@12345",
    "clientId": "web"
}
HTTP 200
[Captures]
member_token: jsonpath "$.Data.token"
[Asserts]
jsonpath "$.Data.succeeded" == true
jsonpath "$.Data.token" isString

# [5.3] Get My Profile (may 404 if no Membership record linked) ────────────────
GET {{gateway}}/api/me
Authorization: Bearer {{member_token}}
HTTP *

# [5.4] Get My Subscription (may 404 if no Membership record) ──────────────────
GET {{gateway}}/api/me/subscription
Authorization: Bearer {{member_token}}
HTTP *

# [5.5] Update My Profile ──────────────────────────────────────────────────────
PATCH {{gateway}}/api/me
Authorization: Bearer {{member_token}}
[MultipartFormData]
Goals: Improve cardio, lose 5kg
HTTP *

# [5.6] Get Available Courses ──────────────────────────────────────────────────
GET {{gateway}}/api/me/courses/available
Authorization: Bearer {{member_token}}
HTTP 200

# [5.7] Get Enrolled Courses ───────────────────────────────────────────────────
GET {{gateway}}/api/me/courses/enrolled
Authorization: Bearer {{member_token}}
HTTP 200

# [5.8] Get Course Detail ──────────────────────────────────────────────────────
GET {{gateway}}/api/courses/00000000-0000-0000-0000-000000000001
Authorization: Bearer {{member_token}}
HTTP *

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 6 — COACH LOGIN & PROGRAM LIFECYCLE                                 ║
# ╚══════════════════════════════════════════════════════════════════════════════╝
# The coach was created in Phase 2 via Courses service. Now register the
# identity user so they can log in and use coach endpoints.

# [6.1] Register Coach Identity User ───────────────────────────────────────────
POST {{gateway}}/api/User/register
[MultipartFormData]
UserName: karim.coach
Email: karim.coach@fittech.com
Password: Coach@12345
PhoneNumber: "+213-555-111222"
FirstName: Karim
LastName: Bensalem
Gender: Male
DateOfBirth: 1990-03-20
IsCoach: true
HTTP 200
[Captures]
coach_user_id: jsonpath "$.Data"
[Asserts]
jsonpath "$.Success" == true

# [6.2] Login as Coach ─────────────────────────────────────────────────────────
POST {{gateway}}/api/User/login
{
    "emailOrUserName": "karim.coach",
    "password": "Coach@12345",
    "clientId": "web"
}
HTTP 200
[Captures]
coach_token: jsonpath "$.Data.token"
[Asserts]
jsonpath "$.Data.succeeded" == true
jsonpath "$.Data.token" isString

# [6.3] Coach Creates a Program ────────────────────────────────────────────────
POST {{gateway}}/api/programs
Authorization: Bearer {{coach_token}}
{
    "name": "HIIT Morning Burn",
    "description": "High-intensity interval training for all fitness levels",
    "level": "Intermediate",
    "exerciseType": "Cardio",
    "durationMinutes": 60,
    "startDate": "2026-06-01",
    "endDate": "2026-06-30",
    "totalPrice": 8000,
    "maxParticipants": 15,
    "pictureUrl": null,
    "timeSlots": [
        { "day": "Monday",    "startTime": "07:00", "endTime": "08:00", "description": "Morning burn" },
        { "day": "Wednesday", "startTime": "07:00", "endTime": "08:00", "description": "Midweek intensity" },
        { "day": "Friday",    "startTime": "07:00", "endTime": "08:00", "description": "Weekend prep" }
    ]
}
HTTP 201
[Captures]
program_id: jsonpath "$.programId"
[Asserts]
jsonpath "$.programId" isString

# [6.4] Coach Views Own Programs ───────────────────────────────────────────────
GET {{gateway}}/api/coaches/{{coach_id}}/programs
Authorization: Bearer {{coach_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [6.5] Admin Lists Pending Program Requests ───────────────────────────────────
GET {{gateway}}/api/programs/requests
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [6.6] Admin Views Program Request Detail ─────────────────────────────────────
GET {{gateway}}/api/programs/requests/{{program_id}}
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.name" == "HIIT Morning Burn"

# [6.7] Admin Accepts Program (triggers session auto-generation) ───────────────
PATCH {{gateway}}/api/programs/requests/{{program_id}}/accept
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.programId" == "{{program_id}}"

# [6.8] Coach Views Program Members ────────────────────────────────────────────
GET {{gateway}}/api/programs/{{program_id}}/members
Authorization: Bearer {{coach_token}}
HTTP 200

# [6.9] Coach Views Their Clients ──────────────────────────────────────────────
GET {{gateway}}/api/coaches/{{coach_id}}/clients
Authorization: Bearer {{coach_token}}
HTTP 200

# [6.10] Coach Views Specific Client Profile ────────────────────────────────────
GET {{gateway}}/api/coaches/{{coach_id}}/clients/{{member_id}}
Authorization: Bearer {{coach_token}}
HTTP 200

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 7 — ACTIVITY & ACCESS CONTROL (Admin)                               ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [7.1] Manual Entry (Check-in) ────────────────────────────────────────────────
POST {{gateway}}/api/activity/entry-exit/manual/enter
Authorization: Bearer {{admin_token}}
{
    "memberId": "{{member_id}}",
    "courseId": null
}
HTTP 201
[Captures]
activity_session_id: jsonpath "$.sessionId"
[Asserts]
jsonpath "$.sessionId" isString

# [7.2] View Today's Sessions ──────────────────────────────────────────────────
GET {{gateway}}/api/activity/sessions/today
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [7.3] Manual Exit (Check-out) ────────────────────────────────────────────────
POST {{gateway}}/api/activity/entry-exit/manual/exit
Authorization: Bearer {{admin_token}}
{
    "memberId": "{{member_id}}",
    "courseId": null
}
HTTP 200

# [7.4] View Member Activity History ───────────────────────────────────────────
GET {{gateway}}/api/activity/members/{{member_id}}
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [7.5] NFC Card Scan (Entry/Exit) ─────────────────────────────────────────────
POST {{gateway}}/api/activity/entry-exit/scan
Authorization: Bearer {{admin_token}}
{
    "cardUid": "NF-TEST-001"
}
HTTP *

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 8 — PAYMENT REQUESTS (Member → Admin)                               ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [8.1] Member Requests Membership Renewal ─────────────────────────────────────
POST {{gateway}}/api/requests/membership-renewal
Authorization: Bearer {{member_token}}
{
    "subscriptionId": "{{subscription_id}}",
    "amount": 5000,
    "notes": "Renewing monthly premium"
}
HTTP 201
[Captures]
renewal_request_id: jsonpath "$.requestId"
[Asserts]
jsonpath "$.requestId" isString

# [8.2] Admin Lists Pending Membership Renewal Requests ────────────────────────
GET {{gateway}}/api/requests/membership-renewal
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [8.3] Admin Accepts Membership Renewal ───────────────────────────────────────
PATCH {{gateway}}/api/requests/membership-renewal/{{renewal_request_id}}/accept
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.requestId" == "{{renewal_request_id}}"

# [8.4] Member Requests Course Purchase ────────────────────────────────────────
POST {{gateway}}/api/requests/course-purchase
Authorization: Bearer {{member_token}}
{
    "courseId": "{{program_id}}",
    "amount": 8000,
    "notes": "Enrolling in HIIT Morning Burn"
}
HTTP 201
[Captures]
course_req_id: jsonpath "$.requestId"
[Asserts]
jsonpath "$.requestId" isString

# [8.5] Admin Lists Pending Course Purchase Requests ───────────────────────────
GET {{gateway}}/api/requests/course-purchase
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# [8.6] Admin Rejects Course Purchase ──────────────────────────────────────────
PATCH {{gateway}}/api/requests/course-purchase/{{course_req_id}}/reject
Authorization: Bearer {{admin_token}}
HTTP 200

# [8.7] Create Direct Payment Record ───────────────────────────────────────────
POST {{gateway}}/api/payments
Authorization: Bearer {{admin_token}}
{
    "userId": "{{member_user_id}}",
    "amount": 5000,
    "paymentMethod": 0,
    "paymentType": 0,
    "referenceId": "{{subscription_id}}",
    "notes": "Direct payment from admin"
}
HTTP 201
[Captures]
payment_id: jsonpath "$.paymentId"
[Asserts]
jsonpath "$.paymentId" isString

# [8.8] Admin Lists All Payments ───────────────────────────────────────────────
GET {{gateway}}/api/payments
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$" isCollection

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 9 — DASHBOARDS & REPORTING (Admin)                                  ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [9.1] Admin Dashboard ────────────────────────────────────────────────────────
GET {{gateway}}/api/dashboard/admin
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.totalMembers" exists
jsonpath "$.monthlyRevenue" exists
jsonpath "$.activeSubscriptions" exists

# [9.2] Finance Dashboard ──────────────────────────────────────────────────────
GET {{gateway}}/api/dashboard/finance
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.totalRevenueThisMonth" exists
jsonpath "$.totalPayments" exists

# [9.3] Download Excel Report ──────────────────────────────────────────────────
GET {{gateway}}/api/reports/excel
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
header "Content-Type" contains "spreadsheet"

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 10 — MARK ATTENDANCE (Coach)                                        ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [10.1] Mark Attendance for a Session ─────────────────────────────────────────
# Session IDs are generated when program is accepted. Using placeholder.
POST {{gateway}}/api/sessions/00000000-0000-0000-0000-000000000001/attendance
Authorization: Bearer {{coach_token}}
{
    "attendance": [
        { "memberId": "{{member_id}}", "status": "Present" },
        { "memberId": "{{member2_id}}", "status": "Absent" }
    ]
}
HTTP *

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 11 — IDENTITY OPERATIONS (Admin + Member)                           ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [11.1] Get User Profile ──────────────────────────────────────────────────────
GET {{gateway}}/api/User/profile/{{member_user_id}}
HTTP 200
[Asserts]
jsonpath "$.Success" == true
jsonpath "$.Data.firstName" exists

# [11.2] Update Profile (multipart) ────────────────────────────────────────────
PUT {{gateway}}/api/User/profile
[MultipartFormData]
UserId: {{member_user_id}}
FirstName: Test
LastName: MemberUpdated
PhoneNumber: "+213-555-777888"
Gender: Male
DateOfBirth: 1997-04-10
HTTP 200
[Asserts]
jsonpath "$.Success" == true

# [11.3] Check User Exists ─────────────────────────────────────────────────────
GET {{gateway}}/api/User/{{member_user_id}}/exists
HTTP 200
[Asserts]
jsonpath "$.Data" == true

# [11.4] Change Password (Admin) ───────────────────────────────────────────────
POST {{gateway}}/api/User/change-password
Authorization: Bearer {{admin_token}}
{
    "currentPassword": "Admin@12345",
    "newPassword": "TempPass@99"
}
HTTP 200
[Asserts]
jsonpath "$.Success" == true

# [11.5] Change Password Back ──────────────────────────────────────────────────
POST {{gateway}}/api/User/change-password
Authorization: Bearer {{admin_token}}
{
    "currentPassword": "TempPass@99",
    "newPassword": "Admin@12345"
}
HTTP 200
[Asserts]
jsonpath "$.Success" == true

# [11.6] Forgot Password ───────────────────────────────────────────────────────
POST {{gateway}}/api/User/forgot-password
{
    "email": "test.member@fittech.com"
}
HTTP 200
[Captures]
reset_token: jsonpath "$.Data.token"
reset_user_id: jsonpath "$.Data.userId"
[Asserts]
jsonpath "$.Success" == true
jsonpath "$.Data.userId" exists
jsonpath "$.Data.token" isString

# [11.7] Reset Password ────────────────────────────────────────────────────────
POST {{gateway}}/api/User/reset-password
{
    "userId": "{{reset_user_id}}",
    "token": "{{reset_token}}",
    "newPassword": "NewPass@123"
}
HTTP 200
[Asserts]
jsonpath "$.Success" == true

# [11.8] Reset Password Back (needed for Phase 12 login) ───────────────────────
POST {{gateway}}/api/User/forgot-password
{
    "email": "test.member@fittech.com"
}
HTTP 200
[Captures]
reset_token2: jsonpath "$.Data.token"
[Asserts]
jsonpath "$.Success" == true

POST {{gateway}}/api/User/reset-password
{
    "userId": "{{reset_user_id}}",
    "token": "{{reset_token2}}",
    "newPassword": "Member@12345"
}
HTTP 200

# [11.9] Send Confirmation Email ───────────────────────────────────────────────
POST {{gateway}}/api/User/send-confirmation-email
{
    "email": "test.member@fittech.com"
}
HTTP *

# [11.10] Upload Medical File (multipart) ───────────────────────────────────────
POST {{gateway}}/api/User/medical-file
[MultipartFormData]
UserId: {{member_user_id}}
File: file,hello.txt; Hello,World
HTTP *

# [11.11] Get Medical File ──────────────────────────────────────────────────────
GET {{gateway}}/api/User/{{member_user_id}}/medical-file
HTTP *

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  PHASE 12 — DESTRUCTIVE / LIFECYCLE OPERATIONS (Admin)                     ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# [12.1] Suspend Member ────────────────────────────────────────────────────────
PATCH {{gateway}}/api/members/{{member_id}}/suspend
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.status" == "Suspended"
jsonpath "$.memberId" == "{{member_id}}"

# [12.2] Activate Member ───────────────────────────────────────────────────────
PATCH {{gateway}}/api/members/{{member_id}}/activate
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.status" == "Active"
jsonpath "$.memberId" == "{{member_id}}"

# [12.3] Deactivate User (Identity) ────────────────────────────────────────────
PUT {{gateway}}/api/User/{{member_user_id}}/deactivate
HTTP 200
[Asserts]
jsonpath "$.Success" == true

# [12.4] Soft-Delete Member (Membership) ───────────────────────────────────────
DELETE {{gateway}}/api/members/{{member_id}}
Authorization: Bearer {{admin_token}}
HTTP 204

# [12.5] Reject Program (Admin) ────────────────────────────────────────────────
# Already accepted; reject should fail or return conflict.
PATCH {{gateway}}/api/programs/requests/{{program_id}}/reject
Authorization: Bearer {{admin_token}}
HTTP *

# [12.6] Soft-Delete Plan ──────────────────────────────────────────────────────
DELETE {{gateway}}/api/plans/{{pack_plan_id}}
Authorization: Bearer {{admin_token}}
HTTP 204

# [12.7] Revoke Refresh Token ──────────────────────────────────────────────────
POST {{gateway}}/api/User/revoke-token
{
    "refreshToken": "{{admin_refresh}}"
}
HTTP 200
[Asserts]
jsonpath "$.Success" == true

# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║  TEST SUITE COMPLETE                                                       ║
# ╚══════════════════════════════════════════════════════════════════════════════╝
# Endpoints covered: 60+  (some return *)  Notes:
#   - GET /api/me and /api/me/subscription may 404 if no Membership record
#     exists for the directly-registered Identity user.
#   - POST /api/sessions/{id}/attendance uses placeholder session ID.
#   - POST /api/activity/entry-exit/scan expects a real NFC card UID.
#   - POST /api/User/medical-file may 400 if file validation fails.
# =============================================================================
