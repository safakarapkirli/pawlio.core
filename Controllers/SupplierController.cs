using System;
using System.Collections;
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
    public class SupplierController : ApiController
    {
        public SupplierController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<ActionResult<List<Supplier>>> GetSuppliers()
        {
            var user = GetUser();
            if (_context.Suppliers == null) return NotFound();
            var suppliers = await _context.Suppliers
                .Include(c => c.Balance)
                .Where(x => x.FirmId == user.FirmId && !x.IsDeleted)
                .ToListAsync();
            return suppliers;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var user = GetUser();
            if (_context.Suppliers == null) return NotFound();
            var supplier = await _context.Suppliers
                .Include(s => s.Balance)
                .FirstOrDefaultAsync(s => s.Id == id && s.FirmId == user.FirmId && !s.IsDeleted);
            if (supplier == null) return NotFound();
            return supplier;
        }

        [HttpPost]
        public async Task<ActionResult<Supplier>> AddSupplier(Supplier supplier)
        {
            var user = GetUser();

            //if (supplier.ImageData != null)
            //    supplier.ImageId = Guid.NewGuid().ToString();

            supplier.Balance = null;
            supplier.FirmId = user.FirmId;
            _context.Suppliers.Add(supplier);
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.SupplierCreate, new Event
            {
                Flavor = user.Flavor,
                EventId = supplier.Id,
                //Title = supplier.Name,
                //Detail = "Yeni tedarikçi firma eklendi.",
            });

            //if (supplier.ImageData != null)
            //    (supplier as IImage).SaveImage(this, _context, user, ImageType.Supplier);

            UpdateUI("updateSupplier", 0, new[] { supplier });
            return supplier;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Supplier>> UpdateSupplier(int id, [FromBody] Supplier supplier)
        {
            var user = GetUser();
            if (id != supplier.Id) return Problem("Hatalı istek!");

            //if (supplier.ImageData != null)
            //    supplier.ImageId = Guid.NewGuid().ToString();

            supplier.Balance = null;
            supplier.FirmId = user.FirmId;
            _context.Entry(supplier).State = EntityState.Modified;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.SupplierUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = supplier.Id,
                //Title = supplier.Name,
                //Detail = "Tedarikçi firma bilgileri güncellendi.",
            });

            //if (supplier.ImageData != null)
            //    (supplier as IImage).SaveImage(this, _context, user, ImageType.Supplier);

            // Bakiye ile vs. son hali alınıyor
            var response = await GetSupplier(id);
            UpdateUI("updateSupplier", 1, new[] { response.Value! });
            return response;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var user = GetUser();
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId);
            if (supplier == null) return NotFound();

            supplier.IsDeleted = true;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.SupplierDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = supplier.Id,
                //Title = supplier.Name,
                //Detail = "Tedarikçi firma silindi.",
            });

            UpdateUI("updateSupplier", 2, new[] { supplier });
            return NoContent();
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<dynamic>> Detail(int id)
        {
            var user = GetUser();

            var accoutings = await _context.Accountings.Where(a => a.SupplierId == id && !a.IsDeleted)
                .OrderByDescending(a => a.Id)
                .Take(20)
                .ToListAsync();

            var totals = await _context.Accountings
                .Where(a => a.SupplierId == id && !a.IsDeleted)
                .GroupBy(a => a.Type)
                .Select(g => new { id = g.Key, quantity = g.Sum(a => a.Quantity), amount = g.Sum(a => a.Amount), buying = g.Sum(a => a.Buying), count = g.Count() })
                .ToListAsync();

            return new
            {
                totals,
                accoutings,
            };
        }
    }
}
