﻿namespace Restify.API.Models;

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
}
