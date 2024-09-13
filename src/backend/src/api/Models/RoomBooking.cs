namespace Restify.API.Models;

public class RoomBooking
{
	public DateOnly			StartDate { get; set; }
	public DateOnly			EndDate { get; set; }
	public decimal			Cost { get; set; }
	public decimal			Discount { get; set; }
	public decimal			Fees { get; set; }
	public BookingStatus	Status { get; set; }
	public int				BookingId { get; set; }
	public int				LodgingId { get; set; }
	public int				Id { get; set; }
	public uint				RoomNumber { get; set; }

	public Booking	Booking { get; }
	public Room		Room { get; }
}
