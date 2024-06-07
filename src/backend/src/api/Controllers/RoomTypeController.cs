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
public class RoomTypeController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly RestifyDbContext _context;

    public RoomTypeController(IConfiguration configuration, RestifyDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    [HttpGet("{lodgingId}")]
    public ObjectResult GetRoomTypes(uint lodgingId,
        [FromQuery] uint? minCapacity,
        [FromQuery] decimal? minPerNightPrice,
        [FromQuery] decimal? maxPerNightPrice)
    {
        Lodging? lodging = _context.Lodging.Find(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }
        
        _context.Entry(lodging).Collection(l => l.RoomTypes).Load();
        var roomTypes = lodging.RoomTypes
            .Select(r =>
            {
                string[] roomTypePhotos = r.Photos
                    .OrderBy(p => p.Ordering)
                    .Select(p => p.FileName)
                    .ToArray();
                return new
                {
                    r.Id,
                    r.Name,
                    r.Capacity,
                    r.PerNightPrice,
                    r.Fees,
                    Photos = roomTypePhotos
                };
            });

        if (minCapacity.HasValue)
        {
            roomTypes = roomTypes.Where(r => r.Capacity >= minCapacity);
        }

        if (minPerNightPrice.HasValue)
        {
            roomTypes = roomTypes.Where(r => r.PerNightPrice >= minPerNightPrice);
        }
        
        if (maxPerNightPrice.HasValue)
        {
            roomTypes = roomTypes.Where(r => r.PerNightPrice <= maxPerNightPrice);
        }
        
        return Ok(roomTypes);
    }
    
    [HttpDelete("{lodgingId}")]
    public ObjectResult DeleteRoomTypes(uint lodgingId, uint[] roomTypeIds)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }

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
    
    [HttpDelete("{lodgingId}/{roomTypeId}/photo")]
    public ObjectResult DeleteRoomTypePhotos(uint lodgingId, uint roomTypeId, string[] fileNames)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        ObjectResult? result = StandardValidations.ValidateLodging(this, lodging);
        if (result != null)
        {
            return result;
        }
        
        _context.Entry(lodging)
            .Collection(l => l.RoomTypes)
            .Load();

        RoomType? roomType = lodging.RoomTypes.FirstOrDefault(r => r.Id == roomTypeId);
        if (roomType == null)
        {
            return NotFound($"No existe un tipo de habitación con el identificador {roomTypeId} en este alojamiento.");
        }
        
        bool noneExists = true;
        int count = fileNames.Length;
        List<string> fileNamesToDelete = new List<string>(fileNames.Length);
        for (int i = 0; count > 0 && i < roomType.Photos.Count; ++i)
        {
            string fileName = roomType.Photos[i].FileName;
            if (Array.Exists(fileNames, n => n == fileName))
            {
                --count;
                noneExists = false;
                roomType.Photos.RemoveAt(i);
                fileNamesToDelete.Add(fileName);
            }
        }
                
        _context.SaveChanges();

        string? filesPath = _configuration["RoomTypeImageFilesPath"];
        foreach (string fileName in fileNamesToDelete)
        {
            string path = Path.Combine(filesPath, fileName);
            System.IO.File.Delete(path);
        }

        if (noneExists)
        {
            return NotFound("El tipo de habitación no tiene ninguna de las fotos especificadas.");
        }
                
        return Ok("Las fotos han sido eliminadas con éxito.");
    }
    
    [HttpPost("{lodgingId}")]
    public ObjectResult StoreRoomTypes(uint lodgingId, RoomTypeRequestData[] roomTypes)
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
                    
            return Created();
        }
        catch (Exception)
        {
            _context.Database.RollbackTransaction();
            return NotAcceptable("Ha ocurrido un error al insertar los datos.");
        }
    }
        
    [HttpPost("{lodgingId}/{roomTypeId}/photo")]
    public async Task<ObjectResult> StoreRoomTypePhotos(uint lodgingId, uint roomTypeId, [FromForm] IFormFileCollection files)
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

        if (files.Count > 10)
        {
            return NotAcceptable("Puede agregar un máximo de 10 fotos por solicitud.");
        }

        _context.Entry(lodging).Collection(l => l.RoomTypes).Load();
        RoomType? roomType = lodging.RoomTypes.FirstOrDefault(r => r.Id == roomTypeId);

        if (roomType == null)
        {
            return NotFound($"No existe un tipo de habitación con el identificador {roomTypeId} en el alojamiento.");
        }
        
        if (roomType.Photos.Count == 100)
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
            roomType.Photos.Add(new RoomTypePhoto 
            {
                FileName = fileName,
                RoomTypeId = roomTypeId,
                Ordering = order
            });

            ++order;
        }

        await _context.SaveChangesAsync();

        List<Task> tasks = new List<Task>();
        string? path = _configuration["RoomTypeImageFilesPath"];
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
    
    [HttpPatch("{lodgingId}/{roomTypeId}")]
    public ObjectResult Update(string lodgingId, uint roomTypeId, RoomTypePatchRequestData data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Lodging? lodging = _context.Find<Lodging>(lodgingId);
        if (lodging == null)
        {
            return StandardResponses.IdDoesNotExist(this, "alojamiento");
        }

        _context.Entry(lodging).Collection(l => l.RoomTypes).Load();

        RoomType? type = lodging.RoomTypes.SingleOrDefault(r => r.Id == roomTypeId);
        if (type == null)
        {
            return StandardResponses.IdDoesNotExist(this, "tipo de habitación");
        }
        
        if (data.Capacity.HasValue)
            type.Capacity = data.Capacity.Value;
        if (data.Fees.HasValue)
            type.Fees = data.Fees.Value;
        if (data.PerNightPrice.HasValue)
            type.PerNightPrice = data.PerNightPrice.Value;
        if (data.Name != null)
            type.Name = data.Name;

        _context.SaveChanges();
        return Ok("La modificación del alojamiento ha sido realizada con éxito.");
    }
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

public class RoomTypePatchRequestData 
{
    public decimal? Fees { get; set; }
    public decimal? PerNightPrice { get; set; }
    [NotEmptyOrWhiteSpace]
    public string? Name { get; set; }
    public uint? Capacity { get; set; }
}
