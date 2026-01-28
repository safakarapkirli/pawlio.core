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
using Pawlio.IsyerimPos;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DefinitionController : ApiController
    {
        public DefinitionController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        // GET: api/Definitions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Definition>>> GetDefinitions()
        {
            var user = this.GetUser();
            if (_context.Definitions == null) return NotFound();
            var defs = await _context.Definitions.Where(d => d.FirmId == user.FirmId).ToListAsync();
            var json = defs.ToJson();
            return defs;
        }

        // GET: api/Definitions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Definition>> GetDefinition(int id)
        {
            var user = this.GetUser();
            if (_context.Definitions == null) return NotFound();
            var definition = await _context.Definitions.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId);
            if (definition == null) return NotFound();
            return definition;
        }

        [HttpPost]
        public async Task<ActionResult<List<Definition>>> AddDefinition(Definition definition)
        {
            var user = this.GetUser();

            definition.FirmId = user.FirmId;
            definition.CreaterId = user.Id;
            if (string.IsNullOrEmpty(definition.Key)) definition.Key = string.IsNullOrEmpty(definition.NameEn) ? definition.NameTr : definition.NameEn;
            _context.Definitions.Add(definition);
            await _context.SaveAsync(this);

            List<Definition> newDefinitions = new List<Definition>();

            //if (definition.ParentId != null)
            //{
            //    var parent = await _context.Definitions.FirstOrDefaultAsync(p => p.Id == definition.ParentId);
            //    if (parent != null && parent.Key == "animal")
            //    {
            //        newDefinitions.Add(new Definition { Key = "race", Name = "Irk", ParentId = definition.Id, CreaterId = user.Id, FirmId = user.FirmId, AddSubDefinitions = true, Static = true });
            //        newDefinitions.Add(new Definition { Key = "color", Name = "Renk", ParentId = definition.Id, CreaterId = user.Id, FirmId = user.FirmId, AddSubDefinitions = true, Static = true });
            //        newDefinitions.Add(new Definition { Key = "vaccine", Name = "Aşılar", ParentId = definition.Id, CreaterId = user.Id, FirmId = user.FirmId, AddSubDefinitions = true, Static = true, ValueType = DefinitionValueType.Vaccine });
            //        newDefinitions.Add(new Definition { Key = "insemition", Name = "Tohumlama", ParentId = definition.Id, CreaterId = user.Id, FirmId = user.FirmId, AddSubDefinitions = true, Static = true, ValueType = DefinitionValueType.Inseminition });
            //        newDefinitions.Add(new Definition { Key = "operation", Name = "Hizmetler", ParentId = definition.Id, CreaterId = user.Id, FirmId = user.FirmId, AddSubDefinitions = true, Static = true, ValueType = DefinitionValueType.Service });

            //        // Listedeki tanımlar DB ye ekleniyor
            //        _context.Definitions.AddRange(newDefinitions);
            //        // Hasta Türü altındaki işlemler kaydediliyor
            //        await _context.SaveAsync(this);
            //    }

            //    // İlk kaydedilen hasta türü istemciye gönderilecek listenin başına ekleniyor
            //    newDefinitions.Insert(0, definition);
            //}

            UpdateUI("updateDefinition", 0, newDefinitions.ToArray());
            return newDefinitions;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Definition>> UpdateDefinition(int id, [FromBody] Definition definition)
        {
            var user = this.GetUser();
            if (id != definition.Id) return Problem("Hatalı istek!");
            var exist = await _context.Definitions.FirstOrDefaultAsync(d => d.Id == id);
            if (exist == null) return NotFound();

            exist.ParentId = definition.ParentId;
            exist.NameTr = definition.NameTr;
            exist.NameEn = definition.NameEn;
            exist.Icon = definition.Icon;
            exist.Value = definition.Value;
            exist.ValueType = definition.ValueType;
            exist.DetailsTr = definition.DetailsTr;
            exist.DetailsEn = definition.DetailsEn;
            exist.AddSubDefinitions = definition.AddSubDefinitions;
            await _context.SaveAsync(this);

            UpdateUI("updateDefinition", 1, new[] { exist });
            return CreatedAtAction("GetDefinition", new { id = definition.Id }, definition);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDefinition(int id)
        {
            var user = this.GetUser();
            if (_context.Definitions == null) return NotFound();

            var definition = await _context.Definitions.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId);
            if (definition == null) return NotFound();

            definition.UpdaterId = user.Id;
            definition.Updated = DateTimeOffset.UtcNow;
            await _context.SaveAsync(this);

            _context.Definitions.Remove(definition);
            await _context.SaveAsync(this);

            UpdateUI("updateDefinition", 2, new[] { definition });
            return NoContent();
        }
    }
}
