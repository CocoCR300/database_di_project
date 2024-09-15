using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;
using Z.EntityFramework.Plus;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Authorize]
[Route("v{version:apiVersion}/[controller]")]
public class PaymentInformationController: BaseController
{
    private readonly AuthenticationUtil _authenticationUtil;
    private readonly IConfiguration     _configuration;
    private readonly RestifyDbContext   _context;

    public PaymentInformationController(IConfiguration configuration, AuthenticationUtil authenticationUtil, RestifyDbContext context)
    {
        _authenticationUtil = authenticationUtil;
        _configuration = configuration;
        _context = context;
    }
    
    [HttpGet]
    public ObjectResult GetUserPaymentInformation()
    {
        string? userName = _authenticationUtil.GetUserName(User);
        if (userName == null)
        {
            return Unauthorized("No se especificó un nombre de usuario.");
        }
        
        User? user = _context.User.Find(userName);
        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        _context.Entry(user).Reference(u => u.Person).Load();
        
        return Ok(_context.PaymentInformation.Where(
            p => p.PersonId == user.Person.Id));
    }

    [HttpPost]
    public ObjectResult Post(PaymentInformationRequestData requestData)
    {
        string? userName = _authenticationUtil.GetUserName(User);
        if (userName == null)
        {
            return Unauthorized("No se especificó un nombre de usuario.");
        }

        User? user = _context.User.Find(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }
        
        _context.Entry(user).Reference(u => u.Person).Load();

        PaymentInformation paymentInformation = new PaymentInformation
        {
            PersonId            = user.Person.Id,
            CardExpiryDate      = requestData.CardExpiryDate,
            CardHolderName      = requestData.CardHolderName,
            CardNumber          = requestData.CardNumber,
            CardSecurityCode    = requestData.CardSecurityCode
        };

        _context.Add(paymentInformation);
        _context.SaveChanges();
        
        return Created(new PaymentInformationResponse(paymentInformation.Id));
    }

    [HttpDelete("{paymentInformationId}")]
    public ObjectResult Delete(int paymentInformationId)
    {
        string? userName = _authenticationUtil.GetUserName(User);
        if (userName == null)
        {
            return Unauthorized("No se especificó un nombre de usuario.");
        }

        User? user = _context.User.Find(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        int rowsAffected = _context.PaymentInformation
            .Where(p => p.PersonId == user.Person.Id && p.Id == paymentInformationId)
            .Delete();

        if (rowsAffected == 0)
        {
            return NotFound("No existe información de pago con el identificador especificado asociada a este usuario.");
        }

        return Ok("La información de pago ha sido eliminada con éxito.");
    }

    [HttpPatch("{paymentInformationId}")]
    public ObjectResult Update(int paymentInformationId, PaymentInformationPatchRequestData requestData)
    {
        string? userName = _authenticationUtil.GetUserName(User);
        if (userName == null)
        {
            return Unauthorized("No se especificó un nombre de usuario.");
        }

        User? user = _context.User.Find(userName);

        if (user == null)
        {
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        PaymentInformation? paymentInformation = _context.PaymentInformation.SingleOrDefault(
            p => p.PersonId == user.Person.Id && p.Id == paymentInformationId);
        
        if (paymentInformation == null)
        {
            return NotFound("No existe información de pago con el identificador especificado asociada a este usuario.");
        }

        if (requestData.CardExpiryDate.HasValue)
            paymentInformation.CardExpiryDate = requestData.CardExpiryDate.Value;
        if (requestData.CardHolderName != null)
            paymentInformation.CardHolderName = requestData.CardHolderName;
        if (requestData.CardNumber != null)
            paymentInformation.CardNumber = requestData.CardNumber;
        if (requestData.CardSecurityCode != null)
            paymentInformation.CardSecurityCode = requestData.CardSecurityCode;

        _context.SaveChanges();

        return Ok("La información de pago ha sido actualizada con éxito.");
    }
}

public record PaymentInformationRequestData
{
    [FutureDate]
    [Required]
    public DateOnly CardExpiryDate { get; set; }
    [MaxLength(100)]
    [Required]
    public string CardHolderName { get; set; }
    [MaxLength(16)]
    [Required]
    public string CardNumber { get; set; }
    [MaxLength(4)]
    [Required]
    public string CardSecurityCode { get; set; }
}

public record PaymentInformationPatchRequestData
{
    [FutureDate]
    public DateOnly? CardExpiryDate { get; set; }
    [MaxLength(100)]
    public string? CardHolderName { get; set; }
    [MaxLength(16)]
    public string? CardNumber { get; set; }
    [MaxLength(4)]
    public string? CardSecurityCode { get; set; }
}

public record PaymentInformationResponse(int Id);