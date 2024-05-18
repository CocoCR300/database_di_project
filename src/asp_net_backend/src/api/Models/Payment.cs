namespace Restify.API.Models;

public class Payment
{
	public DateTimeOffset	DateAndTime { get; set; }
	public decimal			Amount { get; set; }
	public string			InvoiceImageFileName { get; set; }
	public uint				BookingId { get; set; }
	public uint				Id { get; set; }
}
