# Test: Verify /api/members returns identity-enriched fields via gRPC + Redis
# Run: hurl test_members_list.hurl --variable gateway=http://localhost:5098

# 1. Login as admin to get JWT
POST {{gateway}}/api/User/login
Content-Type: application/json
{
  "emailOrUserName": "admin",
  "password": "Admin@12345",
  "clientId": "web"
}

HTTP 200
[Captures]
token: jsonpath "$.Data.Token"

# 2. Call members list endpoint
GET {{gateway}}/api/members?Page=1&PageSize=15
Authorization: Bearer {{token}}

HTTP 200
[Asserts]
jsonpath "$.items" count > 0
jsonpath "$.items[0].memberId" exists
jsonpath "$.items[0].firstName" exists
jsonpath "$.items[0].lastName" exists
jsonpath "$.items[0].email" exists
jsonpath "$.items[0].phoneNumber" exists
jsonpath "$.items[0].profilePhotoUrl" exists
jsonpath "$.items[0].status" exists
jsonpath "$.items[0].joinDate" exists
jsonpath "$.items[0].activePlanName" exists
jsonpath "$.totalCount" > 0
jsonpath "$.page" == 1
jsonpath "$.pageSize" == 15
