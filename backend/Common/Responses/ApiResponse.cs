namespace Resturant_Backend.Common.Responses;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            Error = null
        };
    }

    public static ApiResponse<T> FailureResponse(int statusCode, string message, IDictionary<string, List<string>>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Data = default,
            Message = null,
            Error = new ApiError(statusCode, message, errors)
        };
    }
}

/// <summary>
/// Non-generic response for operations without data
/// </summary>
public static class ApiResponse
{
    public static object Success(string? message = null)
    {
        return new { IsSuccess = true, Message = message };
    }

    public static object Failure(int statusCode, string message, IDictionary<string, List<string>>? errors = null)
    {
        return new
        {
            IsSuccess = false,
            Error = new ApiError(statusCode, message, errors)
        };
    }
}