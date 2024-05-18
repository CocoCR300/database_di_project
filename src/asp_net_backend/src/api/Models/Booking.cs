namespace Restify.API.Models;

public class Booking
{
	public uint		Id { get; set; }
	public string?	Status { get; set; }
	public uint		CustomerId { get; set; }
	public uint		LodgingId { get; set; }

	public IList<RoomBooking>	RoomBookings { get; }
	public Payment				Payment { get; set; }
	public Person				Customer { get; }
	public Lodging				Lodging { get; }
}

public enum BookingStatus
{
	Created,
	Confirmed,
	Cancelled,
	Finished
}
