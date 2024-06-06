using System.Diagnostics.CodeAnalysis;
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

    public bool TryParseCommaSeparatedList<T>(string parameter, ParseFunction<T> parseFunction, [NotNullWhen(true)] out T[]? values)
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