---
type: "query"
date: "2026-05-23T09:43:56.285143+00:00"
question: "how we are handling the cross service token verification (auth) , who is verifying the token and are all teh services have teh same key"
contributor: "graphify"
source_nodes: ["JwtTokenService", "ITokenService", "Gateway"]
---

# Q: how we are handling the cross service token verification (auth) , who is verifying the token and are all teh services have teh same key

## Answer

Tokens are issued by Identity via JwtTokenService (HMAC-SHA256, symmetric key). Only Identity validates tokens correctly. Membership and Payment register AddJwtBearer() but configure NO signing key -- validation will silently fail (401). Gateway (YARP) is a pure pass-through with zero auth middleware. There is no shared key mechanism: only Identity has the key. Membership and Payment would need matching JwtSettings added to appsettings.json or injected via Aspire AppHost to work.

## Source Nodes

- JwtTokenService
- ITokenService
- Gateway