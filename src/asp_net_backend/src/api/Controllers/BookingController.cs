using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class BookingController : Controller
{
    private readonly RestifyDbContext _context;

    public BookingController(RestifyDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public ObjectResult Post(BookingRequestData data)
    {
        if (ModelState.IsValid)
        {
            uint[] roomNumbers = data.Rooms.Select(r => r.Number).ToArray();
            List<RoomBooking> roomBookings = new List<RoomBooking>(data.Rooms.Length);
            Dictionary<uint, decimal> rooms = _context.Room
                .Select(r => new { r.LodgingId, r.Number, r.PerNightPrice })
                .Where(r => r.LodgingId == data.LodgingId
                            && roomNumbers.Contains(r.Number))
                .ToDictionary(r => r.Number, r => r.PerNightPrice);
            
            Booking booking = new Booking 
            {
                StartDate = data.StartDate,
                EndDate = data.EndDate,
                Status = data.Status,
                CustomerId = data.CustomerId,
                LodgingId = data.LodgingId,
            };
            _context.Booking.Add(booking);
            _context.SaveChanges(); // Save changes now so the new booking gets its ID

            foreach (var roomBookingRequestData in data.Rooms)
            {
                roomBookings.Add(new RoomBooking
                {
                    RoomNumber = roomBookingRequestData.Number,
                    LodgingId = data.LodgingId,
                    Cost = rooms[roomBookingRequestData.Number],
                    BookingId = booking.Id
                });
            }
            _context.RoomBooking.AddRange(roomBookings);
            _context.SaveChanges();

            return Ok("La reservación ha sido creada con éxito");
        }
        
        return BadRequest(ModelState);
    }
}

public record RoomBookingRequestData(
    [Required] uint Number,
    [Required] decimal Fee
);

public class BookingRequestData
{
    [Required]
    public DateTimeOffset StartDate { get; set; }
    [Required]
    public DateTimeOffset EndDate { get; set; }
    [Required]
    [MaxLength(20)] // TODO: One of restriction
    public string Status { get; set; }
    [Required]
    [Exists<Person>]
    public uint CustomerId { get; set; }
    [Required]
    public uint LodgingId { get; set; }
    [Required]
    [MinLength(1)]
    public RoomBookingRequestData[] Rooms { get; set; }
}