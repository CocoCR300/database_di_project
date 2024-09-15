using System.ComponentModel.DataAnnotations;
using Restify.API.Controllers;

namespace Restify.API.Util
{
	public class BookingPatchRequestDataAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			BookingPatchRequestData bookingData = (BookingPatchRequestData) value;
			
			if (bookingData.RoomsBookingsToAdd != null && bookingData.RoomsBookingsToAdd.Length > 0
			    || bookingData.RoomsBookingsToDelete != null && bookingData.RoomsBookingsToDelete.Length > 0
			    || bookingData.RoomsBookingsToUpdate != null && bookingData.RoomsBookingsToUpdate.Length > 0)
			{
				return ValidationResult.Success;
			}
			
			return new ValidationResult(
				$"At least one of {nameof(BookingPatchRequestData.RoomsBookingsToAdd)}, " +
				$"{nameof(BookingPatchRequestData.RoomsBookingsToDelete)}" +
				$"or {nameof(BookingPatchRequestData.RoomsBookingsToUpdate)} must be provided to perform an update.");
		}
	}
}
