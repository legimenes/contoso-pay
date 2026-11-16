using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Gateway.API.OpenApi;

internal sealed class BearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "OAuth 2.0 client credentials token from POST /connect/token."
        };
        return Task.CompletedTask;
    }
}