using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Controllers;
using Pawlio.Models;
using Pawlio.Hubs;
using Microsoft.AspNetCore.SignalR;
using Azure;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ApiController
    {
        public CustomerController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MCustomer>>> Get()
        {
            var user = GetUser();
            var res = await _context.Customers
                .Include(c => c.Balance)
                .Where(x => x.FirmId == user.FirmId && !x.IsDeleted) // && x.Balance.FirmId == user.FirmId)
                .ToMCustomer()
                .ToListAsync();
            return res!;
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> AddCustomer(Customer customer)
        {
            var user = GetUser();
            if (_context.Customers == null) return Problem("Entity set 'Customers'  is null.");

            if (customer.ImageData != null)
                customer.ImageId = Guid.NewGuid().ToString();

            customer.CreaterId = user.Id;
            customer.FirmId = user.FirmId;
            _context.Customers.Add(customer);
            await _context.SaveAsync(this);

            if (customer.ImageData != null)
                (customer as IImage).Save(this, _context, user, ImageType.Customer, customerId: customer.Id);

            await this.AddEvent(_context, EventTypes.CustomerCreate, new Event
            {
                Flavor = user.Flavor,
                EventId = customer.Id,
                CustomerId = customer.Id,
                //Title = customer.Name,
                //Detail = "Yeni müşteri eklendi!",
            });

            UpdateUI("updateCustomer", 0, new[] { customer.ToMCustomer()! });
            return customer;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Customer>> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            var user = GetUser();
            if (id != customer.Id) return Problem("Hatalı istek!");

            if (customer.ImageData != null)
                customer.ImageId = Guid.NewGuid().ToString();

            customer.UpdaterId = user.Id;
            customer.Updated = DateTimeOffset.Now;
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveAsync(this);

            if (customer.ImageData != null)
                (customer as IImage).Save(this, _context, user, ImageType.Customer, customerId: customer.Id);

            await this.AddEvent(_context, EventTypes.CustomerUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = customer.Id,
                CustomerId = customer.Id,
                //Title = customer.Name,
                //Detail = "Müşteri bilgileri güncellendi!",
            });

            UpdateUI("updateCustomer", 1, new[] { customer.ToMCustomer()! });
            return customer; //CreatedAtAction("GetCustomer", new { id = customer.Id }, customer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var user = GetUser();
            if (_context.Customers == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.CreaterId == user.Id);
            if (customer == null) return NotFound();

            customer.IsDeleted = true;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.CustomerDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = customer.Id,
                CustomerId = customer.Id,
                //Title = customer.Name,
                //Detail = "Müşteri bilgileri silindi!",
            });

            UpdateUI("updateCustomer", 2, new[] { customer.ToMCustomer()! });
            return NoContent();
        }

        [HttpGet("{id}/{includeAnimals}")]
        public async Task<ActionResult<dynamic>> Detail(int id, int includeAnimals)
        {
            var user = GetUser();
            var customer = await _context.Customers.Include(c => c.Balance).FirstOrDefaultAsync(x => x.Id == id && x.FirmId == user.FirmId);
            if (customer == null) return NotFound();

            // Son 10 muhasebe işlemi
            var baskets = await _context.Baskets.AsNoTracking().Where(a => a.CustomerId == id && !a.IsDeleted)
                .Include(b => b.Accountings)!.ThenInclude(a => a.ImageModels)
                .OrderByDescending(a => a.Id)
                .Take(10)
                .ToBasket()
                .ToListAsync();

            // Toplam mushasebe işlemleri
            var totals = await _context.Accountings.AsNoTracking()
                .Where(a => a.CustomerId == id && !a.IsDeleted)
                .GroupBy(a => a.Type)
                .Select(g => new { id = g.Key, amount = g.Sum(a => a.Amount), buying = g.Sum(a => a.Buying), count = g.Count() })
                .ToListAsync();

            // Toplam mushasebe işlemleri
            var mounthlyReport = await _context.Accountings.AsNoTracking()
                .Where(a => a.CustomerId == id && !a.IsDeleted &&
                    new AccountingTypes[] { 
                        AccountingTypes.SaleProduct,
                        AccountingTypes.Service,
                        AccountingTypes.Vaccine,
                        AccountingTypes.Insemination,
                    }.Contains(a.Type))
                .GroupBy(a => new { year = a.Created.Year, mount = a.Created.Month })
                .Select(g => new { id = (g.Key.year * 100) + g.Key.mount, amount = g.Sum(a => a.Amount), buying = g.Sum(a => a.Buying), count = g.Count() })
                .OrderBy(a => a.id)
                .ToListAsync();

            List<MAnimal?>? animals = null;

            // Veteriner firması ise müşteri detayına müşterinin hayvanlarını ekliyorum
            // Her işlemde güncellenmiş olacak
            if (user.isVeterinary && includeAnimals == 1)
            {
                animals = await _context.Animals
                    .Where(x => x.FirmId == user.FirmId && x.OwnerId == id && !x.IsDeleted)
                    .ToMAnimal()
                    .ToListAsync();
            }

            dynamic res = new { customer, baskets, totals, mounthlyReport, animals };

            return res;
        }
    }
}
