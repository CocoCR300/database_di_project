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
public class RoomController : BaseController
{
    private RestifyDbContext _context;
    
    public RoomController(RestifyDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{lodgingId}/available/{roomTypeId}/{bookingStartDate}/{bookingEndDate}")]
    public ObjectResult GetAvailableRoomsOfType(int lodgingId,
        int roomTypeId, DateOnly bookingStartDate, DateOnly bookingEndDate)
    {
        IQueryable<int> bookedRoomsNumbers = _context.RoomBooking
            .Where(r => r.LodgingId == lodgingId && r.Room.TypeId == roomTypeId
                                                 && (r.Status == BookingStatus.Created ||
                                                     r.Status == BookingStatus.Confirmed)
                                                 && r.StartDate < bookingEndDate && r.EndDate > bookingStartDate)
            .Select(r => r.RoomNumber);

        IQueryable<Room> availableRoomsOfType = _context.Room
            .Where(r => !bookedRoomsNumbers.Contains(r.Number));
        
        return Ok(availableRoomsOfType);
    }
    
    [HttpGet("{lodgingId}")]
    public ObjectResult GetRooms(int lodgingId)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }
        
        _context.Entry(lodging).Collection(l => l.Rooms).Load();
        var rooms = lodging.Rooms
            .Select(r => new { r.Number, r.TypeId });
        return Ok(rooms);
    }
    
    [HttpDelete("{lodgingId}")]
    public ObjectResult DeleteRooms(int lodgingId, int[] roomNumbers)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }
        
        _context.Entry(lodging).Collection(l => l.Rooms).Load();
            
        bool noneExists = true;
        for (int i = 0; i < lodging.Rooms.Count; ++i)
        {
            int roomNumber = lodging.Rooms[i].Number;
            if (roomNumbers.Contains(roomNumber))
            {
                noneExists = false;
                lodging.Rooms.RemoveAt(i);
            }
        }
                
        _context.SaveChanges();

        if (noneExists)
        {
            return NotFound("El alojamiento no tiene ninguno de los números de habitación especificados.");
        }
                
        return Ok("Las habitaciones han sido eliminadas con éxito.");
    }
    
    [HttpPost("{lodgingId}")]
    public ObjectResult StoreRooms(int lodgingId, RoomRequestData[] rooms)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }
        
        _context.Entry(lodging).Collection(l => l.Rooms).Load();
        int[] roomsNumbersOrdered = lodging.Rooms
            .Select(r => r.Number)
            .OrderBy(r => r)
            .ToArray();
        int[] roomTypeIdsOrdered = lodging.RoomTypes
            .Select(r => r.Id)
            .OrderBy(r => r)
            .ToArray();

        List<int> existingRoomNumbers = new List<int>();
        foreach (RoomRequestData room in rooms)
        {
            int typeIdIndex = Array.BinarySearch(roomTypeIdsOrdered, room.TypeId);

            if (typeIdIndex < 0)
            {
                return NotFound($"No existe un tipo de habitación con el identificador {room.TypeId} en el alojamiento.");
            }
                
            int roomNumberIndex = Array.BinarySearch(roomsNumbersOrdered, room.Number);

            if (roomNumberIndex < 0)
            {
                lodging.Rooms.Add(new Room
                {
                    LodgingId = lodging.Id,
                    Number = room.Number,
                    TypeId = room.TypeId
                });
            }
            else
            {
                existingRoomNumbers.Add(room.Number);
            }
        }

        if (existingRoomNumbers.Count > 0)
        {
            string roomNumbersJoined = string.Join(',', existingRoomNumbers);
            return NotAcceptable($"Ya existen habitaciones con los números {roomNumbersJoined}.");
        }
            
        _context.SaveChanges();
        return Created();
    }
    
    [HttpPost("{lodgingId}/sequence")]
    public ObjectResult StoreRoomSequence(int lodgingId, RoomSequenceRequestData roomSequenceData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }

        if (roomSequenceData.StartNumber > roomSequenceData.EndNumber)
        {
            return NotAcceptable("El número inicial debe ser menor al número final.");
        }

        if (lodging.RoomTypes.Any(r => r.Id == roomSequenceData.TypeId))
        {
            return NotFound($"El alojamiento no posee un tipo de habitación con el identificador {roomSequenceData.TypeId}.");
        }
        
        Room[] roomsNumbersOrdered = lodging.Rooms
            .OrderBy(r => r.Number)
            .ToArray();

        List<int> existingRoomNumbers = new List<int>();
        for(int roomNumber = roomSequenceData.StartNumber; roomNumber <= roomSequenceData.EndNumber; ++roomNumber)
        {
            int index = Array.BinarySearch(roomsNumbersOrdered, roomNumber);

            if (index > 0)
            {
                lodging.Rooms.Add(new Room
                {
                    LodgingId = lodging.Id,
                    Number = roomNumber,
                    TypeId = roomSequenceData.TypeId
                });
            }
            else
            {
                existingRoomNumbers.Add(roomNumber);
            }
        }

        if (existingRoomNumbers.Count > 0)
        {
            string roomNumbersJoined = string.Join(',', existingRoomNumbers);
            return NotAcceptable($"Ya existen habitaciones con los números {roomNumbersJoined}.");
        }
            
        _context.SaveChanges();
        return Created();
    }
}

public class RoomRequestData 
{
    [Required(ErrorMessage = "El número de la habitación es obligatorio.")]
    public int Number { get; set; }
    [Required(ErrorMessage = "El identificador del tipo de habitación es obligatorio.")]
    [Exists<RoomType>(ErrorMessage = "No existe un tipo de habitación con el identificador especificado.")]
    public int TypeId { get; set; }
}

public record RoomSequenceRequestData
{
    [Required(ErrorMessage = "El número inicial es obligatorio.")]
    public int StartNumber { get; init; }
    [Required(ErrorMessage = "El número final es obligatorio.")]
    public int EndNumber { get; init; }
    [Required(ErrorMessage = "El identificador del tipo de habitación es obligatorio.")]
    [Exists<RoomType>(ErrorMessage = "No existe un tipo de habitación con el identificador especificado.")]
    public int TypeId { get; init; }
}