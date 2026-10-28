using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Communication.Responses.Errors;

public class ErrorResponse : ProblemDetails
{
    public IDictionary<string, string[]>? Errors { get; set; }
}
