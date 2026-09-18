using Resturant_Backend.Common.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Resturant_Backend.Common.Helpers;

public static class Ensure
{
    public static void Check(
        bool condition,
        string message,
        [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if(!condition)
            throw new BadRequestException(string.IsNullOrWhiteSpace(message)
                ? $"Condition failed: {paramName}"
                : message);
    }

    public static void NotNull(
        [NotNull] object? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if(value is null)
            throw new NotFoundException(string.IsNullOrWhiteSpace(message)
                ? $"{paramName} was not found."
                : message!);
    }

    public static void NotNullOrEmpty(
        [NotNull] string? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if(string.IsNullOrEmpty(value))
            throw new BadRequestException(string.IsNullOrWhiteSpace(message)
                ? $"{paramName} cannot be null or empty."
                : message!);
    }

    public static void Unauthorized(
        [NotNull] object? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if(value is null)
            throw new UnauthorizedException(string.IsNullOrWhiteSpace(message)
                ? $"{paramName} is null or not authenticated."
                : message!);
    }

    [DoesNotReturn]
    public static void BadRequest(string message)
        => throw new BadRequestException(message);

    [DoesNotReturn]
    public static void Forbidden(string message = "ليس لديك صلاحية.")
        => throw new ForbiddenException(message);

    [DoesNotReturn]
    public static void Conflict(string message)
        => throw new ConflictException(message);

    [DoesNotReturn]
    public static void NotFound(string message)
        => throw new NotFoundException(message);

    [DoesNotReturn]
    public static void FieldError(string fieldName, string errorMessage, string mainMessage = "خطأ في البيانات المدخلة.")
    {
        var errors = new Dictionary<string, List<string>>
        {
            { fieldName.ToCamelCase(), new List<string> { errorMessage } }
        };
        throw new ValidationException(errors, mainMessage);
    }

    [DoesNotReturn]
    public static void FieldError<TModel, TProperty>(
        Expression<Func<TModel, TProperty>> propertyLambda,
        string errorMessage,
        string mainMessage = "خطأ في البيانات المدخلة.")
    {
        var member = GetMemberExpression(propertyLambda);
        string propertyName = member.Member.Name;
        throw new ValidationException(
            new Dictionary<string, List<string>>
            {
                { propertyName.ToCamelCase(), new List<string> { errorMessage } }
            },
            mainMessage);
    }

    [DoesNotReturn]
    public static void Validation(IDictionary<string, List<string>> errors, string mainMessage = "خطأ في البيانات المدخلة.")
    {
        if(errors == null || errors.Count == 0)
            throw new ArgumentException("Errors dictionary cannot be null or empty", nameof(errors));

        throw new ValidationException(
            errors.ToDictionary(kvp => kvp.Key.ToCamelCase(), kvp => kvp.Value),
            mainMessage);
    }

    #region Private Helpers

    private static MemberExpression GetMemberExpression<TModel, TProperty>(
        Expression<Func<TModel, TProperty>> propertyLambda)
    {
        if(propertyLambda.Body is MemberExpression member)
            return member;

        if(propertyLambda.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            return unaryMember;

        throw new ArgumentException("Expression must refer to a property.", nameof(propertyLambda));
    }

    #endregion
}