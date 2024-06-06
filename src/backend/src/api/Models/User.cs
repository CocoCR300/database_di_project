namespace Restify.API.Models;

public class User
{
	public uint		RoleId { get; set; }
	public string   Name { get; set; }
	public string   Password { get; set; }

	public Person	Person { get; set; }
	public UserRole	Role { get; set; }

	public static User WithoutPassword(User user)
	{
		return new User
		{
			RoleId = user.RoleId,
			Name = user.Name,
			Person = user.Person,
			Role = user.Role
		};
	}
}
