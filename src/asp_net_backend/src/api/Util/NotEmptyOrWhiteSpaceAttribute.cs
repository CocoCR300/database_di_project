using Restify.API.Data;

using System.ComponentModel.DataAnnotations;

namespace Restify.API.Util
{
	public class NotEmptyOrWhiteSpaceAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}
			
			if (string.IsNullOrWhiteSpace(value as string))
			{
				return new ValidationResult($"The {validationContext.MemberName} field cannot be empty");
			}

			return ValidationResult.Success;
		}
	}
}
