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
    public class AnimalController : ApiController
    {
        public AnimalController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<ActionResult<List<MAnimal>>> Get()
        {
            var user = this.GetUser();
            var res = await _context.Animals.Where(x => x.FirmId == user.FirmId && !x.IsDeleted).ToMAnimal().ToListAsync();
            return res!;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Animal>> GetAnimal(int id)
        {
            var user = this.GetUser();
            var animal = await _context.Animals
                .Include(a => a.WeightHistory)
                .FirstOrDefaultAsync(a => a.Id == id && a.FirmId == user.FirmId && !a.IsDeleted);

            if (animal?.WeightHistory != null)
                animal.WeightHistory = animal.WeightHistory.OrderBy(x => x.Date).ToList();

            if (animal == null) return Problem($"{id} nolu hayvan bulunamadı!");
            return animal;
        }

        //[HttpGet("barcode/{barcode}")]
        //public async Task<ActionResult<List<MAnimal>>> BarcodeSearch(string barcode)
        //{
        //    var user = this.GetUser();
        //    var animals = await _context.Animals.Where(a => a.IdNumber == barcode && a.FirmId == user.FirmId && !a.IsDeleted).ToMAnimal().ToListAsync();
        //    if (animals.Count == 0) return Problem($"{barcode} kimlik numaralı hasta bulunamadı!");
        //    return animals!;
        //}

        [HttpGet("list/{customerId}")]
        public async Task<ActionResult<List<MAnimal>>> List(int customerId)
        {
            var user = this.GetUser();
            var res = await _context.Animals.Where(x => x.FirmId == user.FirmId && x.OwnerId == customerId && !x.IsDeleted).ToMAnimal().ToListAsync();
            return res!;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Animal>> UpdateAnimal(int id, [FromBody] Animal animal)
        {
            var user = this.GetUser();
            if (id != animal.Id) return Problem("Hatalı istek!");

            if (animal.ImageData != null)
                animal.ImageId = Guid.NewGuid().ToString();

            animal.UpdaterId = user.Id;
            animal.Updated = DateTimeOffset.UtcNow;
            _context.Entry(animal).State = EntityState.Modified;
            await _context.SaveAsync(this);

            if (animal.ImageData != null)
                (animal as IImage).Save(this, _context, user, ImageType.Animal, customerId: animal.OwnerId, animalIds: new List<int> { animal.Id });

            //if (animal.Weight > 0)
            //{
            //    _context.AnimalWeights.Add(new AnimalWeight { AnimalId = animal.Id, CreaterId = user.Id, Weight = animal.Weight });
            //    await _context.SaveAsync(this);
            //}

            await this.AddEvent(_context, EventTypes.AnimalUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = animal.Id,
                CustomerId = animal.OwnerId,
                //AnimalId = animal.Id,
                //Title = animal.Name ?? animal.IdNumber ?? $"Hasta Kimlik No: {animal.Id}",
                //Detail = "Hasta bilgileri güncellendi!",
            });

            return animal;
        }

        [HttpPost]
        public async Task<ActionResult<Animal>> AddAnimal(Animal animal)
        {
            var user = this.GetUser();
            if (animal.ImageData != null)
                animal.ImageId = Guid.NewGuid().ToString();

            animal.CreaterId = user.Id;
            animal.FirmId = user.FirmId;
            _context.Animals.Add(animal);
            await _context.SaveAsync(this);

            if (animal.ImageData != null)
                (animal as IImage).Save(this, _context, user, ImageType.Animal, customerId: animal.OwnerId /*, animalId: animal.Id*/);

            await this.AddEvent(_context, EventTypes.AnimalCreate, new Event
            {
                Flavor = user.Flavor,
                EventId = animal.Id,
                CustomerId = animal.OwnerId,
                //AnimalId = animal.Id,
                //Title = animal.Name ?? animal.IdNumber ?? $"Hasta Kimlik No: {animal.Id}",
                //Detail = "Yeni hasta eklendi!",
            });

            //return CreatedAtAction("GetAnimal", new { id = animal.Id }, animal);

            animal.ImageData = null;
            return animal;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnimal(int id)
        {
            var user = this.GetUser();
            var animal = await _context.Animals.FirstOrDefaultAsync(d => d.Id == id && d.CreaterId == user.Id);
            if (animal == null) return NotFound();

            animal.IsDeleted = true;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.AnimalDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = animal.Id,
                CustomerId = animal.OwnerId,
                //AnimalId = animal.Id,
                //Title = animal.Name ?? animal.IdNumber ?? $"Hasta Kimlik No: {animal.Id}",
                //Detail = "Hasta silindi!",
            });

            return NoContent();
        }

        [HttpPost("addWeight/{customerId}")]
        public async Task<ActionResult<List<AnimalWeight>>> AddWeight(int customerId, AnimalWeight weight)
        {
            var user = this.GetUser();
            weight.CreaterId = user.Id;
            _context.AnimalWeights.Add(weight);
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.AnimalWeightAdd, new Event
            {
                Flavor = user.Flavor,
                EventId = weight.Id,
                CustomerId = customerId,
                //AnimalId = weight.AnimalId,
                //Title = "Ağırlık verisi girişi yapıldı!",
                //Detail = $"Ağırlık: {weight.Weight.ToMoney()}, Ölçüm Tarihi: {weight.Date.ToString("dd.MM.yyyy")}",
            });

            var res = await _context.AnimalWeights.Where(aw => aw.AnimalId == weight.AnimalId)
                .OrderBy(aw => aw.Date)
                .ToListAsync();

            return res;
        }
    }
}
