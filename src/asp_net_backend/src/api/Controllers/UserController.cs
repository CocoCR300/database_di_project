using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restify.API.Data;
using Restify.API.Models;
using Restify.API.Util;

using System.ComponentModel.DataAnnotations;

namespace Restify.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            return users.ToArray();
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
				User user = new User();
				user.Name = data.UserName;
				user.Password = data.Password;
				user.RoleId = data.RoleId;

				user.Person = new Person();
				user.Person.FirstName = data.FirstName;
				user.Person.LastName = data.LastName;
				user.Person.EmailAddress = data.EmailAddress;

				_context.User.Add(user);
				_context.SaveChanges();

                return Ok("El usuario ha sido creado con éxito");
			}
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete]
        public ObjectResult Delete(string userName)
        {
            User user = _context.Find<User>(userName);

            if (user != null)
            {
                _context.Entry(user).Reference(u => u.Person).Load();
                //_context.Entry(user.Person).Collection(p => p.PhoneNumbers).Load();

                _context.Remove(user.Person);
                _context.Remove(user);
                _context.SaveChanges();
                return Ok("El usuario ha sido eliminado con éxito.");
            }

            return NotFound("No existe un usuario con el nombre especificado.");
        }

        [HttpPost("{id}/password")]
        public string Post(string id)
        {
            return "Hi!";
        }
    }

    public class UserRequestData
    {
        [Required]
        [EmailAddress]
        [Unique<User>]
        public string   EmailAddress { get; set; }
        [Required]
        [Unique<User>]
        public string   UserName { get; set; }
        [Required]
        public string   FirstName { get; set; }
        [Required]
        public string   LastName { get; set; }
        [Required]
        public string   Password { get; set; }
        [Required]
        [Exists<UserRole>]
		public uint     RoleId { get; set; }
	}
}
