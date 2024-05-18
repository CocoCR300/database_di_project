using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;

namespace Restify.API.Controllers
{
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
                    LodgingType = data.LodgingType,
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
            Lodging lodging = _context.Find<Lodging>(id);

            if (lodging != null)
            {
                _context.Remove(lodging);
                _context.SaveChanges();
                
                return Ok("El alojamiento ha sido eliminado con éxito.");
            }

            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        [HttpPatch("{bookingId}")]
        public ObjectResult Update(string bookingId, LodgingPatchRequestData data)
        {
            Lodging lodging = _context.Find<Lodging>(bookingId);

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
                    if (data.LodgingType != null)
                        lodging.LodgingType = data.LodgingType;

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
        // TODO: One of restriction
        public string   LodgingType { get; set; }
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
        // TODO: One of restriction
        public string?   LodgingType { get; set; }
        [Exists<Person>]
		public uint?    OwnerId { get; set; }
	}
}
