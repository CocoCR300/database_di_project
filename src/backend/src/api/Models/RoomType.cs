namespace Restify.API.Models;

public class RoomType
{
	public decimal	Fees { get; set; }
	public decimal	PerNightPrice { get; set; }
	public int		Capacity { get; set; }
    public int     Id { get; set; }
    public int     LodgingId { get; set; }
    public string   Name { get; set; }
    
    public List<RoomTypePhoto>	Photos { get; }
}