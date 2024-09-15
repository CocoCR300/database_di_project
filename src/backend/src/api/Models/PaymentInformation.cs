namespace Restify.API.Models;

public class PaymentInformation
{
	public DateOnly			CardExpiryDate { get; set; }
	public int				Id { get; set; }
	public int				PersonId { get; set; }
	public string			CardHolderName { get; set; }
	public string			CardNumber { get; set; }
	public string			CardSecurityCode { get; set; }

	public PaymentInformation()
	{
		CardNumber = CardSecurityCode = CardHolderName = string.Empty;
	}
}
