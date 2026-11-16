using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;

namespace Gateway.API.Responses;

public sealed class ApiProblemDetails : ProblemDetails
{
    public IReadOnlyCollection<Error>? Errors { get; init; }

    public string? TraceId { get; init; }

    public string? RequestId { get; init; }
}