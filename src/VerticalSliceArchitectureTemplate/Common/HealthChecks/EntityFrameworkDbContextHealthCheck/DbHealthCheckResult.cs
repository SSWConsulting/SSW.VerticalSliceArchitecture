namespace VerticalSliceArchitectureTemplate.Common.HealthChecks.EntityFrameworkDbContextHealthCheck;

public sealed class DbHealthCheckResult
{
    public bool Success { get; }
    public Exception? Exception { get; }
    public string? Message { get; } = string.Empty;

    public DbHealthCheckResult(string successMessage)
    {
        Success = true;
        Message = successMessage;
    }

    public DbHealthCheckResult(string failureMessage, Exception? exception)
    {
        Success = false;
        Message = failureMessage;
        Exception = exception;
    }
}