using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class LodgingController : Controller
{
    private readonly RestifyDbContext _context;

    public LodgingController(RestifyDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IEnumerable<Lodging> Get()
    {
        var lodgings = _context.Lodging
            .Include(l => l.Perks)
            .Include(l => l.PhoneNumbers)
            .Include(l => l.Owner)
            .ThenInclude(p => p.User);
        return lodgings;
    }

    [HttpGet("{lodgingId}")]
    public ObjectResult Get(uint lodgingId)
    {
        var lodging = _context.Lodging.Find(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Reference(l => l.Owner).Load();
            _context.Entry(lodging).Collection(l => l.Perks).Load();
            _context.Entry(lodging).Collection(l => l.PhoneNumbers).Load();
            _context.Entry(lodging.Owner).Reference(p => p.User).Load();

            return Ok(lodging);
        }

        return NotFound("No existe un alojamiento con el identificador especificado.");
    }

    [HttpGet("{lodgingId}/booking")]
    public IEnumerable<Booking> GetBookings(uint lodgingId)
    {
        IQueryable<Booking> bookings = _context.Booking.Where(b => b.LodgingId == lodgingId)
            .Include(b => b.Customer)
            .Include(b => b.Payment)
            .Include(b => b.RoomBookings);
        return bookings;
    }
    
    [HttpGet("{lodgingId}/room")]
    public ObjectResult GetRooms(uint lodgingId)
    {
        Lodging? lodging = _context.Lodging.Find(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.Rooms).Load();
            return Ok(lodging.Rooms);
        }
        
        return NotFound("No existe un alojamiento con el identificador especificado");
    }
    
    [HttpGet("{lodgingId}/room_type")]
    public ObjectResult GetRoomTypes(uint lodgingId)
    {
        Lodging? lodging = _context.Lodging.Find(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.RoomTypes).Load();
            return Ok(lodging.RoomTypes);
        }
        
        return NotFound("No existe un alojamiento con el identificador especificado");
    }
        
    [HttpPost]
    public ObjectResult Post(LodgingRequestData data)
    {
        if (ModelState.IsValid)
        {
            Lodging lodging = new Lodging
            {
                Name = data.Name,
                Address = data.Address,
                Description = data.Description,
                Type = data.Type,
                EmailAddress = data.EmailAddress,
                OwnerId = data.OwnerId
            };

            _context.Lodging.Add(lodging);
            _context.SaveChanges();

            return Ok("El alojamiento ha sido creado con éxito");
        }
            
        return BadRequest(ModelState);
    }

    [HttpDelete]
    public ObjectResult Delete(uint id)
    {
        Lodging? lodging = _context.Find<Lodging>(id);

        if (lodging != null)
        {
            _context.Remove(lodging);
            _context.SaveChanges();
                
            return Ok("El alojamiento ha sido eliminado con éxito.");
        }

        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
        
    [HttpDelete("{lodgingId}/perk")]
    public ObjectResult DeletePerks(uint lodgingId, uint[] perkIds)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.Perks).Load();
            
            bool noneExists = true;
            for (int i = 0; i < lodging.Perks.Count; ++i)
            {
                uint perkId = lodging.Perks[i].Id;
                if (perkIds.Contains(perkId))
                {
                    noneExists = false;
                    lodging.Perks.RemoveAt(i);
                }
            }
                
            _context.SaveChanges();

            if (noneExists)
            {
                return NotFound(
                    "El alojamiento no tiene asignado ninguno de los beneficios especificados.");
            }
                
            return Ok("Los beneficios han sido eliminados con éxito del alojamiento.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
    
    [HttpDelete("{lodgingId}/phone_number")]
    public ObjectResult DeletePhoneNumbers(uint lodgingId, string[] phoneNumbers)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.PhoneNumbers).Load();
            
            bool noneExists = true;
            for (int i = 0; i < lodging.PhoneNumbers.Count; ++i)
            {
                string phoneNumber = lodging.PhoneNumbers[i].Number;
                if (phoneNumbers.Contains(phoneNumber))
                {
                    noneExists = false;
                    lodging.PhoneNumbers.RemoveAt(i);
                }
            }
                
            _context.SaveChanges();

            if (noneExists)
            {
                return NotFound(
                    "El alojamiento no tiene asignado ninguno de los números de teléfono especificados.");
            }
                
            return Ok("Los números de teléfono han sido eliminados con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
        
    [HttpDelete("{lodgingId}/room")]
    public ObjectResult DeleteRooms(uint lodgingId, uint[] roomNumbers)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.Rooms).Load();
            
            bool noneExists = true;
            for (int i = 0; i < lodging.Rooms.Count; ++i)
            {
                uint roomNumber = lodging.Rooms[i].Number;
                if (roomNumbers.Contains(roomNumber))
                {
                    noneExists = false;
                    lodging.Rooms.RemoveAt(i);
                }
            }
                
            _context.SaveChanges();

            if (noneExists)
            {
                return NotFound(
                    "El alojamiento no tiene ninguno de los números de habitación especificados.");
            }
                
            return Ok("Las habitaciones han sido eliminadas con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
    
    [HttpDelete("{lodgingId}/room_type")]
    public ObjectResult DeleteRoomTypes(uint lodgingId, uint[] roomTypeIds)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.RoomTypes).Load();
            
            bool noneExists = true;
            for (int i = 0; i < lodging.RoomTypes.Count; ++i)
            {
                uint roomTypeId = lodging.RoomTypes[i].Id;
                if (roomTypeIds.Contains(roomTypeId))
                {
                    noneExists = false;
                    lodging.RoomTypes.RemoveAt(i);
                }
            }
                
            _context.SaveChanges();

            if (noneExists)
            {
                return NotFound(
                    "El alojamiento no tiene ninguno de los tipos de habitación especificados.");
            }
                
            return Ok("Las tipos de habitación han sido eliminadas con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
    
    [HttpPost("{lodgingId}/perk")]
    public ObjectResult StorePerks(uint lodgingId, uint[] perkIds)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            bool noneExists = true;
            IEnumerable<Perk> perks = _context.Perks.Where(p => perkIds.Contains(p.Id)).AsEnumerable();
            foreach (Perk perk in perks)
            {
                noneExists = false;
                lodging.Perks.Add(perk);
            }
                
            _context.SaveChanges();

            if (noneExists)
            {
                return NotFound("No existe ningún beneficio con los identificadores especificados.");
            }
                
            return Ok("Los beneficios han sido agregados con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
        
    [HttpPost("{lodgingId}/phone_number")]
    public ObjectResult StorePhoneNumbers(uint lodgingId, string[] phoneNumbers)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            foreach (string phoneNumber in phoneNumbers)
            {
                lodging.PhoneNumbers.Add(new LodgingPhoneNumber
                {
                    LodgingId = lodging.Id,
                    Number = phoneNumber
                });
            }
            _context.SaveChanges();
            return Ok("Los números de teléfono han sido agregados con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }

    [HttpPost("{lodgingId}/room")]
    public ObjectResult StoreRooms(uint lodgingId, RoomRequestData[] rooms)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            foreach (RoomRequestData room in rooms)
            {
                lodging.Rooms.Add(new Room
                {
                    LodgingId = lodging.Id,
                    Number = room.Number,
                    TypeId = room.TypeId
                });
            }
            
            _context.SaveChanges();
            return Ok("Las habitaciones han sido agregadas con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
    
    [HttpPost("{lodgingId}/room_type")]
    public ObjectResult StoreRoomTypes(uint lodgingId, RoomTypeRequestData[] roomTypes)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging != null)
        {
            foreach (RoomTypeRequestData roomType in roomTypes)
            {
                lodging.RoomTypes.Add(new RoomType 
                {
                    LodgingId = lodging.Id,
                    Name = roomType.Name,
                    Capacity = roomType.Capacity,
                    PerNightPrice = roomType.PerNightPrice
                });
            }
            
            _context.SaveChanges();
            return Ok("Los tipos de habitación han sido agregados con éxito.");
        }
            
        return NotFound("No existe un alojamiento con el identificador especificado.");
    }
        
    [HttpPatch("{bookingId}")]
    public ObjectResult Update(string bookingId, LodgingPatchRequestData data)
    {
        Lodging? lodging = _context.Find<Lodging>(bookingId);

        if (ModelState.IsValid)
        {
            if (lodging != null)
            {
                if (data.OwnerId != null)
                    lodging.OwnerId = data.OwnerId.Value;
                    
                if (data.Name != null)
                    lodging.Name = data.Name;
                if (data.Description != null)
                    lodging.Description = data.Description;
                if (data.Address != null)
                    lodging.Address = data.Address;
                if (data.Type != null)
                    lodging.Type = data.Type.Value;
                if (data.EmailAddress != null)
                    lodging.EmailAddress = data.EmailAddress;

                _context.SaveChanges();
                return Ok("La modificación del alojamiento ha sido realizada con éxito.");
            }
                
            return NotFound("No existe un alojamiento con el nombre especificado.");
        }
            
        return new ObjectResult("Datos inválidos.")
        {
            StatusCode = StatusCodes.Status406NotAcceptable
        };
    }
}


public class LodgingRequestData
{
    [Required]
    [Unique<Lodging>]
    [MaxLength(300)]
    public string   Address { get; set; }
    [Required]
    [MaxLength(1000)]
    public string   Description { get; set; }
    [Required]
    [MaxLength(100)]
    public string   Name { get; set; }
    [Required]
    public LodgingType Type { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string   EmailAddress { get; set; }
    [Required]
    [Exists<Person>]
    public uint     OwnerId { get; set; }
}
    
public class LodgingPatchRequestData
{
    [Unique<Lodging>]
    [MaxLength(300)]
    public string?   Address { get; set; }
    [MaxLength(1000)]
    public string?   Description { get; set; }
    [MaxLength(100)]
    public string?   Name { get; set; }
    public LodgingType?   Type { get; set; }
    [EmailAddress]
    [MaxLength(200)]
    public string?   EmailAddress { get; set; }
    [Exists<Person>]
    public uint?    OwnerId { get; set; }
}

public class RoomRequestData 
{
    [Required]
    public uint Number { get; set; }
    [Required]
    [Exists<RoomType>]
    public uint TypeId { get; set; }
}

public class RoomTypeRequestData 
{
    [Required]
    public decimal PerNightPrice { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public uint Capacity { get; set; }
}