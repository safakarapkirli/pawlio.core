using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Microsoft.AspNetCore.SignalR;
using Pawlio.Hubs;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ApiController
    {
        public EventController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<ActionResult<List<Event>>> GetEvents()
        {
            var user = this.GetUser();
            if (_context.Events == null) return NotFound();
            var @events = await _context.Events.Where(d => d.FirmId == user.FirmId && d.BranchId == user.BranchId).ToListAsync();
            return @events;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var user = this.GetUser();
            if (_context.Events == null) return NotFound();
            var @event = await _context.Events.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId && d.BranchId == user.BranchId);
            if (@event == null) return NotFound();
            return @event;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var user = this.GetUser();
            var @event = await _context.Events.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId && d.BranchId == user.BranchId);
            if (@event == null) return Problem("Stok haraketi bulunamadı!");
            //@event.Amount = 0; // Delete işleminde trigger çalışmadığı için önce miktarı sıfırlıyorum sonra siliyorum
            await _context.SaveAsync(this);
            _context.Events.Remove(@event);
            await _context.SaveAsync(this);
            return NoContent();
        }
    }
}
