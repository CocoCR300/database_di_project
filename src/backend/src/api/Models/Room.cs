namespace Restify.API.Models;

public class Room
{
	public int	LodgingId { get; set; }
	public int	Number { get; set; }
	public int	TypeId { get; set; }

	public Lodging	Lodging { get; }
	public RoomType	Type { get; set; }
	
	public Room() {}

	public Room(RoomType type)
	{
		 Type = type;
	}
}