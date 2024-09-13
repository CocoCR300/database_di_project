namespace Restify.API.Models;

public class PaymentInformation
{
	public DateTime			CardExpiryDate { get; set; }
	public string			CardHolderName { get; set; }
	public string			CardNumber { get; set; }
	public string			CardSecurityCode { get; set; }
	public int				PersonId { get; set; }
	public int				Id { get; set; }

	public PaymentInformation()
	{
		CardNumber = CardSecurityCode = CardHolderName = string.Empty;
	}
}
