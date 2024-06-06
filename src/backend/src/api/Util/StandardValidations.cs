using Microsoft.AspNetCore.Mvc;
using Restify.API.Controllers;
using Restify.API.Models;

namespace Restify.API.Util;

public static class StandardValidations
{
    public static ObjectResult? ValidateLodging(BaseController controller, Lodging? lodging)
    {
        if (lodging == null)
        {
            return controller.NotFound("No existe un alojamiento con el identificador especificado.");
        }
        
        if (!Lodging.OffersRooms(lodging))
        {
            return StandardResponses.LodgingDoesNotOfferRooms(controller, lodging);
        }

        return null;
    }
}