namespace Restify.API.Models;

public class Lodging
{
	public uint		Id { get; set; }
	public uint		OwnerId { get; set; }
    public string	Address { get; set; }
    public string	Description { get; set; }
    public string	LodgingType { get; set; }
	public string	Name { get; set; }

	public IList<Room>	Rooms { get; }
	public Person		Owner { get; }
}
