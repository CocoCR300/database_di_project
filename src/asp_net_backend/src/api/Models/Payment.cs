namespace Restify.API.Models;

public class Payment
{
	public DateTimeOffset	DateAndTime { get; set; }
	public uint				BookingId { get; set; }
	public uint				Id { get; set; }
}
