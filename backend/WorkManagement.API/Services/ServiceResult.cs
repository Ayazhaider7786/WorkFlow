namespace WorkManagement.API.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Exception? Exception { get; private set; }
    public ServiceResultType ResultType { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            ResultType = ServiceResultType.Success
        };
    }

    public static ServiceResult<T> Created(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            ResultType = ServiceResultType.Created
        };
    }

    public static ServiceResult<T> Failure(string message, Exception? exception = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            Exception = exception,
            ResultType = ServiceResultType.Error
        };
    }

    public static ServiceResult<T> NotFound(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ResultType = ServiceResultType.NotFound
        };
    }

    public static ServiceResult<T> BadRequest(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ResultType = ServiceResultType.BadRequest
        };
    }

    public static ServiceResult<T> Unauthorized(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ResultType = ServiceResultType.Unauthorized
        };
    }

    public static ServiceResult<T> Forbidden(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ResultType = ServiceResultType.Forbidden
        };
    }
}

public enum ServiceResultType
{
    Success,
    Created,
    Error,
    NotFound,
    BadRequest,
    Unauthorized,
    Forbidden
}
