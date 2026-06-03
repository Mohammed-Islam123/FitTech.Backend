# =============================================================================
# Card Flow End-to-End Test
# Tests the complete NFC card scan flow: entry → verify → workout log → exit
#
# Prerequisites:
#   - All services running (via Aspire: dotnet run --project FitTech.AppHost)
#   - Seed data loaded (auto-seeded on first run in Development mode)
#   - Hurl installed (https://hurl.dev)
#
# Run:
#   hurl --test --variable GATEWAY=http://localhost:5098 tests/card-flow.hurl
#
# Seed data used:
#   - Admin:   admin / Admin@12345
#   - Member 50: seed.member50@fitteck.com / Member@12345 (card: CARD-0050)
# =============================================================================

# ============================================================
# Step 1: Admin logs in
# ============================================================
POST {{gateway}}/api/User/login
Content-Type: application/json
{
    "emailOrUserName": "admin",
    "password": "Admin@12345",
    "clientId": "web"
}
HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.succeeded" == true
jsonpath "$.data.token" exists
jsonpath "$.data.requiresTwoFactor" == false
[Captures]
admin_token: jsonpath "$.data.token"

# ============================================================
# Step 2: Admin resolves card CARD-0050 to a member
# ============================================================
GET {{gateway}}/api/members/by-card/CARD-0050
Authorization: Bearer {{admin_token}}
HTTP 200
[Asserts]
jsonpath "$.memberId" exists
jsonpath "$.firstName" exists
jsonpath "$.lastName" exists
jsonpath "$.status" == "Active"
[Captures]
member50_id: jsonpath "$.memberId"
member50_name: jsonpath "$.firstName"

# ============================================================
# Step 3: Admin scans card for ENTRY
# The Activity service should:
#   - Resolve the card to member 50
#   - Verify member is Active
#   - Verify member has an active subscription
#   - Verify remaining sessions > 0
#   - Track the entry (decrement sessions)
#   - Create a MemberActivity record
# ============================================================
POST {{gateway}}/api/activity/entry-exit/scan
Authorization: Bearer {{admin_token}}
Content-Type: application/json
{
    "cardUid": "CARD-0050"
}
HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.verificationType" == "Entering"
jsonpath "$.memberName" == "{{member50_name}}"
jsonpath "$.remainingSessions" exists

# ============================================================
# Step 4: Member 50 logs in
# ============================================================
POST {{gateway}}/api/User/login
Content-Type: application/json
{
    "emailOrUserName": "seed.member50@fitteck.com",
    "password": "Member@12345",
    "clientId": "web"
}
HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.data.succeeded" == true
jsonpath "$.data.token" exists
[Captures]
member50_token: jsonpath "$.data.token"

# ============================================================
# Step 5: Member 50 gets their profile (to get memberId for workout logs)
# ============================================================
GET {{gateway}}/api/me
Authorization: Bearer {{member50_token}}
HTTP 200
[Asserts]
jsonpath "$.memberId" exists
jsonpath "$.email" == "seed.member50@fitteck.com"
jsonpath "$.isActive" == true
[Captures]
profile_member_id: jsonpath "$.memberId"

# ============================================================
# Step 6: Member 50 checks their sessions
# Should see the active gym visit (no checkOutTime)
# ============================================================
GET {{gateway}}/api/me/sessions
Authorization: Bearer {{member50_token}}
HTTP 200
[Asserts]
jsonpath "$[0].sessionId" exists
jsonpath "$[0].checkInTime" exists
jsonpath "$[0].checkOutTime" == null
[Captures]
active_session_id: jsonpath "$[0].sessionId"

# ============================================================
# Step 7: Member 50 creates a workout log for this session
# ============================================================
POST {{gateway}}/api/workout-logs
Authorization: Bearer {{member50_token}}
Content-Type: application/json
{
    "memberId": "{{profile_member_id}}",
    "activitySessionId": "{{active_session_id}}",
    "notes": "Great workout! Test session for card flow validation.",
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
        },
        {
            "name": "Deadlift",
            "description": "3 sets of 5 reps at 120kg",
            "caloriesBurned": 130.0,
            "durationMinutes": 18
        }
    ]
}
HTTP 200
[Asserts]
jsonpath "$.id" exists
jsonpath "$.memberId" == "{{profile_member_id}}"
jsonpath "$.activitySessionId" == "{{active_session_id}}"
jsonpath "$.exercises" count == 3
jsonpath "$.exercises[0].name" == "Bench Press"
jsonpath "$.exercises[1].name" == "Squat"
jsonpath "$.exercises[2].name" == "Deadlift"
[Captures]
workout_log_id: jsonpath "$.id"

# ============================================================
# Step 8: Admin scans card for EXIT
# The Activity service should find the active session and mark checkout
# ============================================================
POST {{gateway}}/api/activity/entry-exit/scan
Authorization: Bearer {{admin_token}}
Content-Type: application/json
{
    "cardUid": "CARD-0050"
}
HTTP 200
[Asserts]
jsonpath "$.success" == true
jsonpath "$.verificationType" == "Exiting"
jsonpath "$.memberName" == "{{member50_name}}"

# ============================================================
# Step 9: Member 50 checks sessions again
# Should now see the session with a checkOutTime
# ============================================================
GET {{gateway}}/api/me/sessions
Authorization: Bearer {{member50_token}}
HTTP 200
[Asserts]
jsonpath "$[0].sessionId" == "{{active_session_id}}"
jsonpath "$[0].checkOutTime" exists
jsonpath "$[0].checkOutTime" isNotEqualTo null

# ============================================================
# Step 10: Member 50 fetches their workout log to verify it's saved
# ============================================================
GET {{gateway}}/api/workout-logs/{{workout_log_id}}
Authorization: Bearer {{member50_token}}
HTTP 200
[Asserts]
jsonpath "$.id" == "{{workout_log_id}}"
jsonpath "$.activitySessionId" == "{{active_session_id}}"
jsonpath "$.notes" == "Great workout! Test session for card flow validation."
jsonpath "$.exercises" count == 3

# ============================================================
# Step 11: Member 50 lists workout logs by activity session
# ============================================================
GET {{gateway}}/api/workout-logs?activitySessionId={{active_session_id}}
Authorization: Bearer {{member50_token}}
HTTP 200
[Asserts]
jsonpath "$" count >= 1
jsonpath "$[0].activitySessionId" == "{{active_session_id}}"
jsonpath "$[0].id" == "{{workout_log_id}}"


# =============================================================================
# TEST SUMMARY
# =============================================================================
# ✅ Step 1-2:  Admin authentication + card resolution
# ✅ Step 3:    Card scan for ENTRY → "Entering" response
# ✅ Step 4-5:  Member authentication + profile retrieval
# ✅ Step 6:    Session appears in member's session list (active, no checkout)
# ✅ Step 7:    Workout log created and linked to the gym session
# ✅ Step 8:    Card scan for EXIT → "Exiting" response
# ✅ Step 9:    Session now has checkOutTime
# ✅ Step 10-11: Workout log persists and is searchable
# =============================================================================
