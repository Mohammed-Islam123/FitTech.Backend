---
type: "code_change"
date: "2026-05-31T09:38:51.494264+00:00"
question: "Add fixed member seed Member@fitteck.com with password Member@12345 in DataSeeder.cs"
contributor: "graphify"
source_nodes: ["DataSeeder.cs", "Member"]
---

# Q: Add fixed member seed Member@fitteck.com with password Member@12345 in DataSeeder.cs

## Answer

Added fixed member user in DataSeeder.cs:93-104 alongside Admin. UserMember@fitteck.com with password Member@12345 is seeded into Member role during startup.

## Source Nodes

- DataSeeder.cs
- Member