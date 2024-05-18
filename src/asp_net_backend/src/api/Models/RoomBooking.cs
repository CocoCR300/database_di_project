﻿namespace Restify.API.Models;

public class RoomBooking
{
	public DateTimeOffset	StartDate { get; set; }
	public DateTimeOffset	EndDate { get; set; }
	public decimal			Cost { get; set; }
	public decimal			Fees { get; set; }
	public BookingStatus	Status { get; set; }
	public uint				BookingId { get; set; }
	public uint				LodgingId { get; set; }
	public uint				Id { get; set; }
	public uint				RoomNumber { get; set; }

	public Booking	Booking { get; }
	public Room		Room { get; }
}
