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
        return Enum.GetNames<BookingStatus>();
    }
    
    [HttpGet("lodging/{lodgingId}/{pageSize}/{page}")]
    public async Task<ObjectResult> GetLodgingBookings(uint lodgingId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [Range(0, int.MaxValue)] int pageSize = 10,
        [Range(0, int.MaxValue)] int page = 1)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);
        if (lodging != null)
        {
            IQueryable<Booking> bookings = _context.Booking
                .AsNoTracking()
                .Where(b => b.LodgingId == lodgingId)
                .Include(b => b.RoomBookings);
            
            bookings = StandardFilters.BookingByDates(bookings, startDate, endDate);
            
            IQueryable<object> bookingsInfo = bookings 
                .Select(b => new { b.Id, b.CustomerId, b.Customer.UserName, b.Payment, b.RoomBookings });
            return Ok(await PaginatedList<object>.CreateAsync(bookingsInfo, page, pageSize));
        }

        return NotFound("No existe ningún alojamiento con el identificador especificado.");
    }
    
    [HttpGet("user/{userName}/{pageSize}/{page}")]
    public async Task<ObjectResult> GetUserBookings(string userName,
        [FromQuery] uint? lodgingId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [Range(0, int.MaxValue)] int pageSize = 10,
        [Range(0, int.MaxValue)] int page = 1)
    {
        User? user = _context.Find<User>(userName);
        if (user != null)
        {
            IQueryable<Booking> bookings = _context.Booking
                .AsNoTracking()
                .Where(b => b.CustomerId == user.Person.Id)
                .Include(b => b.RoomBookings);

            bookings = StandardFilters.BookingByLodging(bookings, lodgingId);
            bookings = StandardFilters.BookingByDates(bookings, startDate, endDate);
            
            IQueryable<object> bookingsInfo = bookings 
                .Select(b => new { b.Id, b.LodgingId, b.Lodging.Name, b.Payment, b.RoomBookings });
            return Ok(await PaginatedList<object>.CreateAsync(bookingsInfo, page, pageSize));
        }

        return NotFound("No existe ningún usuario con el nombre especificado.");
    }
    
    [HttpPost]
    public ObjectResult Post(BookingRequestData data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Lodging lodging = _context.Find<Lodging>(data.LodgingId)!;
        _context.Entry(lodging).Collection(l => l.RoomTypes).Load();
        
        ObjectResult? result = GetRoomBookingsToAdd(lodging, data.Rooms, out List<RoomBooking>? roomBookingsToAdd,
            out _);

        if (result != null)
        {
            return result;
        }
            
        Booking booking = new Booking(roomBookingsToAdd!)
        {
            CustomerId = data.CustomerId,
            LodgingId = data.LodgingId
        };
            
        _context.Booking.Add(booking);
        _context.SaveChanges();

        return Created(booking);
    }

    [HttpDelete("user/{userName}")]
    public ObjectResult DeleteUserBookings(string userName, uint[] bookingIds)
    {
        User? user = _context.Find<User>(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        _context.Entry(user).Reference(u => u.Person).Load();
        int rows = _context.Booking
            .Where(b => b.CustomerId == user.Person.Id && bookingIds.Contains(b.Id))
            .Delete();

        if (rows == 0)
        {
            return NotFound("No existe ninguna reserva con los identificadores especificados.");
        }

        string message = "Las reservaciones han sido eliminadas con éxito.";
        if (rows != bookingIds.Length)
        {
            return Ok($"{message} Algunos identificadores no correspondieron a ninguna reservación.");
        }

        return Ok(message);

    }

    public ObjectResult ChangeBookingStatus(string userName, uint bookingId, BookingStatus newBookingStatus)
    {
        User? user = _context.Find<User>(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        Booking? booking = _context.Find<Booking>(bookingId);

        if (booking == null)
        {
            return NotFound("No existe una reservación con el identificador especificado.");
        }
        
        _context.Entry(booking).Collection(b => b.RoomBookings).Load();

        foreach (RoomBooking roomBooking in booking.RoomBookings)
        {
            roomBooking.Status = newBookingStatus;
        }

        _context.SaveChanges();
        
        return Ok("El estado de la reserva ha sido actualizado con éxito.");
    }
    
    [HttpPatch("user/{userName}")]
    public ObjectResult UpdateUserBooking(string userName, BookingPatchRequestData bookingData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = _context.Find<User>(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        _context.Entry(user).Reference(u => u.Person).Load();
        Booking? booking = _context.Booking
            .Where(b => b.Id == bookingData.BookingId)
            .Include(b => b.Lodging)
            .ThenInclude(l => l.RoomTypes)
            .Include(b => b.RoomBookings)
            .ThenInclude(r => r.Room)
            .ThenInclude(r => r.Type)
            .FirstOrDefault();

        if (booking == null || booking.CustomerId != user.Person.Id)
        {
            return NotFound("No existe una reservación asociada al cliente, con el identificador especificado.");
        }

        List<uint> nonExistentRoomBookingIdsToDelete = new List<uint>();
        if (bookingData.RoomsBookingsToDelete != null)
        {
            foreach (var roomBookingId in bookingData.RoomsBookingsToDelete)
            {
                RoomBooking? roomBooking = booking.RoomBookings.FirstOrDefault(r => r.Id == roomBookingId);
                if (roomBooking == null)
                {
                    nonExistentRoomBookingIdsToDelete.Add(roomBookingId);
                    continue;
                }
                
                booking.RoomBookings.Remove(roomBooking);
            }
        }

        Dictionary<uint, List<(DateOnly StartDate, DateOnly EndDate)>>? newRoomBookingsByRoomNumber = null;
        if (bookingData.RoomsBookingsToAdd != null)
        {
            ObjectResult? result = GetRoomBookingsToAdd(booking.Lodging, bookingData.RoomsBookingsToAdd,
                out var roomBookingsToAdd, out newRoomBookingsByRoomNumber);

            if (result != null)
            {
                return result;
            }

            foreach (RoomBooking roomBooking in roomBookingsToAdd)
            {
                booking.RoomBookings.Add(roomBooking);
            }
        }

        bool lodgingDoesNotOfferRooms = !Lodging.OffersRooms(booking.Lodging);
        uint roomTypeId = 0; 
        
        if (lodgingDoesNotOfferRooms)
        {
            roomTypeId = booking.Lodging.RoomTypes[0].Id;
        }

        bool invalidUpdateData = false;
        List<uint> invalidRoomBookingIds = new List<uint>();
        if (bookingData.RoomsBookingsToUpdate != null)
        {
            if (newRoomBookingsByRoomNumber == null)
            {
                newRoomBookingsByRoomNumber = new Dictionary<uint, List<(DateOnly StartDate, DateOnly EndDate)>>();
            }
            
            Dictionary<uint, List<Room>>? availableRoomsByType = null;
            foreach (RoomBookingPatchRequestData roomBookingData in bookingData.RoomsBookingsToUpdate)
            {
                RoomBooking? roomBooking = booking.RoomBookings.FirstOrDefault(
                    r => r.Id == roomBookingData.RoomBookingId);

                if (roomBooking == null)
                {
                    invalidRoomBookingIds.Add(roomBookingData.RoomBookingId);
                    invalidUpdateData = true;
                }
                else if (!invalidUpdateData)
                {
                    if (lodgingDoesNotOfferRooms)
                    {
                        roomBookingData.RoomTypeId = roomTypeId;
                    }
                    DateOnly    startDate = roomBookingData.StartDate ?? roomBooking.StartDate,
                                endDate = roomBookingData.EndDate ?? roomBooking.EndDate;
                    if (endDate.CompareTo(startDate) <= 0)
                    {
                        return NotAcceptable(
                            "La fecha de inicio de la reservación debe ser anterior a la fecha de finalización.");
                    }
                    
                    // If only the discount is to be updated, then there's no need to do all the rest here
                    if (roomBookingData.RoomTypeId == roomBooking.Room.TypeId &&
                        (!roomBookingData.StartDate.HasValue || roomBookingData.StartDate.Value == roomBooking.StartDate) &&
                        (!roomBookingData.EndDate.HasValue || roomBookingData.EndDate.Value == roomBooking.EndDate))
                    {
                        if (roomBookingData.Discount.HasValue)
                            roomBooking.Discount = roomBookingData.Discount.Value;
                        
                        break;
                    }
                    
                    if (availableRoomsByType == null)
                    {
                        var roomBookingIdsToExclude = booking.RoomBookings.Select(
                            r => r.BookingId);
                        
                        if (bookingData.RoomsBookingsToDelete != null)
                        {
                            roomBookingIdsToExclude = roomBookingIdsToExclude
                                .Concat(bookingData.RoomsBookingsToDelete);
                        }
                        
                        var requestedRoomsDataByType = bookingData.RoomsBookingsToUpdate
                            .GroupBy(r => r.RoomTypeId)
                            .ToArray();
                        availableRoomsByType = GetAvailableRoomsByType(booking.LodgingId,
                            requestedRoomsDataByType,
                            roomBookingIdsToExclude.ToArray());
                    }

                    Room? selectedRoom;
                    // TODO: Should test first if the room type really exists in this lodging
                    if (availableRoomsByType.TryGetValue(roomBookingData.RoomTypeId, out var availableRoomsOfType))
                    {
                        selectedRoom = null;
                        List<(DateOnly StartDate, DateOnly EndDate)>? newBookings = null;
                        if (roomBooking.Room.TypeId == roomBookingData.RoomTypeId &&
                            IsRoomStillAvailable(roomBooking.RoomNumber, roomBookingData, newRoomBookingsByRoomNumber, out newBookings))
                        {
                            selectedRoom = availableRoomsOfType.Find(r => r.Number == roomBooking.RoomNumber);
                        }
                        else
                        {
                            foreach (var availableRoom in availableRoomsOfType)
                            {
                                if (IsRoomStillAvailable(availableRoom.Number, roomBookingData, newRoomBookingsByRoomNumber, out newBookings))
                                {
                                    selectedRoom = availableRoom;
                                    break;
                                }
                            }
                        }

                        if (selectedRoom != null)
                        {
                            if (roomBookingData.StartDate.HasValue)
                                roomBooking.StartDate = roomBookingData.StartDate.Value;
                            
                            if (roomBookingData.EndDate.HasValue)
                                roomBooking.EndDate = roomBookingData.EndDate.Value;
                            
                            if (roomBookingData.Discount.HasValue)
                                roomBooking.Discount = roomBookingData.Discount.Value;

                            // TODO: This calculation and assignment could be done in the database itself
                            int days = roomBooking.EndDate.DayNumber - roomBooking.StartDate.DayNumber;
                            
                            roomBooking.RoomNumber = selectedRoom.Number;
                            roomBooking.Cost = selectedRoom.Type.PerNightPrice * days;
                            roomBooking.Fees = selectedRoom.Type.Fees;
                            
                            newBookings.Add((roomBooking.StartDate, roomBooking.EndDate));
                        }
                    }
                    else
                    {
                        return NotAcceptable(
                            "No hay habitaciones disponibles para realizar la actualización de la reservación en los intervalos de tiempo especificados.");
                    }
                    
                    if (selectedRoom == null)
                    {
                        return NotAcceptable(
                            "No hay habitaciones disponibles para realizar la actualización en la reservación en los intervalos de tiempo especificados.");
                    }
                }
            }
        }

        if (invalidUpdateData)
        {
            string idsJoined = string.Join(',', invalidRoomBookingIds);
            return NotFound($"No existen las reservaciones de habitación con los identificadores {idsJoined} en la reservación a modificar.");
        }

        string message = "Las reservaciones han sido actualizadas con éxito.";
        if (nonExistentRoomBookingIdsToDelete.Count > 0)
        {
            string nonExistentRoomBookingIdsToDeleteJoined = string.Join(',', nonExistentRoomBookingIdsToDelete);
            message +=
                $" Los identificadores {nonExistentRoomBookingIdsToDeleteJoined} a borrar no correspondieron a ninguna reserva de habitación";
        }

        _context.SaveChanges();
        return Ok(message);
    }

    private ObjectResult? GetRoomBookingsToAdd(Lodging lodging,
        RoomBookingRequestData[] roomBookingsData,
        out List<RoomBooking>? roomBookingsToAdd,
        out Dictionary<uint, List<(DateOnly StartDate, DateOnly EndDate)>>? roomBookingDatesByRoomNumber)
    {
        bool lodgingDoesNotOfferRooms = !Lodging.OffersRooms(lodging);
        if (lodgingDoesNotOfferRooms)
        {
            uint roomTypeId = lodging.RoomTypes[0].Id;
            foreach (RoomBookingRequestData data in roomBookingsData)
            {
                data.RoomTypeId = roomTypeId;
            }
        }
        
        var requestedRoomsDataByType = roomBookingsData
            .GroupBy(r => r.RoomTypeId)
            .ToArray();
        
        var availableRoomsByType = GetAvailableRoomsByType(lodging.Id, requestedRoomsDataByType);
        var availableRoomsNewBookings = new Dictionary<uint, List<(DateOnly StartDate, DateOnly EndDate)>>();

        List<RoomBooking> roomBookings = new List<RoomBooking>();
        // You can't have enough nested for loops, can you?
        foreach (var group in requestedRoomsDataByType)
        {
            if (availableRoomsByType.TryGetValue(group.Key, out var availableRoomsOfType))
            {
                foreach (var requestedRoomData in group)
                {
                    bool noRoomsAvailable = true;
                    foreach (var room in availableRoomsOfType)
                    {
                        if (IsRoomStillAvailable(room.Number, requestedRoomData, availableRoomsNewBookings,
                                out var newBookings))
                        {
                            // TODO: This calculation and assignment could be done in the database itself
                            int days = requestedRoomData.EndDate.Value.DayNumber - requestedRoomData.StartDate.Value.DayNumber;
                            roomBookings.Add(new RoomBooking
                            {
                                LodgingId = lodging.Id,
                                StartDate = requestedRoomData.StartDate!.Value,
                                EndDate = requestedRoomData.EndDate!.Value,
                                RoomNumber = room.Number,
                                Cost = room.Type.PerNightPrice * days,
                                Fees = room.Type.Fees,
                                Discount = requestedRoomData.Discount ?? 0,
                                Status = BookingStatus.Created
                            });
                                
                            newBookings.Add((requestedRoomData.StartDate.Value, requestedRoomData.EndDate.Value));
                            noRoomsAvailable = false;
                            break;
                        }
                    }
                        
                    if (noRoomsAvailable)
                    {
                        roomBookingsToAdd = roomBookings;
                        roomBookingDatesByRoomNumber = availableRoomsNewBookings;
                        
                        return NotAcceptable(
                            $"No hay habitaciones disponibles para realizar la reservación en los intervalos de tiempo especificados.");
                    }
                }
            }
            else
            {
                roomBookingsToAdd = roomBookings;
                roomBookingDatesByRoomNumber = availableRoomsNewBookings;
                
                return NotFound($"No hay habitaciones disponibles en los intervalos de tiempo especificados.");
            }
        }

        roomBookingsToAdd = roomBookings;
        roomBookingDatesByRoomNumber = availableRoomsNewBookings;
        return null;
    }

    private Dictionary<uint, List<Room>> GetAvailableRoomsByType(
        uint lodgingId,
        IList<IGrouping<uint, IRoomBookingRequestData>> requestedRoomsDataByType,
        uint[]? roomBookingsToExclude = null)
    {
        Dictionary<uint, List<Room>> availableRoomsByType = new Dictionary<uint, List<Room>>(requestedRoomsDataByType.Count);
        foreach (var requestedRoomData in requestedRoomsDataByType)
        {
            StringBuilder sqlQuery = new StringBuilder($"""
                SELECT * FROM Room AS r
                WHERE r.lodgingId = {lodgingId} AND r.roomTypeId = {requestedRoomData.Key}
                AND r.roomNumber NOT IN (
                    SELECT rb.roomNumber FROM RoomBooking AS rb
                        INNER JOIN Room AS r2 ON r2.roomNumber = rb.roomNumber
                        INNER JOIN RoomType AS rt ON rt.roomTypeId = r2.roomTypeId
                        WHERE rb.lodgingId = {lodgingId}
                            AND rt.roomTypeId = {requestedRoomData.Key}
                            AND rb.status IN ('Created', 'Confirmed')
                            {(roomBookingsToExclude != null ? $"AND rb.roomBookingId NOT IN ({string.Join(',', roomBookingsToExclude)})" : "")}
                            AND (
                """);

            foreach (var roomData in requestedRoomData)
            {
                sqlQuery.Append($"""
                                 (rb.startDate < '{roomData.EndDate!.Value.ToString(RestifyDbContext.DATE_FORMAT)}'
                                 AND rb.endDate > '{roomData.StartDate!.Value.ToString(RestifyDbContext.DATE_FORMAT)}')
                                 OR
                                 """);
            }

            sqlQuery.Replace("OR", "))", sqlQuery.Length - 2, 2);

            var availableRoomsOfType = _context.Room
                .FromSqlRaw(sqlQuery.ToString())
                .Include(r => r.Type)
                .ToList();

            if (availableRoomsOfType.Count > 0)
            {
                availableRoomsByType.Add(requestedRoomData.Key, availableRoomsOfType);
            }
        }

        return availableRoomsByType;
    }

    private bool IsRoomStillAvailable(uint roomNumber, IRoomBookingRequestData roomBookingData,
        Dictionary<uint, List<(DateOnly StartDate, DateOnly EndDate)>> availableRoomsNewBookings,
        out List<(DateOnly StartDate, DateOnly EndDate)> newBookings)
    {
        bool isAvailable = true;
        if (availableRoomsNewBookings.TryGetValue(roomNumber, out newBookings))
        {
            foreach (var reservation in newBookings)
            {
                if (reservation.StartDate < roomBookingData.EndDate &&
                    reservation.EndDate > roomBookingData.StartDate)
                {
                    isAvailable = false;
                    break;
                }
            }
        }
        else
        {
            newBookings = new List<(DateOnly StartDate, DateOnly EndDate)>();
            availableRoomsNewBookings.Add(roomNumber, newBookings);
        }

        return isAvailable;
    }
}

public interface IRoomBookingRequestData
{
    uint        RoomTypeId { get; }
    DateOnly?   StartDate { get; }
    DateOnly?   EndDate { get; }
    decimal?    Discount { get; }
}

[EndDateIsAfterStartDate]
public record RoomBookingRequestData : IRoomBookingRequestData
{
    [Required(ErrorMessage = "El identificador del tipo de habitación es obligatorio.")]
    public uint        RoomTypeId { get; set; }
    [Required(ErrorMessage = "La fecha de inicio de la reservación de la habitación es obligatoria.")]
    public DateOnly?   StartDate { get; init; }
    [Required(ErrorMessage = "La fecha de finalización de la reservación es obligatoria.")]
    public DateOnly?   EndDate { get; init; }
    public decimal?    Discount { get; init; }
}

[EndDateIsAfterStartDate]
public record RoomBookingPatchRequestData : IRoomBookingRequestData
{
    [Required(ErrorMessage = "El identificador de la reservación de la habitación es obligatorio.")]
    public uint          RoomBookingId { get; init; }
    [Required(ErrorMessage = "El identificador del tipo de habitación es obligatorio.")]
    public uint          RoomTypeId { get; set; }
    public DateOnly?     StartDate { get; init; }
    public DateOnly?     EndDate { get; init; }
    public decimal?      Discount { get; init; }
}

public record BookingRequestData
{
    [Required(ErrorMessage = "El identificador del cliente es obligatorio.")]
    [Exists<Person>(ErrorMessage = "No existe un cliente con el identificador especificado.")]
    public uint CustomerId { get; init; }
    [Required(ErrorMessage = "El identificador del alojamiento es obligatorio.")]
    [Exists<Lodging>(ErrorMessage = "No existe un alojamiento con el identificador especificado.")]
    public uint LodgingId { get; init; }
    public RoomBookingRequestData[] Rooms { get; init; }
}


[BookingPatchRequestDataValidation]
public record BookingPatchRequestData
{
    [Required(ErrorMessage = "El identificador de la reservación es obligatorio.")]
    [Exists<Booking>(ErrorMessage = "No existe una reservación con el identificador especificado.")]
    public uint BookingId { get; init; }
    public RoomBookingRequestData[]? RoomsBookingsToAdd { get; init; }
    public uint[]? RoomsBookingsToDelete { get; init; }
    public RoomBookingPatchRequestData[]? RoomsBookingsToUpdate { get; init; }
}