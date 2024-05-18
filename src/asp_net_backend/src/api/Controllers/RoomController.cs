using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class RoomController : Controller
{
    [HttpGet]
    public ObjectResult Get()
    {
        return Ok("Not implemented");
    }
}