﻿using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

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

    [HttpGet("{pageSize}/{page}")]
    public ObjectResult Index(
        [FromQuery] string? lodgingName,
        [FromQuery] string? description,
        [FromQuery] string? address,
        [FromQuery] string? lodgingTypes,
        [FromQuery] string? perkIds,
        [Range(0, int.MaxValue)] int pageSize = 10,
        [Range(0, int.MaxValue)] int page = 1)
    {
        IQueryable<Lodging> lodgingsQuery = _context.Lodging
            .AsNoTracking()
            .Include(l => l.Perks)
            .Include(l => l.PhoneNumbers)
            .Include(l => l.RoomTypes)
            .Include(l => l.Owner)
            .ThenInclude(person => person.User);

        if (string.IsNullOrWhiteSpace(lodgingName))
        {
            lodgingName = string.Empty;
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            description = string.Empty;
        }
        if (string.IsNullOrWhiteSpace(address))
        {
            address = string.Empty;
        }

        lodgingsQuery = lodgingsQuery.Where(l =>
            l.Name.Contains(lodgingName)
            || l.Description.Contains(description)
            || l.Address.Contains(address));
        
        if (!string.IsNullOrWhiteSpace(lodgingTypes))
        {
            LodgingType[] lodgingTypeValues = Enum.GetValues<LodgingType>();
            bool ParseFunction(string input, out LodgingType lodgingTypeName)
            {
                if (int.TryParse(input, out int index) && index < lodgingTypeValues.Length)
                {
                    lodgingTypeName = lodgingTypeValues[index];
                    return true;
                }

                lodgingTypeName = default;
                return false;
            }

            if (TryParseCommaSeparatedList(lodgingTypes, ParseFunction, out LodgingType[]? lodgingTypeNumbers))
            {
                lodgingsQuery = lodgingsQuery.Where(l => lodgingTypeNumbers.Contains(l.Type));
            }
            else
            {
                return NotAcceptable(
                    "El parámetro 'lodgingTypes' debe ser una lista de números enteros positivos separados por comas.");
            }
            
        }
        
        // Switch to "API-side" evaluation
        IEnumerable<Lodging> lodgings = lodgingsQuery.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(perkIds))
        {
            if (TryParseCommaSeparatedList(perkIds, uint.TryParse, out uint[]? perkIdsArray))
            {
                lodgings = lodgings.Where(l => perkIdsArray.All(p => l.Perks.Any(p1 => p == p1.Id)));
            }
            else
            {
                return NotAcceptable(
                    "El parámetro 'perkIds' debe ser una lista de números enteros positivos separados por comas.");
            }
            
        }
        
        object[] lodgingObjects = lodgings.Select<Lodging, object>(lodging =>
        {
            string[] lodgingPhoneNumbers = lodging.PhoneNumbers.Select(p => p.Number).ToArray();
            string[] lodgingPhotos = lodging.Photos
                .OrderBy(p => p.Ordering)
                .Select(p => p.FileName)
                .ToArray();
            
            if (!Lodging.OffersRooms(lodging))
            {
                _context.Entry(lodging).Collection(l => l.RoomTypes);
                
                RoomType? roomType = null;

                if (lodging.RoomTypes.Count > 0)
                {
                    roomType = lodging.RoomTypes[0];  
                } 
                
                return new
                {
                    lodging.Address,
                    lodging.Description,
                    lodging.EmailAddress,
                    lodging.Id,
                    lodging.Name,
                    Owner = Models.User.MergeForResponse(lodging.Owner.User, lodging.Owner),
                    lodging.Perks,
                    PhoneNumbers = lodgingPhoneNumbers,
                    Photos = lodgingPhotos,
                    lodging.Type,
                    roomType?.PerNightPrice,
                    roomType?.Fees
                };
            }
            
            return new
            {
                lodging.Address,
                lodging.Description,
                lodging.EmailAddress,
                lodging.Id,
                lodging.Name,
                Owner = Models.User.MergeForResponse(lodging.Owner.User, lodging.Owner),
                lodging.Perks,
                PhoneNumbers = lodgingPhoneNumbers,
                Photos = lodgingPhotos,
                lodging.Type
            };
        }).ToArray();
        
        return Ok(PaginatedList<object>.Create(lodgingObjects, lodgingObjects.Length, page, pageSize));
    }

    [HttpGet("{lodgingId}")]
    public ObjectResult Get(uint lodgingId)
    {
        var lodging = _context.Lodging
            .AsNoTracking()
            .Where(l => l.Id == lodgingId)
            .Include(l => l.Owner)
            .Select(l => new
            {
                l.Address, l.Description, l.EmailAddress, l.Id, l.Name,
                l.Owner, l.Perks, l.PhoneNumbers, l.Photos, l.Type,
                l.RoomTypes
            }).SingleOrDefault();

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }
        
        if (!Lodging.TypeOffersRooms(lodging.Type))
        {
            RoomType? roomType = null;

            if (lodging.RoomTypes.Count > 0)
            {
                roomType = lodging.RoomTypes[0];  
            }

            string[] lodgingPhoneNumbers = lodging.PhoneNumbers.Select(p => p.Number).ToArray();
            string[] lodgingPhotos = lodging.Photos
                .OrderBy(p => p.Ordering)
                .Select(p => p.FileName)
                .ToArray();
            return Ok(new
            {
                lodging.Address,
                lodging.Description,
                lodging.EmailAddress,
                lodging.Id,
                lodging.Name,
                Owner = Models.User.MergeForResponse(lodging.Owner.User, lodging.Owner),
                lodging.Perks,
                PhoneNumbers = lodgingPhoneNumbers,
                Photos = lodgingPhotos,
                lodging.Type,
                roomType?.PerNightPrice,
                roomType?.Fees
            });
        }

        return Ok(lodging);
    }

    [HttpGet("type")]
    public string[] GetLodgingTypes()
    {
        string[] lodgingTypes = Enum.GetNames<LodgingType>();

        return lodgingTypes;
    }

    [HttpPost]
    public ObjectResult Post(LodgingRequestData data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        List<Room> rooms = null;
        List<RoomType> roomTypes = null;
        if (!Lodging.TypeOffersRooms(data.Type))
        {
            if (!data.PerNightPrice.HasValue || !data.Fees.HasValue)
            {
                return NotAcceptable(
                    "El costo por noche y el impuesto es obligatorio para el tipo de alojamiento especificado.");
            }

            decimal perNightPrice = data.PerNightPrice.Value,
                    fees = data.Fees.Value;
            roomTypes = new List<RoomType>
            {
                new RoomType
                {
                    PerNightPrice = perNightPrice,
                    Fees = fees
                }
            };
            rooms = new List<Room>
            {
                new Room(roomTypes[0])
                {
                    Number = 0
                }
            };
        }

        Lodging lodging = new Lodging(rooms, roomTypes)
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

        return Created(lodging);

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
        
        bool noneExists = true;
        int count = fileNames.Length;
        List<string> fileNamesToDelete = new List<string>(fileNames.Length);
        for (int i = 0; count > 0 && i < lodging.Photos.Count; ++i)
        {
            string fileName = lodging.Photos[i].FileName;
            if (Array.Exists(fileNames, n => n == fileName))
            {
                --count;
                noneExists = false;
                lodging.Photos.RemoveAt(i);
                fileNamesToDelete.Add(fileName);
            }
        }
                
        _context.SaveChanges();

        string? filesPath = _configuration["LodgingImageFilesPath"];
        foreach (string fileName in fileNamesToDelete)
        {
            string path = Path.Combine(filesPath, fileName);
            System.IO.File.Delete(path);
        }

        if (noneExists)
        {
            return NotFound("El alojamiento no tiene ninguna de las fotos especificadas.");
        }
                
        return Ok("Las fotos han sido eliminadas con éxito.");
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

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        foreach (string phoneNumber in phoneNumbers)
        {
            lodging.PhoneNumbers.Add(new LodgingPhoneNumber
            {
                LodgingId = lodging.Id,
                Number = phoneNumber
            });
        }
        _context.SaveChanges();
        return Created(phoneNumbers);

    }

    [HttpPost("{lodgingId}/photo")]
    public async Task<ObjectResult> StorePhotos(uint lodgingId, [FromForm] IFormFileCollection files)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        if (files.Count > 10)
        {
            return NotAcceptable("Puede agregar un máximo de 10 fotos por solicitud.");
        }

        if (lodging.Photos.Count == 100)
        {
            return NotAcceptable("Puede agregar un máximo de 100 fotos por alojamiento.");
        }
        
        byte order = lodging.Photos.MaxBy(p => p.Ordering)?.Ordering ?? 0;
        string[] fileNames = new string[files.Count];
        for (int i = 0; i < files.Count; ++i)
        {
            IFormFile file = files[i];
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            string fileName;
            if (!fileExtension.Equals(string.Empty))
            {
                fileName = $"{Guid.NewGuid().ToString()}{fileExtension}";
            }
            else
            {
                fileName = Guid.NewGuid().ToString();
            }

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
        Directory.CreateDirectory(path);
        for (int i = 0; i < files.Count; ++i)
        {
            var file = files[i];
            string fileName = fileNames[i];
            string filePath = Path.Combine(path, fileName);
            tasks.Add(DataUtil.SaveFile(filePath, file));
        }

        await Task.WhenAll(tasks);

        return Created(path, fileNames);
    }
    
    [HttpPatch("{lodgingId}")]
    public ObjectResult Update(string lodgingId, LodgingPatchRequestData data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Lodging? lodging = _context.Find<Lodging>(lodgingId);
        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el nombre especificado.");
        }

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
    public decimal? Fees { get; set; }
    public decimal? PerNightPrice { get; set; }
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