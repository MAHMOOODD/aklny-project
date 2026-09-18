using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Exceptions;
using Resturant_Backend.Common.Responses;
using System.Diagnostics.CodeAnalysis;

public static class ControllerExtensions
{
    #region Exception Throwers
    [DoesNotReturn]
    public static void BadRequestEx(this ControllerBase controller, string message)
        => throw new BadRequestException(message);

    [DoesNotReturn]
    public static void NotFoundEx(this ControllerBase controller, string message)
        => throw new NotFoundException(message);

    [DoesNotReturn]
    public static void UnauthorizedEx(this ControllerBase controller, string message = "غير مصرح لك بالوصول.")
        => throw new UnauthorizedException(message);

    [DoesNotReturn]
    public static void ForbiddenEx(this ControllerBase controller, string message = "ليس لديك صلاحية.")
        => throw new ForbiddenException(message);

    [DoesNotReturn]
    public static void ConflictEx(this ControllerBase controller, string message)
        => throw new ConflictException(message);

    [DoesNotReturn]
    public static void ValidationEx(this ControllerBase controller, IDictionary<string, List<string>> errors, string message = "حدثت أخطاء في البيانات المدخلة.")
        => throw new ValidationException(errors, message);
    #endregion

    #region Success Responses
    public static IActionResult Success<T>(this ControllerBase controller, T data, string? message = null)
        => controller.Ok(ApiResponse<T>.SuccessResponse(data, message));

    public static IActionResult SuccessMessage(this ControllerBase controller, string message)
        => controller.Ok(ApiResponse<object>.SuccessResponse(null, message));
    // ✅ Data = null, Message = message

    public static IActionResult Success(this ControllerBase controller)
        => controller.Ok(ApiResponse<object>.SuccessResponse(null));
    // ✅ Data = null, Message = null
    #endregion


}