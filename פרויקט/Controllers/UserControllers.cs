using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Users.Models;
using Users.Services;
using ServiceUsers.interfaces;
using Token.Services;

namespace Users.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        IIUsers service;

        public UserController(IIUsers ser)
        {
            this.service = ser;
        }

        [HttpPost]
        [Route("[action]")]
        public ActionResult<string> Login([FromBody] LoginRequest request)
        {
            var dt = DateTime.Now;

             if (request.Username != "lali" || request.Password != "123")
            {
                return Unauthorized();
            }   

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim("type", "Admin"),
            };

            var token = TokenService.GetToken(claims);
            return Ok(TokenService.WriteToken(token));
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

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create(User user)
        {
            service.Add(user);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
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