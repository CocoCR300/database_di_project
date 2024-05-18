using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;

namespace Restify.API.Controllers
{
    [ApiController]
    [ApiVersion(2)]
    [Route("v{version:apiVersion}/[controller]")]
    public class UserController : Controller
    {
        private readonly RestifyDbContext _context;

        public UserController(RestifyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<User> Get()
        {
            var users = _context.User.Include(u => u.Role);
            return users;
        }

        [HttpGet("{userName}")]
        public User? Get(string userName)
        {
            var user = _context.User.Find(userName);
            _context.Entry(user).Reference(u => u.Role).Load();

            return user;
        }

        [HttpPost]
        public ObjectResult Post(UserRequestData data)
        {
            if (ModelState.IsValid)
            {
                User user = new User
                {
                    Name = data.UserName,
                    Password = DataUtil.GetHash(data.Password),
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

                return Ok("El usuario ha sido creado con éxito");
            }
            
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public ObjectResult Delete(string userName)
        {
            User user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();
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
                _context.Entry(user).Reference(u => u.Person).Load();
                IQueryable<PhoneNumber> phoneNumbersToDelete = _context.PhoneNumber.Where(p => p.PersonId == user.Person.Id && phoneNumbers.Contains(p.Number));
                _context.PhoneNumber.RemoveRange(phoneNumbersToDelete);
                _context.SaveChanges();
                
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
                        return new ObjectResult("La contraseña es incorrecta.")
                        {
                            StatusCode = StatusCodes.Status406NotAcceptable
                        };
                    }
                    else if (data.NewPassword != data.NewPasswordConfirmation)
                    {
                        return new ObjectResult("Las contraseñas no coinciden")
                        {
                            StatusCode = StatusCodes.Status406NotAcceptable
                        };
                    }
                    else if (string.Equals(user.Password, (newPasswordHash = DataUtil.GetHash(data.NewPassword)),
                                 StringComparison.OrdinalIgnoreCase))
                    {
                        return new ObjectResult("La nueva contraseña no puede ser igual a la anterior.")
                        {
                            StatusCode = StatusCodes.Status406NotAcceptable
                        };
                    }
                    else
                    {
                        user.Password = newPasswordHash;
                        _context.SaveChanges();

                        return Ok("El cambio de contraseña ha sido realizado con éxito.");
                    }
                }
                else
                {
                    return NotFound("No existe un usuario con el nombre especificado.");
                }
            }
            else
            {
                return BadRequest("Los datos son inválidos.");
            }
        }

        [HttpPost("{userName}/phone_number")]
        public ObjectResult StorePhoneNumber(string userName, string[] phoneNumbers)
        {
            User user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();
                _context.Entry(user.Person).Collection(p => p.PhoneNumbers).Load();
                foreach (string phoneNumber in phoneNumbers)
                {
                    user.Person.PhoneNumbers.Add(new PhoneNumber
                    {
                        PersonId = user.Person.Id,
                        Number = phoneNumber
                    });
                }
                _context.SaveChanges();
                return Ok("Los números de teléfono han sido agregados con éxito.");
            }
            
            return NotFound("No existe un usuario con el nombre especificado.");
        }

        [HttpPatch("{userName}")]
        public ObjectResult Update(string userName, UserPatchRequestData data)
        {
            User user = _context.Find<User>(userName);

            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    _context.Entry(user).Reference(u => u.Person).Load();
                    
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
            else
            {
                return new ObjectResult("Datos inválidos.")
                {
                    StatusCode = StatusCodes.Status406NotAcceptable
                };
            }
        }
    }


    public class UserRequestData
    {
        [Required]
        [EmailAddress]
        [Unique<User>]
        [MaxLength(200)]
        public string   EmailAddress { get; set; }
        [Required]
        [Unique<User>]
        [MaxLength(50)]
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
        [EmailAddress]
        [Unique<User>]
        [MaxLength(200)]
        public string?   EmailAddress { get; set; }
        [MaxLength(50)]
        public string?   FirstName { get; set; }
        [MaxLength(50)]
        public string?   LastName { get; set; }
        [Exists<UserRole>]
		public uint?     RoleId { get; set; }
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
