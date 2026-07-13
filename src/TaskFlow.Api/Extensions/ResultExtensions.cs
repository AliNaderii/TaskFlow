using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Common;

namespace TaskFlow.Api.Extensions;

public static class ResultExtensions
{   
    public static IActionResult ToProblemDetails(this Error error)
    {
        var statusCode = GetStatusCode(error.Code);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = error.Message
        };

        problemDetails.Extensions["code"] = error.Code;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static int GetStatusCode(string code)
    {
        if (code.Contains("NotFound"))
        {
            return StatusCodes.Status404NotFound;
        }
        
        if (code.Contains("AlreadyExists")
            || code.Contains("AlreadyArchived")
            || code.Contains("AlreadyActive"))
        {
            return StatusCodes.Status409Conflict;
        }

        return StatusCodes.Status400BadRequest;
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            404 => "Resource not found.",
            409 => "Conflict.",
            _ => "Validation error."
        };
    }
}