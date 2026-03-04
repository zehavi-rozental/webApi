using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using IceCreams.Models;
using IceCreams.Services;
using ServiceIceCream.interfaces;
using Shared.Interfaces;

namespace IceCreams.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IceCreamController : ControllerBase
    {
        private readonly IIIceCreams service;
        private readonly IActiveUser activeUser;

        public IceCreamController(IIIceCreams ser, IActiveUser activeUser)
        {
            this.service = ser;
            this.activeUser = activeUser;
        }

        [HttpGet] // Returns ice creams - current user's only, or all if admin
        public ActionResult<List<IceCream>> GetAll()
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();
            
            return Ok(service.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<IceCream> Get(int id)
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();

            var iceCream = service.Get(id);
            if (iceCream == null)
                return NotFound();

            // Double-check authorization: user can only see their own items (unless admin)
            if (user.Role != "Admin" && iceCream.UserId != user.Id)
                return Forbid();

            return Ok(iceCream);
        }

        [HttpPost]
        public IActionResult Create(IceCream iceCream)
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();

            service.Add(iceCream);
            return CreatedAtAction(nameof(Get), new { id = iceCream.Id }, iceCream);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, IceCream iceCream)
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();

            if (id != iceCream.Id)
                return BadRequest();

            var existingIceCream = service.Get(id);
            if (existingIceCream is null)
                return NotFound();

            // Ensure user can only update their own items (unless admin)
            if (user.Role != "Admin" && existingIceCream.UserId != user.Id)
                return Forbid();

            service.Update(iceCream);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();

            var iceCream = service.Get(id);
            if (iceCream is null)
                return NotFound();

            // Ensure user can only delete their own items (unless admin)
            if (user.Role != "Admin" && iceCream.UserId != user.Id)
                return Forbid();

            service.Delete(id);
            return NoContent();
        }
    }
}