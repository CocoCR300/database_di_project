namespace Restify.API.Models;

public class Room
{
	public int	LodgingId { get; set; }
	public uint	Number { get; set; }
	public int	TypeId { get; set; }

	public Lodging	Lodging { get; }
	public RoomType	Type { get; }
	
	public Room() {}

	public Room(RoomType type)
	{
		 Type = type;
	}
}