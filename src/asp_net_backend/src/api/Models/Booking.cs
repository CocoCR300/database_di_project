namespace Restify.API.Models;

public class Booking
{
	public DateTimeOffset	StartDate { get; set; }
	public DateTimeOffset	EndDate { get; set; }
	public string			Status { get; set; }
	public uint				BookingId { get; set; }
	public uint				CustomerId { get; set; }
	public uint				LodgingId { get; set; }
	public uint				PaymentId { get; set; }

	public IList<RoomBooking>	RoomBookings { get; }
	public Payment				Payment { get; set; }
	public Person				Customer { get; }
	public Lodging				Lodging { get; }
}
