using System.ComponentModel.DataAnnotations;

namespace Restify.API.Util;

public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is DateOnly dateOnly)
        {
        }
        else if (value is DateTime dateTime)
        {
            dateOnly = DateOnly.FromDateTime(dateTime);
        }
        else if (value is DateTimeOffset dateTimeOffset)
        {
            dateOnly = DateOnly.FromDateTime(dateTimeOffset.DateTime);
        }
        else
        {
            throw new ArgumentException(
                "The value parameter is not an instance of the DateOnly, DateTime or DateTimeOffset classes.");
        }

        if (dateOnly.CompareTo(DateOnly.FromDateTime(DateTime.Now)) > 0)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"{validationContext.MemberName} has to be a date in the future.");
    }
}