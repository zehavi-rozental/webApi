using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Users.Models;
using Users.Services;
using ServiceUsers.interfaces;

namespace Users.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        public AdminController() { }

        [HttpPost]
        [Route("[action]")]
        public ActionResult<String> Login([FromBody] User User)
        {
            var dt = DateTime.Now;
            //var query = $"select * from users where idnumber = @idnumber";
            if (User.Username != "David"
            || User.Password != $"W{dt.Year}#{dt.Day}!")
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new Claim("username", User.Username),
                new Claim("type", "Admin"),
            };

            var token = FbiTokenService.GetToken(claims);

            return new OkObjectResult(FbiTokenService.WriteToken(token));
        }
        //--------------
         
        IIUsers service;
        public UserController(IIUsers ser)
        {
            this.service=ser;
        }

        [HttpGet] // מחזיר את רשימת המשתמשים
        public ActionResult<List<User>> GetAll() => service.GetAll();

        [HttpGet("{id}")]
        [Route("[action]")]

        public ActionResult<User> Get(int id)
        {
            var user = service.Get(id);
            if (user == null)
                return NotFound();

            return user;
        }
//if it dosent work...
        [HttpPost]
        [Route("[action]")]

        public  IActionResult Create(User user)
        {
            service.Add(user);
            return  CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, User user)
        {
            if (id != user.Id)
                return BadRequest();

            var existingUser = service.Get(id);
            if (existingUser is null)
                return NotFound();

            service.Update(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = service.Get(id);
            if (user is null)
                return NotFound();
            service.Delete(id);
            return NoContent();
        }
    }
}