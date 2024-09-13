namespace Restify.API.Models;

public class Lodging
{
	public int		Id { get; set; }
	public int		OwnerId { get; set; }
    public string	Address { get; set; }
    public string	Description { get; set; }
    public LodgingType Type { get; set; }
	public string	Name { get; set; }
	public string	EmailAddress { get; set; }

	public List<LodgingPhoneNumber>	PhoneNumbers { get; }
	public List<LodgingPhoto>			Photos { get; }
	public List<Perk>					Perks { get; }
	public List<Room>					Rooms { get; }
	public List<RoomType>				RoomTypes { get; }
	public Person						Owner { get; }
	
	public Lodging() {}

	public Lodging(List<Room>? rooms, List<RoomType>? roomTypes)
	{
		Rooms = rooms;
		RoomTypes = roomTypes;
	}

	public static bool OffersRooms(Lodging lodging)
	{
        return TypeOffersRooms(lodging.Type);
	}
	
	public static bool TypeOffersRooms(LodgingType type)
	{
        return type is not LodgingType.Apartment and not LodgingType.VacationRental;
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
