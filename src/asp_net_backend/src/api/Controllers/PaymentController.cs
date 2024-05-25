using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Restify.API.Data;
using Restify.API.Models;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class PaymentController : BaseController
{
    private readonly IConfiguration     _configuration;
    private readonly RestifyDbContext   _context;

    public PaymentController(IConfiguration configuration, RestifyDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    [HttpGet("user/{userName}")]
    public ObjectResult Get(string userName)
    {
        User? user = _context.Find<User>(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        _context.Entry(user).Reference(u => u.Person).Load();

        Payment[] payments = _context.Booking
            .Where(b => b.CustomerId == user.Person.Id && b.Payment != null)
            .Select(b => b.Payment!)
            .ToArray();

        return Ok(payments);
    }
    
    [HttpGet("lodging/{lodgingId}")]
    public ObjectResult Get(uint lodgingId)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        Payment[] payments = _context.Booking
            .Where(b => b.LodgingId == lodgingId && b.Payment != null)
            .Select(b => b.Payment!)
            .ToArray();

        return Ok(payments);
    }
    
    [HttpPost("{bookingId}")]
    public ObjectResult Store(uint bookingId, PaymentRequestData data)
    {
        Booking? booking = _context.Find<Booking>(bookingId);

        if (booking == null)
        {
            return NotFound("No existe una reservación con el identificador especificado.");
        }

        if (booking.Payment != null)
        {
            return NotFound("Ya hay un pago asociado a esta reservación.");
        }

        string fileName = $"{Guid.NewGuid().ToString()}.{data.InvoiceImageFileExtension}";
        
        booking.Payment = new Payment
        {
            DateAndTime = data.DateAndTime,
            Amount = data.Amount,
            InvoiceImageFileName = fileName,
            BookingId = booking.Id
        };
        _context.SaveChanges();

        string? path = _configuration["InvoiceImageFilesPath"];
        string filePath = Path.Combine(path, fileName);
        byte[] imageBytes = Convert.FromBase64String(data.InvoiceImageBase64);
        System.IO.File.WriteAllBytes(filePath, imageBytes);
        
        return Ok(booking.Payment);
    }
}

public record PaymentRequestData
{
    [Required]
	public DateTimeOffset	DateAndTime { get; set; }
    [Required]
	public decimal			Amount { get; set; }
    [Required]
	public string           InvoiceImageBase64 { get; set; }
    [Required]
	public string           InvoiceImageFileExtension { get; set; }
}