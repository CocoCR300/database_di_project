using System.ComponentModel.DataAnnotations;
using System.Text;
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
            var requestedRoomsDataByType = data.Rooms
                .GroupBy(r => r.RoomTypeId)
                .ToArray();

            var availableRoomsByType = new Dictionary<uint, List<Room>>(requestedRoomsDataByType.Length);
            var availableRoomsNewBookings = new Dictionary<uint, List<(DateOnly StartDate, DateOnly EndDate)>>();
            foreach (var requestedRoomData in requestedRoomsDataByType)
            {
                // TODO: Check if the lodging type is a house or whatever to not check the room bookings
                StringBuilder sqlQuery = new StringBuilder($"""
                    SELECT * FROM Room AS r
                    WHERE r.lodgingId = {data.LodgingId} AND r.roomTypeId = {requestedRoomData.Key}
                    AND r.roomNumber NOT IN (
                        SELECT rb.roomNumber FROM RoomBooking AS rb
                            INNER JOIN Room AS r2 ON r2.roomNumber = rb.roomNumber
                            INNER JOIN RoomType AS rt ON rt.roomTypeId = r2.roomTypeId
                            WHERE rb.lodgingId = {data.LodgingId}
                                AND rt.roomTypeId = {requestedRoomData.Key}
                                AND rb.status IN ('Created', 'Confirmed')
                                AND (
                    """);

                foreach (var roomData in requestedRoomData)
                {
                    sqlQuery.Append($"(rb.startDate < '{roomData.EndDate.ToString(RestifyDbContext.DATE_FORMAT)}' AND rb.endDate > '{roomData.StartDate.ToString(RestifyDbContext.DATE_FORMAT)}') OR");
                }

                sqlQuery.Replace("OR", "))", sqlQuery.Length - 2, 2);

                var availableRoomsOfType = _context.Room
                    .FromSqlRaw(sqlQuery.ToString())
                    .Include(r => r.Type);
                
                availableRoomsByType.Add(requestedRoomData.Key, availableRoomsOfType
                    .ToList());
            }

            List<RoomBooking> roomBookings = new List<RoomBooking>();
            // You can't have enough nested for loops, can you?
            foreach (var group in requestedRoomsDataByType)
            {
                var availableRoomsOfType = availableRoomsByType[group.Key];

                foreach (var requestedRoomData in group)
                {
                    foreach (var room in availableRoomsOfType)
                    {
                        if (availableRoomsNewBookings.TryGetValue(room.Number, out var newBookings))
                        {
                            foreach (var reservation in newBookings)
                            {
                                if (reservation.StartDate < requestedRoomData.EndDate &&
                                    reservation.EndDate > requestedRoomData.StartDate)
                                {
                                    goto nextAvailableRoom;
                                }
                            }
                        }
                        else
                        {
                            newBookings = new List<(DateOnly StartDate, DateOnly EndDate)>();
                            availableRoomsNewBookings.Add(room.Number, newBookings);
                            goto nextRoomRequest;
                        }
                        
                        roomBookings.Add(new RoomBooking
                        {
                            LodgingId = data.LodgingId,
                            StartDate = requestedRoomData.StartDate,
                            EndDate = requestedRoomData.EndDate,
                            RoomNumber = room.Number,
                            Cost = room.Type.PerNightPrice,
                            Fees = room.Type.Fees,
                            Discount = requestedRoomData.Discount,
                            Status = BookingStatus.Created
                        });
                        
                        newBookings.Add((requestedRoomData.StartDate, requestedRoomData.EndDate));
                        
                        nextAvailableRoom:
                            continue; // Just whatever for the label to work
                    }

                    return NotAcceptable(
                        "No hay habitaciones suficientes para llevar a cabo la reservación en los intervalos de tiempo especificados.");
                    
                    nextRoomRequest:
                        continue; // Just whatever for the label to work
                }
            }
            
            Booking booking = new Booking 
            {
                CustomerId = data.CustomerId,
                LodgingId = data.LodgingId
            };
            
            foreach (var roomBooking in roomBookings)
            {
                booking.RoomBookings.Add(roomBooking);
            }
            
            _context.Booking.Add(booking);
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
    
    [HttpPatch("user/{userName}")]
    public ObjectResult UpdateUserBooking(string userName, BookingPatchRequestData bookingData)
    {
        if (ModelState.IsValid)
        {
            User? user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();
                Booking? booking = _context.Booking
                    .Where(b => b.Id == bookingData.BookingId
                                && b.CustomerId == user.Person.Id
                                && b.LodgingId == bookingData.LodgingId)
                    .Include(b => b.RoomBookings)
                    .First();

                if (booking == null)
                {
                    return NotFound(
                        "No existe una reservación asociada al cliente y alojamiento especificados, con el ese identificador.");
                }
                
                foreach (RoomBooking roomBooking in booking.RoomBookings)
                {
                    //RoomBookingRequestData? data = Array.Find(bookingData.Rooms,
                    //    r => r.Number == roomBooking.RoomNumber);

                    //if (data == null)
                    //{
                    //    // TODO: Put all the rooms that are not associated in this message
                    //    return NotFound($"La habitación {roomBooking.RoomNumber} no está incluida en la reservación.");
                    //}
                    
                    
                }

                string message = "Las reservaciones han sido actualizadas con éxito.";
                //if (rows == bookingIds.Length)
                //{
                //    return Ok($"{message} Algunos identificadores no correspondieron a ninguna reservación.");
                //}

                return Ok(message);
            }
            
            return NotFound("No existe un usuario con el nombre especificado.");
        }
        
        return BadRequest(ModelState);
    }
}

[EndDateIsAfterStartDate]
public record RoomBookingRequestData(
    [Required] uint     RoomTypeId,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required] decimal  Discount
);

public class BookingRequestData
{
    [Required]
    [Exists<Person>]
    public uint CustomerId { get; set; }
    [Required]
    [Exists<Lodging>]
    public uint LodgingId { get; set; }
    [Required]
    public RoomBookingRequestData[] Rooms { get; set; }
}

public class BookingPatchRequestData
{
    [Required]
    [Exists<Booking>]
    public uint BookingId { get; set; }
    [Required]
    [Exists<Lodging>]
    public uint LodgingId { get; set; }
    [Required]
    public RoomBookingRequestData[] Rooms { get; set; }
}