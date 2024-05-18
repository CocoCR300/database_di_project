namespace Restify.API.Models;

public class PhoneNumber
{
	public string	Number { get; set; }
}

public class LodgingPhoneNumber : PhoneNumber
{
	public uint LodgingId { get; set; }
}

public class PersonPhoneNumber : PhoneNumber
{
	public uint PersonId { get; set; }
}
