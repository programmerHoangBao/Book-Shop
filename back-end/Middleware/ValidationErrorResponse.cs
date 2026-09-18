using back_end.DTOs;
using back_end.Records;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Middleware
{
    public class ValidationErrorResponse
    {
        public static IActionResult Create(ActionContext context)
        {
            var errorMessages = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Invalid request!."
                        : error.ErrorMessage
                )
                .ToList();

            var response = ApiResponse<List<string>>.Response(
                errorRecord: ErrorRecord.RequestInvalid,
                data: errorMessages
            );

            return new BadRequestObjectResult(response);
        }
    }
}
}
