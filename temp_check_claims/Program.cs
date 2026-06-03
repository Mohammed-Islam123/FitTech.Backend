using System.IdentityModel.Tokens.Jwt;
var map = JwtSecurityTokenHandler.DefaultInboundClaimTypeMap;
System.Console.WriteLine($"Map count: {map.Count}");
if (map.TryGetValue("role", out var rct))
    System.Console.WriteLine($"role -> {rct}");
else
    System.Console.WriteLine("role -> NOT MAPPED");
if (map.TryGetValue("sub", out var sct))
    System.Console.WriteLine($"sub -> {sct}");
else
    System.Console.WriteLine("sub -> NOT MAPPED");
