using Restify.API.Data;

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Restify.API.Util
{
	public class UniqueAttribute<T> : ValidationAttribute where T : class
	{
		public string? PropertyName { get; init; }

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}

			string propertyName;
			if (!string.IsNullOrEmpty(PropertyName))
			{
				propertyName = PropertyName;
			}
			else
			{
				propertyName = validationContext.MemberName;
			}

			ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
			Expression	property = Expression.Property(parameter, typeof(T), propertyName),
						equals = Expression.Equal(property, Expression.Constant(value));
			Expression<Func<T, bool>> predicate = (Expression<Func<T, bool>>) Expression.Lambda(equals, parameter);
			
			RestifyDbContext context = validationContext.GetRequiredService<RestifyDbContext>();
			T? entity = context.Set<T>().Where(predicate).FirstOrDefault();
			if (entity == null)
			{
				return ValidationResult.Success;
			}
			
			return new ValidationResult(ErrorMessage);
		}
	}
}
