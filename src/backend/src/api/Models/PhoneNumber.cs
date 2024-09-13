namespace Restify.API.Models;

public class PhoneNumber
{
	public string	Number { get; set; }
}

public class LodgingPhoneNumber : PhoneNumber
{
	public int LodgingId { get; set; }
}

public class PersonPhoneNumber : PhoneNumber
{
	public int PersonId { get; set; }
}
