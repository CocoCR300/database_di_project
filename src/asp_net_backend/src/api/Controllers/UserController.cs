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
    public class UserController : BaseController
    {
        private readonly RestifyDbContext _context;

        public UserController(RestifyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ObjectResult Get()
        {
            var users = _context.User
                .Include(u => u.Person)
                .Select(u => new { u.Name, u.Person, u.RoleId });
            
            return Ok(users);
        }

        [HttpGet("{userName}")]
        public ObjectResult Get(string userName)
        {
            User? user = _context.User.Find(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();

                return Ok(new
                {
                    user.Name,
                    user.RoleId,
                    Person = new {
                        user.Person.Id,
                        user.Person.FirstName,
                        user.Person.LastName,
                        user.Person.EmailAddress,
                        user.Person.PhoneNumbers
                    }
                });
            }

            return NotFound("No existe un usuario con el nombre especificado.");
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
                bool noneExists = true;
                _context.Entry(user).Reference(u => u.Person).Load();
                // TODO: Is this necessary now that PhoneNumbers are owned ??
                _context.Entry(user.Person).Collection(p => p.PhoneNumbers).Load();

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
            User user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();
                _context.Entry(user.Person).Collection(p => p.PhoneNumbers).Load();
                foreach (string phoneNumber in phoneNumbers)
                {
                    user.Person.PhoneNumbers.Add(new PersonPhoneNumber
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
            if (ModelState.IsValid)
            {
                User? user = _context.Find<User>(userName);
                
                if (user != null)
                {
                    _context.Entry(user).Reference(u => u.Person).Load();

                    if (data.UserName != null)
                    {
                        User? existingUser = _context.Find<User>(data.UserName);
                        if (existingUser != null)
                        {
                            return NotAcceptable("Ya existe un usuario con el nuevo nombre especificado.");
                        }
                        
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


    public class UserRequestData
    {
        [Required]
        [MaxLength(200)]
        [EmailAddress]
        [Unique<User>]
        public string   EmailAddress { get; set; }
        [Required]
        [MaxLength(50)]
        [Unique<User>]
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
        [Unique<User>]
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
