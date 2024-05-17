namespace Restify.API.Models;

public class User
{
	public uint		RoleId { get; set; }
	public string   Name { get; set; }
	public string   Password { get; set; }

	public Person	Person { get; set; }
	public UserRole	Role { get; set; }
}
