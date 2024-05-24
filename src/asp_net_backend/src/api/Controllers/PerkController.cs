using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Restify.API.Data;
using Restify.API.Models;
using Z.EntityFramework.Plus;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class PerkController : BaseController
{
    private readonly RestifyDbContext _context;

    public PerkController(RestifyDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IEnumerable<Perk> Get()
    {
        return _context.Perks;
    }

    [HttpPost]
    public ObjectResult Post(string[] perkNames)
    {
        foreach (string name in perkNames)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Los nombres de beneficios no pueden estar vacíos.");
            }

            if (name.Length > 50)
            {
                return BadRequest("Los nombres de beneficios pueden tener 50 carácteres como máximo.");
            }
            
            Perk perk = new Perk
            {
                Name = name
            };

            _context.Perks.Add(perk);
        }

        try
        {
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            if (ex.InnerException is MySqlException
                    { ErrorCode: MySqlErrorCode.DuplicateUnique or MySqlErrorCode.DuplicateKeyEntry })
            {
                return NotAcceptable("Ya existe un beneficio con uno de los nombres especificados.");
            }
            
            return NotAcceptable("Ha ocurrido un error al insertar los datos.");
        }
        
        return Ok("Los beneficios han sido agregados con éxito.");
    }

    [HttpDelete]
    public ObjectResult Delete(uint[] perkIds)
    {
        int rows;
        try
        {
            rows = _context.Perks.Where(p => perkIds.Contains(p.Id)).Delete();
        }
        catch (Exception ex)
        {
            return NotAcceptable("Ha ocurrido un error al eliminar los datos.");
        }

        if (rows == 0)
        {
            return NotFound("No existe ningún beneficio con los identificadores especificados.");
        }

        const string message = "Los beneficios han sido eliminados con éxito.";
        if (rows != perkIds.Length)
        {
            return Ok(
                $"{message} Algunos de los identificadores no correspondieron a beneficios registrados.");
        }

        return Ok(message);
    }

    [HttpPatch]
    public ObjectResult Update(PerkUpdateRequestData[] data)
    {
        if (ModelState.IsValid)
        {
            IEnumerable<uint> perkIds = data.Select(p => p.Id);
            var existingPerks = _context.Perks.Where(p => perkIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);
            if (existingPerks.Count == 0)
            {
                return NotAcceptable("No existe ningún beneficio con los identificadores especificados.");
            }

            bool allExists = true;
            foreach (var perkData in data)
            {
                if (existingPerks.TryGetValue(perkData.Id, out var perk))
                {
                    perk.Name = perkData.Name;
                }
                else
                {
                    allExists = false;
                }
            }

            _context.SaveChanges();

            const string message = "Los beneficios han sido actualizados con éxito.";
            if (!allExists)
            {
                return Ok($"{message} Algunos de los identificadores no correspondieron a beneficios registrados.");
            }
            
            return Ok(message);
        }

        return BadRequest("Datos inválidos");
    }
}

public class PerkUpdateRequestData
{
    [Required(ErrorMessage = "El identificador del beneficio adicional es obligatorio.")]
    public uint     Id { get; set; }
    [Required(ErrorMessage = "El nombre del beneficio adicional es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre del beneficio adicional debe tener 50 carácteres como máximo.")]
    public string   Name { get; set; }
}