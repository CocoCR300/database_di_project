namespace Restify.API.Models;

public class Room
{
	public bool		Occupied { get; set; }
	public decimal	PerNightPrice { get; set; }
	public uint		Capacity { get; set; }
	public uint		LodgingId { get; set; }
	public uint		Number { get; set; }

	public Lodging	Lodging { get; }
}