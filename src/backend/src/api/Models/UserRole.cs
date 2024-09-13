namespace Restify.API.Models;

public class UserRole
{
	public const int Administrator	= 1;
	public const int Customer		= 2;
	public const int Lessor		= 3;
	
	public int		Id { get; set; }
	public string	Type { get; set; }
}