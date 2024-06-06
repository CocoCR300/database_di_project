using System.Security.Cryptography;
using System.Text;

namespace Restify.API.Util;

public static class DataUtil
{
    public static string GetHash(string data)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashBytes = SHA256.HashData(dataBytes);
        string hashString = Convert.ToBase64String(hashBytes);

        return hashString;
    }

    public static async Task SaveFile(string filePath, IFormFile file)
    {
        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }
    } 
}