using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Restify.API.Models;
using Restify.API.Util;

namespace Restify.API.Controllers;

public class BaseController : Controller
{
    [NonAction]
    public ObjectResult BadRequest(string message)
    {
        return new ObjectResult(new Response(message))
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }
    
    [NonAction]
    public ObjectResult Created(object value)
    {
        return base.Created(string.Empty, value);
    }
    
    [NonAction]
    public ObjectResult Created(string storageSubPath, object value)
    {
        return base.Created(Path.Combine(Values.StoragePath, storageSubPath), value);
    }
    
    [NonAction]
    public ObjectResult NotAcceptable(string message)
    {
        return new ObjectResult(new Response(message))
        {
            StatusCode = StatusCodes.Status406NotAcceptable
        };
    }

    [NonAction]
    public NotFoundObjectResult NotFound(string message)
    {
        return new NotFoundObjectResult(new Response(message));
    }

    protected bool TryParseCommaSeparatedList<T>(string parameter, ParseFunction<T> parseFunction, [NotNullWhen(true)] out T[]? values)
    {
        string[] parameterArray = parameter.Split(',');
        T[] _values = new T[parameterArray.Length];
        for (int i = 0; i < parameterArray.Length; ++i)
        {
            string valueString = parameterArray[i];
            if (parseFunction(valueString, out T value))
            {
                _values[i] = value;
            }
            else
            {
                values = null;
                return false;
            }
        }

        values = _values;
        return true;
    }
}

public delegate bool ParseFunction<TResult>(string input, out TResult output);