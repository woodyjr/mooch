using Microsoft.AspNetCore.Http;

namespace Mooch.Api.Infrastructure.Http;

public sealed class Result<T>
{
    private Result(int statusCode, T? value, string? error, IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        StatusCode = statusCode;
        Value = value;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public int StatusCode { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    public static Result<T> Success(T value, int statusCode = StatusCodes.Status200OK) =>
        new(statusCode, value, null, null);

    public static Result<T> Failure(int statusCode, string error) =>
        new(statusCode, default, error, null);

    public static Result<T> Validation(IReadOnlyDictionary<string, string[]> validationErrors) =>
        new(StatusCodes.Status400BadRequest, default, null, validationErrors);
}
