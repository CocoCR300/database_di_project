namespace Restify.API.Models;

public class Lodging
{
	public uint		Id { get; set; }
	public uint		OwnerId { get; set; }
    public string	Address { get; set; }
    public string	Description { get; set; }
    public LodgingType Type { get; set; }
	public string	Name { get; set; }
	public string	EmailAddress { get; set; }

	public IList<LodgingPhoneNumber>	PhoneNumbers { get; }
	public IList<LodgingPhoto>			Photos { get; }
	public IList<Perk>					Perks { get; }
	public IList<Room>					Rooms { get; }
	public IList<RoomType>				RoomTypes { get; }
	public Person						Owner { get; }

	public Lodging()
	{
		Perks = new List<Perk>(0);
	}
}

public enum LodgingType
{
	Apartment,
	GuestHouse,
	Hotel,
	Lodge,
	Motel,
	VacationRental
}
