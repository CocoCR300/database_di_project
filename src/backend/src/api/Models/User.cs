namespace Restify.API.Models;

public class User
{
	public int		RoleId { get; set; }
	public string   Name { get; set; }
	public string   Password { get; set; }

	public Person	Person { get; set; }
	public UserRole	Role { get; set; }

	public static object MergeForResponse(User user, Person person)
	{
		string[] phoneNumbers = person.PhoneNumbers.Select(p => p.Number).ToArray();
		
		return new
		{
			UserName = user.Name,
			user.RoleId,
			PersonId = person.Id,
			person.FirstName,
			person.LastName,
			person.EmailAddress,
			PhoneNumbers = phoneNumbers
		};
	}

	public static Dictionary<string, User> DatabaseUsers = new User[]
	{
		new User
		{
			Name = "restify_administrator",
			Password = "SuperSecretAdministratorPasswordThatShouldNotBePushedToGitHub",
			RoleId = UserRole.Administrator,
			Person = new Person
			{
				Id = -1,
				UserName = "restify_administrator",
				FirstName = "Administrator",
				LastName = "Restify",
				EmailAddress = "administrator@restify.com"
			},
			Role = new UserRole
			{
				Id = UserRole.Administrator,
				Type = "Administrator"
			}
		},
		new User
		{
			Name = "restify_employee",
			Password = "SuperSecretEmployeePasswordThatShouldNotBePushedToGitHub",
			RoleId = UserRole.Lessor,
			Person = new Person
			{
				Id = 0,
				UserName = "restify_employee",
				FirstName = "Employee",
				LastName = "Restify",
				EmailAddress = "employee@restify.com"
			},
			Role = new UserRole
			{
				Id = UserRole.Lessor,
				Type = "Lessor"
			}
		}
	}.ToDictionary(u => u.Name, u => u);
}
