namespace Restify.API.Models;

public class Person
{
	public uint		Id { get; set; }
	public string	UserName { get; set; }
	public string	FirstName { get; set; }
	public string	LastName { get; set; }
	public string	EmailAddress { get; set; }

	public IList<PersonPhoneNumber>	PhoneNumbers { get; }
	public User	User { get; }

	public Person()
	{
		PhoneNumbers = new List<PersonPhoneNumber>();
	}
}