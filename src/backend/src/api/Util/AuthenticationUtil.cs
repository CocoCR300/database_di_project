using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Restify.API.Models;

namespace Restify.API.Util;

public class AuthenticationUtil
{
    public const string PersonIdClaimName = "personId";
    public const string RoleClaimName = "role";
    public const string RoleIdClaimName = "roleId";
    public const string UserNameClaimName = "userName";

    private readonly ILogger<AuthenticationUtil> _logger;
    private IConfiguration _configuration;

    public static byte[] IssuerKeyBytes { get; private set; } = Array.Empty<byte>();
    public static TokenValidationParameters ValidationParameters { get; private set; }
        
    public AuthenticationUtil(IConfiguration configuration, ILogger<AuthenticationUtil> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public static void Initialize(IConfiguration configuration)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(configuration["JwtSettings:SigningKey"]);
        byte[] salt = Encoding.UTF8.GetBytes(configuration["JwtSettings:SigningKeySalt"]);
        IssuerKeyBytes = Rfc2898DeriveBytes.Pbkdf2(keyBytes, salt, 600000, HashAlgorithmName.SHA512, 128);

        ValidationParameters = new TokenValidationParameters
        {
            ValidAudience = configuration["JwtSettings:Audience"],
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(IssuerKeyBytes),
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public string HashPassword(string password)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SigningKey"]);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = HMACSHA256.HashData(keyBytes, passwordBytes);
        string hash = Convert.ToBase64String(hashBytes);

        return hash;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        ClaimsPrincipal? claims = null;
        try
        {
            claims = new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters, out var decodedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
        }
        
        return claims;
    }
    
    public string GenerateJwtToken(User user) {
        var claims = new List<Claim> {
            new Claim(UserNameClaimName, user.Name),
            new Claim(RoleIdClaimName, user.RoleId.ToString()),
            new Claim(RoleClaimName, user.Role.Type),
            new Claim(PersonIdClaimName, user.Person.Id.ToString())
        };

        var jwtToken = new JwtSecurityToken(
            audience: _configuration["JwtSettings:Audience"],
            issuer: _configuration["JwtSettings:Issuer"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(IssuerKeyBytes),
                SecurityAlgorithms.HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public bool VerifyPassword(User user, string password)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SigningKey"]);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = HMACSHA256.HashData(keyBytes, passwordBytes);

        return Enumerable.SequenceEqual(Convert.FromBase64String(user.Password), hash);
    }
}