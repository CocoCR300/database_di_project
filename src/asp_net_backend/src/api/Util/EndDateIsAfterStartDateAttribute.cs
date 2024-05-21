using System.ComponentModel.DataAnnotations;
using Restify.API.Models;

namespace Restify.API.Util
{
	public class EndDateIsAfterStartDateAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			if (value == null)
			{
				return ValidationResult.Success;
			}

			dynamic roomBookingData = value;

			if (roomBookingData.EndDate.CompareTo(roomBookingData.StartDate) > 0)
			{
				return ValidationResult.Success;
			}
			
			return new ValidationResult($"{nameof(RoomBooking.EndDate)} must be after {nameof(RoomBooking.StartDate)}.");
		}
	}
}
