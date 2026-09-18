namespace Resturant_Backend.Common.Exceptions;

/// <summary>
/// Base exception class for all application-specific exceptions
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when a request is malformed or invalid (HTTP 400)
/// </summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, statusCode: 400) { }
}

/// <summary>
/// Thrown when authentication is required but not provided or invalid (HTTP 401)
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "غير مصرح لك بالوصول، يرجى تسجيل الدخول أولاً.")
        : base(message, statusCode: 401) { }
}

/// <summary>
/// Thrown when authenticated user lacks required permissions (HTTP 403)
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "ليس لديك الصلاحيات الكافية لإتمام هذا الإجراء.")
        : base(message, statusCode: 403) { }
}

/// <summary>
/// Thrown when a requested resource is not found (HTTP 404)
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, statusCode: 404) { }
}

/// <summary>
/// Thrown when a request conflicts with the current state (HTTP 409)
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, statusCode: 409) { }
}

/// <summary>
/// Thrown when validation fails for one or more fields (HTTP 400)
/// </summary>
public class ValidationException : AppException
{
    public IDictionary<string, List<string>> ValidationErrors { get; }

    public ValidationException(IDictionary<string, List<string>> errors, string message = "حدثت أخطاء في البيانات المدخلة.")
        : base(message, statusCode: 400)
    {
        ValidationErrors = errors ?? new Dictionary<string, List<string>>();
    }
}