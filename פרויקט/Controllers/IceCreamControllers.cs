using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using IceCreams.Models;
using IceCreams.Services;
using ServiceIceCream.interfaces;

namespace IceCreams.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IceCreamControllers : ControllerBase
    {
        IIIceCreams service;
        public IceCreamControllers(IIIceCreams ser)
        {
            this.service=ser;
        }

        [HttpGet] // מחזיר את רשימת הגלידות
        public ActionResult<List<IceCream>> GetAll() => service.GetAll();

        [HttpGet("{id}")]
        public ActionResult<IceCream> Get(int id)
        {
            var iceCream = service.Get(id);
            if (iceCream == null)
                return NotFound();

            return iceCream;
        }
//if it dosent work...
        [HttpPost]
        public  IActionResult Create(IceCream iceCream)
        {
            service.Add(iceCream);
            return  CreatedAtAction(nameof(Get), new { id = iceCream.Id }, iceCream);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, IceCream iceCream)
        {
            if (id != iceCream.Id)
                return BadRequest();

            var existingIceCream = service.Get(id);
            if (existingIceCream is null)
                return NotFound();

            service.Update(iceCream);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var iceCream = service.Get(id);
            if (iceCream is null)
                return NotFound();
            service.Delete(id);
            return NoContent();
        }
    }
}