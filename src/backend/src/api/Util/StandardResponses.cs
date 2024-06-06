using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Restify.API.Controllers;
using Restify.API.Models;

namespace Restify.API.Util;

public static class StandardResponses
{
    public static ObjectResult IdDoesNotExist(BaseController controller, string entityName, string identifierName = "identificador")
    {
        return controller.NotFound($"No existe un {entityName} con el {identifierName} especificado.");
    }
    
    public static ObjectResult LodgingDoesNotOfferRooms(BaseController controller, Lodging lodging)
    {
        return controller.NotAcceptable(
            $"Este alojamiento es del tipo {lodging.Type} y no ofrece arrendamiento por habitaciones");
    }
}