using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;
using Z.EntityFramework.Plus;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class BookingController : BaseController
{
    private readonly RestifyDbContext _context;

    public BookingController(RestifyDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public string[] GetStatuses()
    {
        return  Enum.GetNames<BookingStatus>();
    }
    
    [HttpGet("lodging/{lodgingId}")]
    public ObjectResult GetLodgingBookings(uint lodgingId)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);
        if (lodging != null)
        {
            var bookings = _context.Booking
                .AsNoTracking()
                .Where(b => b.LodgingId == lodgingId)
                .Include(b => b.RoomBookings)
                .Select(b => new { b.Id, b.CustomerId, b.LodgingId, b.Status, b.Payment, b.RoomBookings });
            
            return Ok(bookings);
        }

        return NotFound("No existe ningún alojamiento con el identificador especificado.");
    }
    
    [HttpGet("user/{userName}")]
    public ObjectResult GetUserBookings(string userName)
    {
        User? user = _context.Find<User>(userName);
        if (user != null)
        {
            _context.Entry(user).Reference(u => u.Person).Load();
            var bookings = _context.Booking
                .AsNoTracking()
                .Where(b => b.CustomerId == user.Person.Id)
                .Include(b => b.RoomBookings)
                .Select(b => new { b.Id, b.CustomerId, b.LodgingId, b.Status, b.Payment, b.RoomBookings });
            
            return Ok(bookings);
        }

        return NotFound("No existe ningún usuario con el nombre especificado.");
    }
    
    [HttpPost]
    public ObjectResult Post(BookingRequestData data)
    {
        if (ModelState.IsValid)
        {
            List<RoomBooking> roomBookings = new List<RoomBooking>(data.Rooms.Length);
            
            uint[] requestedRoomNumbers = data.Rooms.Select(r => r.Number)
                .ToArray();
            
            var existingRoomBookings = _context.RoomBooking
                .Select(r => new { r.LodgingId, r.RoomNumber, r.StartDate, r.EndDate })
                .Where(r => r.LodgingId == data.LodgingId && requestedRoomNumbers.Contains(r.RoomNumber))
                .ToDictionary(r => r.RoomNumber, r => r);

            var referencedLodgingRooms = _context.Room
                .Select(r => new { r.LodgingId, r.Number, r.Type }) // TODO: Does selecting a navigation property work? 
                .Where(r => r.LodgingId == data.LodgingId && requestedRoomNumbers.Contains(r.Number))
                .ToArray();

            if (referencedLodgingRooms.Length != data.Rooms.Length)
            {
                return NotFound("El alojamiento no tiene algunos de los números de habitación especificados.");
            }

            var availableRoomsRequestData = new List<RoomBookingRequestData>();
            foreach (var room in referencedLodgingRooms)
            {
                var roomData = Array.Find(data.Rooms, r => r.Number == room.Number);
                
                if (existingRoomBookings.TryGetValue(room.Number, out var existingRoomBooking))
                {
                    if (existingRoomBooking.StartDate <= roomData.EndDate &&
                        existingRoomBooking.EndDate >= roomData.StartDate)
                    {
                        availableRoomsRequestData.Add(roomData);
                    }
                    else
                    {
                        return NotAcceptable("Algunas de las habitaciones ya están reservadas en el rango de fechas especificado.");
                    }
                }
                else
                {
                    availableRoomsRequestData.Add(roomData);
                }
            }
            
            Dictionary<uint, decimal> perNightPricesByRoom = referencedLodgingRooms
                .ToDictionary(r => r.Number, r => r.Type.PerNightPrice);
            
            Booking booking = new Booking 
            {
                CustomerId = data.CustomerId,
                LodgingId = data.LodgingId
            };
            _context.Booking.Add(booking);
            _context.SaveChanges(); // Save changes now so the new booking gets its ID

            foreach (var roomBookingRequestData in availableRoomsRequestData)
            {
                roomBookings.Add(new RoomBooking
                {
                    RoomNumber = roomBookingRequestData.Number,
                    StartDate = roomBookingRequestData.StartDate,
                    EndDate = roomBookingRequestData.EndDate,
                    LodgingId = data.LodgingId,
                    Cost = perNightPricesByRoom[roomBookingRequestData.Number],
                    Status = BookingStatus.Created,
                    BookingId = booking.Id
                });
            }
            _context.RoomBooking.AddRange(roomBookings);
            _context.SaveChanges();

            return Ok("La reservación ha sido creada con éxito");
        }
        
        return BadRequest(ModelState);
    }

    [HttpDelete("user/{userName}")]
    public ObjectResult DeleteUserBookings(string userName, uint[] bookingIds)
    {
        User? user = _context.Find<User>(userName);

        if (user != null)
        {
            _context.Entry(user).Reference(u => u.Person).Load();
            int rows = _context.Booking
                .Where(b => b.CustomerId == user.Person.Id && bookingIds.Contains(b.Id))
                .Delete();

            if (rows == 0)
            {
                return NotFound("No existe ninguna reserva con los identificadores especificados.");
            }

            string message = "Las reservaciones han sido eliminadas con éxito.";
            if (rows == bookingIds.Length)
            {
                return Ok($"{message} Algunos identificadores no correspondieron a ninguna reservación.");
            }

            return Ok(message);
        }
        
        return NotFound("No existe un usuario con el nombre especificado.");
    }
}

public record RoomBookingRequestData(
    [Required] uint Number,
    [Required] DateTimeOffset StartDate,
    [Required] DateTimeOffset EndDate
);

public class BookingRequestData
{
    [Required]
    [MaxLength(20)]
    public BookingStatus Status { get; set; }
    [Required]
    [Exists<Person>]
    public uint CustomerId { get; set; }
    [Required]
    [Exists<Lodging>]
    public uint LodgingId { get; set; }
    [Required]
    public RoomBookingRequestData[] Rooms { get; set; }
}