#r "nuget: Microsoft.IdentityModel.Tokens, 8.*"
#r "nuget: System.IdentityModel.Tokens.Jwt, 8.*"

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

var map = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap;
Console.WriteLine($"Map count: {map.Count}");
if (map.TryGetValue("role", out var roleClaimType))
    Console.WriteLine($"role -> {roleClaimType}");
else
    Console.WriteLine("role -> NOT MAPPED");
if (map.TryGetValue("sub", out var subClaimType))
    Console.WriteLine($"sub -> {subClaimType}");
else
    Console.WriteLine("sub -> NOT MAPPED");

Console.WriteLine("\nFirst 10 mappings:");
var count = 0;
foreach (var kv in map)
{
    if (count++ >= 10) break;
    Console.WriteLine($"  {kv.Key} -> {kv.Value}");
}
