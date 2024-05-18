using System.Security.Cryptography;
using System.Text;

namespace Restify.API.Util;

public class DataUtil
{
    public static string GetHash(string data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashBytes = SHA256.HashData(dataBytes);
        string hashString = Convert.ToBase64String(hashBytes);

        return hashString;
    }
}