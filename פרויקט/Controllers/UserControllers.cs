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
        IIUsers service;
        public UserController(IIUsers ser)
        {
            this.service=ser;
        }

        [HttpGet] // מחזיר את רשימת המשתמשים
        public ActionResult<List<User>> GetAll() => service.GetAll();

        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            var user = service.Get(id);
            if (user == null)
                return NotFound();

            return user;
        }
//if it dosent work...
        [HttpPost]
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