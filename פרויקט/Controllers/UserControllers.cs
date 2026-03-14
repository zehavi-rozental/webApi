using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using MyMiddleware.Models;
using MyMiddleware.Services;
using ServiceUsers.interfaces;
using ServiceIceCream.interfaces;

namespace MyMiddleware.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IIUsers userService;
        private readonly IIIceCreams iceCreamService;

        public UserController(IIUsers userService, IIIceCreams iceCreamService)
        {
            this.userService = userService;
            this.iceCreamService = iceCreamService;
        }

        [HttpPost]
        [Route("/api/login")]
        public ActionResult<string> Login([FromBody] LoginRequest request)
        {
            // אימות משתמש מול ה-database
            var users = userService.GetAll();
            var user = users.FirstOrDefault(u => u.Name == request.Username && u.Password == request.Password);
            
            if (user == null)
            {
                return Unauthorized();
            }

            // יצירת JWT עם claims נכונים
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("username", user.Name),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = TokenService.GetToken(claims);
            return Ok(TokenService.WriteToken(token));
        }

        [HttpGet] // Returns list of all users - Admin only
        [Authorize(Roles = "Admin")]
        public ActionResult<List<User>> GetAll() => Ok(userService.GetAll());

        [HttpGet]
        [Route("me")]
        [Authorize]
        public ActionResult<User> Me()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();
            var users = userService.GetAll();
            var user = users.FirstOrDefault(u => u.Name == username);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<User> Get(int id)
        {
            var user = userService.Get(id);
            if (user == null)
                return NotFound();

            // Users can only view their own profile, admins can view anyone's
            var currentUserId = User.FindFirst("userId")?.Value;
            if (User.IsInRole("Admin") == false && currentUserId != id.ToString())
                return Forbid();

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(User user)
        {
            userService.Add(user);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, User user)
        {
            if (id != user.Id)
                return BadRequest();

            var existingUser = userService.Get(id);
            if (existingUser is null)
                return NotFound();

            // Allow admin or self
            var currentUserId = User.FindFirst("userId")?.Value;
            if (User.IsInRole("Admin") == false && currentUserId != id.ToString())
                return Forbid();

            // Regular users cannot change their role or other users' roles
            if (User.IsInRole("Admin") == false)
            {
                // Prevent regular users from changing role
                user.Role = existingUser.Role;
            }
            // Admins can change everything, including role

            // If password is empty, keep the existing one
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = existingUser.Password;
            }

            userService.Update(user);
            return NoContent();
        }

        [HttpPut("profile")]
        [Authorize]
        public IActionResult UpdateProfile([FromBody] User user)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();
            var users = userService.GetAll();
            var existingUser = users.FirstOrDefault(u => u.Name == username);
            if (existingUser == null)
                return NotFound();

            // Only allow updating name and password
            if (!string.IsNullOrWhiteSpace(user.Name))
                existingUser.Name = user.Name;
            if (!string.IsNullOrWhiteSpace(user.Password))
                existingUser.Password = user.Password;

            userService.Update(existingUser);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = userService.Get(id);
            if (user is null)
                return NotFound();

            // Cascading delete: delete all items belonging to this user
            iceCreamService.DeleteAllByUserId(user.Id.ToString());

            // Then delete the user
            userService.Delete(id);
            return NoContent();
        }
    }
}
