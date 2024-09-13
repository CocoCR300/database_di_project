using Restify.API.Data;

using System.ComponentModel.DataAnnotations;

namespace Restify.API.Util
{
	public class ExistsAttribute<T> : ValidationAttribute where T : class
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}
			
			var context = validationContext.GetRequiredService<RestifyDbContext>();
			if (context.Find<T>(value) != null)
			{
				return ValidationResult.Success;
			}

			return new ValidationResult($"There is no {typeof(T).Name} with the specified {validationContext.MemberName}.");
		}
	}
}
