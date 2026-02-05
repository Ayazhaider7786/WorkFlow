using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace WorkManagement.API.Filters;

/// <summary>
/// Action filter that logs API requests and responses for documentation purposes
/// </summary>
public class ApiResponseDocumentationFilter : IAsyncActionFilter
{
    private readonly ILogger<ApiResponseDocumentationFilter> _logger;

    public ApiResponseDocumentationFilter(ILogger<ApiResponseDocumentationFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var actionName = context.ActionDescriptor.DisplayName;
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        _logger.LogInformation("API Request: {Method} {Path} - Action: {Action}", httpMethod, path, actionName);

        var result = await next();
        
        stopwatch.Stop();

        if (result.Exception != null && !result.ExceptionHandled)
        {
            _logger.LogError(result.Exception, "API Error: {Method} {Path} - Duration: {Duration}ms", 
                httpMethod, path, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            var statusCode = (result.Result as ObjectResult)?.StatusCode ?? 200;
            _logger.LogInformation("API Response: {Method} {Path} - Status: {Status} - Duration: {Duration}ms",
                httpMethod, path, statusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}

/// <summary>
/// Attribute to document expected response types for an API endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ApiResponseTypeAttribute : Attribute
{
    public int StatusCode { get; }
    public Type? ResponseType { get; }
    public string Description { get; }

    public ApiResponseTypeAttribute(int statusCode, string description, Type? responseType = null)
    {
        StatusCode = statusCode;
        Description = description;
        ResponseType = responseType;
    }
}

/// <summary>
/// Common response type attributes for reuse
/// </summary>
public static class CommonResponses
{
    public const string Success = "Operation completed successfully";
    public const string Created = "Resource created successfully";
    public const string BadRequest = "Invalid request parameters";
    public const string Unauthorized = "Authentication required";
    public const string Forbidden = "Access denied";
    public const string NotFound = "Resource not found";
    public const string ServerError = "Internal server error";
}
