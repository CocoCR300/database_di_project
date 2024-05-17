using Restify.API.Data;

using System.ComponentModel.DataAnnotations;

namespace Restify.API.Util
{
	public class UniqueAttribute<T> : ValidationAttribute where T : class
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var context = validationContext.GetService<RestifyDbContext>();
			if (context.Find<T>(value) == null)
			{
				return ValidationResult.Success;
			}
			return new ValidationResult($"The specified {validationContext.MemberName} is already taken.");
		}
	}
}
