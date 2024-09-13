﻿namespace Restify.API.Models;

public class Payment
{
	public DateTimeOffset	DateAndTime { get; set; }
	public decimal			Amount { get; set; }
	public int?				BookingId { get; set; }
	public int				Id { get; set; }
	public int				PaymentInformationId { get; set; }
}
