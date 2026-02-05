namespace WorkManagement.API.Models;

/// <summary>
/// Standard API response wrapper for consistent response format
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

/// <summary>
/// Detailed error response for exceptions
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public string? ExceptionType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Path { get; set; }
    public string? TraceId { get; set; }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PaginatedResponse<T>
{
    public bool Success { get; set; } = true;
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
