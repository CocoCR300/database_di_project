using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Asp.Versioning;

namespace Restify.API.Controllers
{
    [ApiController]
    [ApiVersion(2)]
    [Route("v{version:apiVersion}/[controller]")]
    public class UserController : BaseController
    {
        private readonly AuthenticationUtil _authenticationUtil;
        private readonly RestifyDbContext _context;

        public UserController(AuthenticationUtil authenticationUtil, RestifyDbContext context)
        {
            _authenticationUtil = authenticationUtil;
            _context = context;
        }

        [HttpGet("{pageSize}/{page}")]
        public ObjectResult Get(uint? roleId,
            [Range(0, int.MaxValue)] int pageSize = 10,
            [Range(0, int.MaxValue)] int page = 1)
        {
            IQueryable<User> usersQuery = _context.User
                .Include(u => u.Person);

            if (roleId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.RoleId == roleId);
            }

            object[] users = usersQuery
                .AsEnumerable()
                .Select(u => Models.User.MergeForResponse(u, u.Person))
                .ToArray();
            
            return Ok(PaginatedList<object>.Create(users, users.Length, page, pageSize));
        }

        [HttpGet("{userName}")]
        public ObjectResult Get(string userName)
        {
            User? user = _context.User.Find(userName);

            if (user != null)
            {
                return Ok(Models.User.MergeForResponse(user, user.Person));
            }

            return NotFound("No existe un usuario con el nombre especificado.");
        }
        
        [HttpPost("identity")]
        public ObjectResult IdentityFromToken(TokenRequest request)
        {
            ClaimsPrincipal? claimsPrincipal = _authenticationUtil.ValidateToken(request.Token);

            if (claimsPrincipal == null)
            {
                return BadRequest("Token no válido.");
            }

            Dictionary<string, string> claimsByName = new Dictionary<string, string>();

            foreach (Claim claim in claimsPrincipal.Claims)
            {
                claimsByName.Add(claim.Type, claim.Value);
            }

            return Ok(claimsByName);
        }


        [HttpPost("login")]
        public ObjectResult LogIn(LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
    
            User? user = _context.Find<User>(loginRequest.UserName);

            if (user == null)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }

            var result = _authenticationUtil.VerifyPassword(user, loginRequest.Password);

            if (!result)
            {
                return Unauthorized("Nombre de usuario o contraseña incorrectos.");
            }

