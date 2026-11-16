using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Gateway.API.OpenApi;

internal sealed class BearerSecurityOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        bool requiresAuthorization = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();
        if (!requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = []
        });
        return Task.CompletedTask;
    }
}