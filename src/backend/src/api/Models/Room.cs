namespace Restify.API.Models;

public class Room
{
	public uint	LodgingId { get; set; }
	public uint	Number { get; set; }
	public uint	TypeId { get; set; }

	public Lodging	Lodging { get; }
	public RoomType	Type { get; }
	
	public Room() {}

	public Room(RoomType type)
	{
		 Type = type;
	}
}