            _context.Entry(user).Reference(u => u.Role).Load();
            var token = _authenticationUtil.GenerateJwtToken(user);
            return Ok(token);
        }
        
        [HttpPost("signup")]
        public ObjectResult SignUp(UserRequestData data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = new User
            {
                Name = data.UserName,
                Password = _authenticationUtil.HashPassword(data.Password),
                RoleId = data.RoleId,
                Person = new Person
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    EmailAddress = data.EmailAddress
                }
            };
            
            _context.User.Add(user);
            _context.SaveChanges();
            
            var token = _authenticationUtil.GenerateJwtToken(user);
            return Ok(token);
        }

        [HttpDelete("{userName}")]
        public ObjectResult Delete(string userName)
        {
            User? user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user.Person).Collection(p => p.PhoneNumbers).Load();

                _context.Remove(user.Person);
                _context.Remove(user);
                _context.SaveChanges();
                
                return Ok("El usuario ha sido eliminado con éxito.");
            }

            return NotFound("No existe un usuario con el nombre especificado.");
        }

        [HttpDelete("{userName}/phone_number")]
        public ObjectResult DeletePhoneNumbers(string userName, string[] phoneNumbers)
        {
            User? user = _context.Find<User>(userName);

            if (user != null)
            {
                bool noneExists = true;

                for (int i = 0; i < user.Person.PhoneNumbers.Count; ++i)
                {
                    string phoneNumber = user.Person.PhoneNumbers[i].Number;
                    if (phoneNumbers.Contains(phoneNumber))
                    {
                        noneExists = false;
                        user.Person.PhoneNumbers.RemoveAt(i);
                    }
                }
                
                _context.SaveChanges();

                if (noneExists)
                {
                    return NotFound("El usuario no tiene asignado ninguno de los números de teléfono especificados.");
                }
                
                return Ok("Los números de teléfono han sido eliminados con éxito.");
            }
            
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        [HttpPost("{userName}/password")]
        public ObjectResult ChangePassword(string userName, PasswordRequestData data)
        {
            if (ModelState.IsValid)
            {
                User? user = _context.Find<User>(userName);

                if (user != null)
                {
                    string newPasswordHash;
                    if (!string.Equals(user.Password, DataUtil.GetHash(data.CurrentPassword),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return NotAcceptable("La contraseña es incorrecta.");
                    }
                    
                    if (data.NewPassword != data.NewPasswordConfirmation)
                    {
                        return NotAcceptable("Las contraseñas no coinciden");
                    }
                    
                    if (string.Equals(user.Password, (newPasswordHash = DataUtil.GetHash(data.NewPassword)),
                                 StringComparison.OrdinalIgnoreCase))
                    {
                        return NotAcceptable("La nueva contraseña no puede ser igual a la anterior.");
                    }
                    
                    user.Password = newPasswordHash;
                    _context.SaveChanges();

                    return Ok("El cambio de contraseña ha sido realizado con éxito.");
                }
                
                return NotFound("No existe un usuario con el nombre especificado.");
            }
            
            return BadRequest("Los datos son inválidos.");
        }

        [HttpPost("{userName}/phone_number")]
        public ObjectResult StorePhoneNumbers(string userName, string[] phoneNumbers)
        {
            User? user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();
                foreach (string phoneNumber in phoneNumbers)
                {
                    if (user.Person.PhoneNumbers.Any(p =>
                            string.Equals(p.Number, phoneNumber)))
                    {
                        continue;
                    }
                    
                    user.Person.PhoneNumbers.Add(new PersonPhoneNumber
                    {
                        PersonId = user.Person.Id,
                        Number = phoneNumber
                    });
                }
                _context.SaveChanges();
                return Created();
            }
            
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        [HttpPatch("{userName}")]
        public ObjectResult Update(string userName, UserPatchRequestData data)
        {
            if (ModelState.IsValid)
            {
                User? user = _context.Find<User>(userName);
                
                if (user != null)
                {
                    if (data.UserName != null)
                    {
                        user.Name = data.UserName;
                        user.Person.UserName = data.UserName;
                    }
                    
                    if (data.RoleId != null)
                        user.RoleId = data.RoleId.Value;
                    
                    if (data.FirstName != null)
                        user.Person.FirstName = data.FirstName;
                    if (data.LastName != null)
                        user.Person.LastName = data.LastName;
                    if (data.EmailAddress != null)
                        user.Person.EmailAddress = data.EmailAddress;

                    _context.SaveChanges();
                    return Ok("La modificación del usuario ha sido realizada con éxito.");
                }
                
                return NotFound("No existe un usuario con el nombre especificado.");
            }
            
            return BadRequest("Datos inválidos.");
        }
    }

    public record LoginRequest(
        [Required]
        string UserName,
        [Required]
        string Password
    );
    
    public class UserRequestData
    {
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        [Unique<Person>]
        public string   EmailAddress { get; set; }
        [Required]
        [MaxLength(50)]
        [Unique<User>(PropertyName = nameof(Models.User.Name))]
        public string   UserName { get; set; }
        [Required]
        [MaxLength(50)]
        public string   FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string   LastName { get; set; }
        [Required]
        [MaxLength(50)]
        public string   Password { get; set; }
        [Required]
        [Exists<UserRole>]
		public uint     RoleId { get; set; }
	}
    
    public class UserPatchRequestData
    {
        [Unique<Person>]
        [NotEmptyOrWhiteSpace]
        [MaxLength(200)]
        [EmailAddress]
        public string?  EmailAddress { get; set; }
        [NotEmptyOrWhiteSpace]
        [MaxLength(50)]
        public string?  UserName { get; set; }
        [NotEmptyOrWhiteSpace]
        [MaxLength(50)]
        public string?  FirstName { get; set; }
        [NotEmptyOrWhiteSpace]
        [MaxLength(50)]
        public string?  LastName { get; set; }
        [Exists<UserRole>]
		public uint?    RoleId { get; set; }
	}

    public class PasswordRequestData
    {
        [Required]
        [MaxLength(50)]
        public string CurrentPassword { get; set; }
        [Required]
        [MaxLength(50)]
        public string NewPassword { get; set; }
        [Required]
        [MaxLength(50)]
        public string NewPasswordConfirmation { get; set; }
    }
}

public class TokenRequest
{
    public string Token { get; set; }
}
