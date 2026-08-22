using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using MyMiddleware.Models;
using MyMiddleware.Services;
using ServiceIceCream.interfaces;
using Shared.Interfaces;

namespace MyMiddleware.Controllers
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

        [HttpGet("by-user-sql")]
        public ActionResult<List<IceCream>> GetByUserSql()
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();

            // This endpoint demonstrates raw SQL queries using EF Core's FromSqlInterpolated
            var result = service.GetIceCreamsForUserSql(user.Id);
            return Ok(result);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<UserIceCreamStatsDto>> GetStats()
        {
            return Ok(service.GetUserIceCreamStatsSql());
        }

        [HttpGet("ranking")]
        [Authorize(Roles = "Admin")]
        public ActionResult<List<UserIceCreamRankingDto>> GetRanking()
        {
            return Ok(service.GetTopUserByIceCreamCountSql());
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
        public IActionResult Create([FromBody] IceCream iceCreamInput)
        {
            var user = activeUser.ActiveUser;
            if (user == null)
                return Unauthorized();

            // Create new IceCream with UserId set during initialization
            var iceCream = new IceCream
            {
                Name = iceCreamInput.Name,
                Milki = iceCreamInput.Milki,
                UserId = user.Id
            };

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
            {
                Console.WriteLine($"[IceCreamController] Update failed: id mismatch. Route id={id}, Body id={iceCream.Id}");
                return BadRequest();
            }

            var existingIceCream = service.Get(id);
            if (existingIceCream is null)
            {
                Console.WriteLine($"[IceCreamController] Update failed: id {id} not found.");
                return NotFound();
            }

            // Ensure user can only update their own items (unless admin)
            if (user.Role != "Admin" && existingIceCream.UserId != user.Id)
            {
                Console.WriteLine($"[IceCreamController] Update forbidden: user {user.Id} tried to update ice cream {id} owned by {existingIceCream.UserId}");
                return Forbid();
            }

            // Log the update attempt
            Console.WriteLine($"[IceCreamController] Updating ice cream {id}: Name={iceCream.Name}, Milki={iceCream.Milki}");

            // Only update allowed fields
            existingIceCream.Name = iceCream.Name;
            existingIceCream.Milki = iceCream.Milki;
            service.Update(existingIceCream);
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