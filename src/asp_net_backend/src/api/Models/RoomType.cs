namespace Restify.API.Models;

public class RoomType
{
	public decimal	Fees { get; set; }
	public decimal	PerNightPrice { get; set; }
	public uint		Capacity { get; set; }
    public uint     Id { get; set; }
    public uint     LodgingId { get; set; }
    public string   Name { get; set; }
    
    public IList<RoomTypePhoto>	Photos { get; }

    public RoomType()
    {
	    Photos = new List<RoomTypePhoto>(0);
    }
}