---
type: "query"
date: "2026-05-29T11:14:16.595804+00:00"
question: "can you check if these issues are valid in the yarp proxy configs and implementation : 1. gateway in AppHost.cs does not .WithReference(membershipApi) even though gateway.yaml routes use membership-cluster. If service discovery relies on references (as the identity URL lookup does), the gateway may not receive membership-api endpoints. 2. /api/courses is routed to membership-cluster while a dedicated courses-api exists and courses-cluster is configured for other routes—confirm this is intentional"
contributor: "graphify"
source_nodes: ["Gateway (YARP Reverse Proxy)", "gateway.yaml"]
---

# Q: can you check if these issues are valid in the yarp proxy configs and implementation : 1. gateway in AppHost.cs does not .WithReference(membershipApi) even though gateway.yaml routes use membership-cluster. If service discovery relies on references (as the identity URL lookup does), the gateway may not receive membership-api endpoints. 2. /api/courses is routed to membership-cluster while a dedicated courses-api exists and courses-cluster is configured for other routes—confirm this is intentional

## Answer

Issue 1 is valid at the config level: AppHost gateway omits WithReference(membershipApi) while gateway.yaml defines membership-cluster -> http+https://membership-api, and the gateway only references identity/payment/courses/activity/aggregation/chat. Issue 2 is consistent with current implementation: Membership exposes /api/courses endpoints and the handlers are stubbed; Courses service owns /api/coaches, /api/programs, /api/sessions.

## Source Nodes

- Gateway (YARP Reverse Proxy)
- gateway.yaml