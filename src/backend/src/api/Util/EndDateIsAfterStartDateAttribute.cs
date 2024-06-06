using System.ComponentModel.DataAnnotations;
using Restify.API.Controllers;
using Restify.API.Models;

namespace Restify.API.Util
{
	public class EndDateIsAfterStartDateAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			IRoomBookingRequestData roomBookingData = (IRoomBookingRequestData) value;
			if (!roomBookingData.StartDate.HasValue || !roomBookingData.EndDate.HasValue)
			{
				return ValidationResult.Success;
			}

			DateOnly	startDate = roomBookingData.StartDate.Value,
						endDate = roomBookingData.EndDate.Value;
			if (endDate.CompareTo(startDate) > 0)
			{
				return ValidationResult.Success;
			}
			
			return new ValidationResult($"{nameof(RoomBooking.EndDate)} must be after {nameof(RoomBooking.StartDate)}.");
		}
	}
}
