using Microsoft.AspNetCore.Mvc;

namespace Mooch.Api.Infrastructure.Http;

public static class ControllerExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.ValidationErrors is not null)
        {
            var validationProblem = new ValidationProblemDetails(result.ValidationErrors.ToDictionary(
                entry => entry.Key,
                entry => entry.Value))
            {
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(validationProblem);
        }

        if (result.Value is not null)
        {
            return controller.StatusCode(result.StatusCode, result.Value);
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            return controller.StatusCode(result.StatusCode, result.Error);
        }

        return controller.StatusCode(result.StatusCode);
    }
}
