using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
    {
        var authSchemes = await authProvider.GetAllSchemesAsync();
        if (authSchemes.Any(s => s.Name == "Bearer"))
        {
            // Fix 1: Properly initialize the components if null
            doc.Components ??= new OpenApiComponents();

            // Fix 2: Initialize or fetch the security scheme map using the correct interface abstraction
            doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            // Define your scheme
            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Paste your JWT Bearer token directly into the input field."
            };

            // Safely assign it to the dictionary interface
            doc.Components.SecuritySchemes["Bearer"] = bearerScheme;

            // Fix 3: Explicitly type the list to resolve the string[] implicit mapping error
            var securityRequirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
            };

            doc.Security ??= new List<OpenApiSecurityRequirement>();
            doc.Security.Add(securityRequirement);
        }
    }
}
