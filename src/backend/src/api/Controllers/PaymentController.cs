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
public class PaymentController : BaseController
{
    private readonly IConfiguration     _configuration;
    private readonly RestifyDbContext   _context;

    public PaymentController(IConfiguration configuration, RestifyDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    [HttpGet("user/{userName}/{pageSize}/{page}")]
    public async Task<ObjectResult> Get(string userName,
        [FromQuery] uint? lodgingId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [Range(0, int.MaxValue)] int pageSize = 10,
        [Range(0, int.MaxValue)] int page = 1)
    {
        User? user = _context.Find<User>(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        _context.Entry(user).Reference(u => u.Person).Load();

        IQueryable<Booking> bookings = _context.Booking
            .Where(b => b.CustomerId == user.Person.Id && b.Payment != null);

        bookings = StandardFilters.BookingByLodging(bookings, lodgingId);
        bookings = StandardFilters.BookingByDates(bookings, startDate, endDate);

        IQueryable<Payment> payments = bookings.Select(b => b.Payment!);
        return Ok(await PaginatedList<Payment>.CreateAsync(payments, pageSize, page));
    }
    
    [HttpGet("lodging/{lodgingId}")]
    public async Task<ObjectResult> Get(uint lodgingId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [Range(0, int.MaxValue)] int pageSize = 10,
        [Range(0, int.MaxValue)] int page = 1)
    {
        Lodging? lodging = _context.Find<Lodging>(lodgingId);

        if (lodging == null)
        {
            return NotFound("No existe un alojamiento con el identificador especificado.");
        }

        IQueryable<Booking> bookings = _context.Booking
            .Where(b => b.LodgingId == lodging.Id && b.Payment != null);

        bookings = StandardFilters.BookingByDates(bookings, startDate, endDate);

        IQueryable<Payment> payments = bookings.Select(b => b.Payment!);
        return Ok(await PaginatedList<object>.CreateAsync(payments, page, pageSize));
    }
    
    [HttpPost("{bookingId}")]
    public ObjectResult Store(uint bookingId, [FromForm] PaymentRequestData data)
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

        string fileExtension = Path.GetExtension(data.InvoiceImageFile.FileName).ToLowerInvariant();
        
        string fileName;
        if (!fileExtension.Equals(string.Empty))
        {
            fileName = $"{Guid.NewGuid().ToString()}{fileExtension}";
        }
        else
        {
            fileName = Guid.NewGuid().ToString();
        }
        
        booking.Payment = new Payment
        {
            DateAndTime = data.DateAndTime,
            Amount = data.Amount,
            InvoiceImageFileName = fileName,
            BookingId = booking.Id
        };
        _context.SaveChanges();

        string? path = _configuration["InvoiceImageFilesPath"];
        Directory.CreateDirectory(path);
        
        string filePath = Path.Combine(path, fileName);

        using (FileStream fileStream = System.IO.File.Create(filePath))
        {
            data.InvoiceImageFile.CopyTo(fileStream);
        }
        
        return Created(booking.Payment);
    }
}

public record PaymentRequestData
{
    [Required]
	public DateTimeOffset	DateAndTime { get; set; }
    [Required]
	public decimal			Amount { get; set; }
	public IFormFile        InvoiceImageFile { get; set; }
}