using Microsoft.AspNetCore.Mvc;

namespace Restify.API.Controllers;

public class BaseController : Controller
{
    public ObjectResult NotAcceptable(string message)
    {
        return new ObjectResult(message)
        {
            StatusCode = StatusCodes.Status406NotAcceptable
        };
    }
}