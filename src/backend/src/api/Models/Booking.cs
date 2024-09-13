namespace Restify.API.Models;

public class Booking
{
	public int				Id { get; set; }
	public int				CustomerId { get; set; }
	public int				LodgingId { get; set; }

	public List<RoomBooking>	RoomBookings { get; }
	public Payment?				Payment { get; set; }
	public Person				Customer { get; }
	public Lodging				Lodging { get; }

	public Booking() {}
	
	public Booking(List<RoomBooking> roomBookings)
	{
		RoomBookings = roomBookings;
	}
}

public enum BookingStatus
{
	Created,
	Confirmed,
	Cancelled,
	Finished
}
