namespace Restify.API.Models;

public class UserRole
{
	public const uint Administrator	= 1;
	public const uint Customer		= 2;
	public const uint Lessor		= 3;
	
	public uint		Id { get; set; }
	public string	Type { get; set; }
}