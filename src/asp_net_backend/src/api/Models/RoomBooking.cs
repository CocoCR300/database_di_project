namespace Restify.API.Models;

public class RoomBooking
{
	
	public decimal	Cost { get; set; }
	public decimal	Fees { get; set; }
	public uint		BookingId { get; set; }
	public uint		LodgingId { get; set; } // TODO: Does not exist in the database table
	public uint		Id { get; set; }
	public uint		RoomNumber { get; set; }

	public Booking	Booking { get; }
	public Room		Room { get; }
}
