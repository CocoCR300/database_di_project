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
public class LodgingController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly RestifyDbContext _context;

    public LodgingController(IConfiguration configuration, RestifyDbContext context)
    {
        _configuration = configuration;
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

    [HttpGet("type")]
    public string[] GetLodgingTypes()
    {
        string[] lodgingTypes = Enum.GetNames<LodgingType>();

        return lodgingTypes;
    }
    
    [HttpGet("{lodgingId}/room")]
    public ObjectResult GetRooms(uint lodgingId)
    {
        Lodging? lodging = _context.Lodging.Find(lodgingId);

        if (lodging != null)
        {
            _context.Entry(lodging).Collection(l => l.Rooms).Load();
            var rooms = lodging.Rooms
                .Select(r => new { r.Number, r.TypeId });
            return Ok(rooms);
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
            var roomTypes = lodging.RoomTypes
                .Select(r => new { r.Id, r.Name, r.Capacity, r.PerNightPrice, r.Fees });
            return Ok(roomTypes);
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

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

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
    
    [HttpDelete("{lodgingId}/photo")]
    public ObjectResult DeletePhotos(uint lodgingId, string[] fileNames)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        string[] existingFileNames = lodging.Photos
            .Select(p => p.FileName)
            .OrderBy(n => n)
            .ToArray();
        
        bool noneExists = true;
        foreach (var fileName in fileNames)
        {
            int index = Array.BinarySearch(existingFileNames, fileName);
            if (index > 0)
            {
                noneExists = false;
                lodging.Photos.RemoveAt(index);
            }
        }
                
        _context.SaveChanges();

        if (noneExists)
        {
            return NotFound("El alojamiento no tiene ninguna de las fotos especificadas.");
        }
                
        return Ok("Los números de teléfono han sido eliminados con éxito.");

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

    [HttpPost("{lodgingId}/photo")]
    public async Task<ObjectResult> StorePhotos(uint lodgingId, PhotoRequestData[] photosData)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        if (photosData.Length > 10)
        {
            return NotAcceptable("Puede agregar un máximo de 10 fotos por solicitud.");
        }

        if (lodging.Photos.Count == 100)
        {
            return NotAcceptable("Puede agregar un máximo de 100 fotos por alojamiento.");
        }
        
        byte order = lodging.Photos.MaxBy(p => p.Ordering)?.Ordering ?? 0;
        string[] fileNames = new string[photosData.Length];
        for (int i = 0; i < photosData.Length; ++i)
        {
            PhotoRequestData data = photosData[i];
            
            string fileName = $"{Guid.NewGuid().ToString()}.{data.ImageFileExtension}";

            fileNames[i] = fileName;
            lodging.Photos.Add(new LodgingPhoto
            {
                FileName = fileName,
                LodgingId = lodging.Id,
                Ordering = order
            });

            ++order;
        }

        await _context.SaveChangesAsync();

        List<Task> tasks = new List<Task>();
        string? path = _configuration["LodgingImageFilesPath"];
        for (int i = 0; i < photosData.Length; ++i)
        {
            var data = photosData[i];
            string fileName = fileNames[i];
            string filePath = Path.Combine(path, fileName);
            byte[] imageBytes = Convert.FromBase64String(data.ImageBase64);
            tasks.Add(System.IO.File.WriteAllBytesAsync(filePath, imageBytes));
        }

        await Task.WhenAll(tasks);

        return Ok("Los beneficios han sido agregados con éxito.");

    }
    
    [HttpPost("{lodgingId}/room_type")]
    public ObjectResult StoreRoomTypes(uint lodgingId, RoomTypeRequestData[] roomTypes)
    {
        if (ModelState.IsValid)
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
                        PerNightPrice = roomType.PerNightPrice,
                        Fees = roomType.Fees
                    });
                }

                try
                {
                    _context.Database.BeginTransaction();
                    _context.SaveChanges();
                    _context.Database.CommitTransaction();
                    
                    return Ok("Los tipos de habitación han sido agregados con éxito.");
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    return NotAcceptable("Ha ocurrido un error al insertar los datos.");
                }
                
            }
                
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }
        
        return BadRequest("Datos inválidos.");
    }
        
    [HttpPatch("{bookingId}")]
    public ObjectResult Update(string bookingId, LodgingPatchRequestData data)
    {
        if (ModelState.IsValid)
        {
            Lodging? lodging = _context.Find<Lodging>(bookingId);

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
            
        return BadRequest("Datos inválidos.");
    }
}


public class LodgingRequestData
{
    [Required(ErrorMessage = "La dirección del alojamiento es obligatoria.")]
    [MaxLength(300, ErrorMessage = "La dirección debe tener 300 carácteres como máximo.")]
    public string   Address { get; set; }
    [Required(AllowEmptyStrings = true)]
    [MaxLength(1000, ErrorMessage = "La descripción del alojamiento debe tener 1000 carácteres como máximo.")]
    public string   Description { get; set; }
    [Required(ErrorMessage = "El nombre del alojamiento es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre del alojamiento debe tener 100 carácteres como máximo.")]
    public string   Name { get; set; }
    [Required(ErrorMessage = "El tipo del alojamiento es obligatorio.")]
    public LodgingType Type { get; set; }
    [Required(ErrorMessage = "El correo electrónico del alojamiento es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El correo electrónico debe tener 200 carácteres como máximo.")]
    [EmailAddress(ErrorMessage = "El correo electrónico es inválido.")]
    public string   EmailAddress { get; set; }
    [Required(ErrorMessage = "El identificador del arrendador es obligatorio.")]
    [Exists<Person>(ErrorMessage = "No existe un arrendador con el identificador especificado.")]
    public uint     OwnerId { get; set; }
}
    
public class LodgingPatchRequestData
{
    [MaxLength(300, ErrorMessage = "La dirección debe tener 300 carácteres como máximo.")]
    public string?   Address { get; set; }
    [MaxLength(1000, ErrorMessage = "La descripción del alojamiento debe tener 1000 carácteres como máximo.")]
    public string?   Description { get; set; }
    [MaxLength(100, ErrorMessage = "El nombre del alojamiento debe tener 100 carácteres como máximo.")]
    public string?   Name { get; set; }
    public LodgingType?   Type { get; set; }
    [MaxLength(200, ErrorMessage = "El correo electrónico debe tener 200 carácteres como máximo.")]
    [EmailAddress(ErrorMessage = "El correo electrónico es inválido.")]
    public string?   EmailAddress { get; set; }
    [Exists<Person>(ErrorMessage = "No existe un arrendador con el identificador especificado.")]
    public uint?    OwnerId { get; set; }
}

public class RoomTypeRequestData 
{
    [Required(ErrorMessage = "El impuesto del tipo de habitación es obligatorio.")]
    public decimal Fees { get; set; }
    [Required(ErrorMessage = "El precio por noche del tipo de habitación es obligatorio.")]
    public decimal PerNightPrice { get; set; }
    [Required(ErrorMessage = "El nombre del tipo de habitación es obligatorio.")]
    public string Name { get; set; }
    [Required(ErrorMessage = "La capacidad del tipo de habitación es obligatorio.")]
    public uint Capacity { get; set; }
}

public class PhotoRequestData
{
    [Required]
	public string ImageBase64 { get; set; }
    [Required]
	public string ImageFileExtension { get; set; }
}