using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Microsoft.AspNetCore.SignalR;
using Pawlio.Hubs;

namespace Pawlio.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ApiController
    {
        public HomeController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<ActionResult<dynamic>> Get()
        {
            var user = this.GetUser();

            var firm = await _context.Firms.FirstOrDefaultAsync(f => f.Id == user.FirmId);
            if (firm == null) return Problem("Firma bulunamadı!");
            if (firm.TimeOut < DateTimeOffset.Now) return Problem("timeout");

            var query = _context.Baskets.AsQueryable();
            query = query.Include(b => b.Accountings)!.ThenInclude(a => a.ImageModels);

            if (user.isVeterinary)
            {
                query = query.Include(b => b.Accountings)!.ThenInclude(a => a.Symptoms);
                query = query.Include(b => b.Accountings)!.ThenInclude(a => a.Animals)!.ThenInclude(an => an.Animal);
            }

            var finalQuery = query
                .Include(b => b.Payments)
                .OrderByDescending(a => a.Id)
                .Where(a =>
                    a.FirmId == user.FirmId &&
                    a.BranchId == user.BranchId &&
                    a.Created.Date == DateTimeOffset.Now.Date &&
                    !a.IsDeleted)
                .ToBasket();

            // 5. SQL Çıktısını al ve çalıştır
            string sql = finalQuery.ToQueryString();
            var lastBaskets = await finalQuery.ToListAsync();

            //List<int> customers = new List<int>();
            //List<int> animals = new List<int>();

            //foreach(var b in lastBaskets)
            //{
            //    if (!customers.Contains(b.CustomerId)) customers.Add(b.CustomerId);
            //    if (b.Accountings?.Animals != null)
            //        foreach(var a in b.Accountings.Animals)
            //            if (!animals.Contains(a.Id)) 
            //                animals.Add(a.Id);
            //}

            //var startDate = DateTime.Now.Date.AddMonths(-1);
            //var branchTotals = await _context.Accountings
            //    .Where(a => a.FirmId == user.FirmId && a.Date >= startDate && !a.IsDeleted)
            //    .GroupBy(a => new { a.BranchId, a.Date.Date })
            //    .Select(g => new
            //    {
            //        g.Key.BranchId,
            //        g.Key.Date,
            //        count = g.Count(),
            //        total = g.Sum(a => a.Amount * a.Quantity),
            //        profit = g.Sum(a => a.Profit * a.Quantity),
            //    })
            //    .OrderBy(a => a.Date)
            //    .ToListAsync();


            var appointments = await _context.Appointments
                .Include(a => a.Animals)
                .Where(d =>
                    d.CreaterId == user.Id &&
                    d.FirmId == user.FirmId &&
                    d.BranchId == user.BranchId &&
                    !d.IsDeleted)
                .OrderByDescending(a => a.Id)
                .Take(250)
                .ToAppointment()
                .ToListAsync<dynamic>();

            var startDate = DateTimeOffset.Now.AddMonths(-1);
            var monthlyCustomers = await _context.Baskets
                .Where(b => b.FirmId == user.FirmId && b.BranchId == user.BranchId && b.CustomerId != null && b.Created > startDate)
                .GroupBy(b => b.Created.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            var res = new { lastBaskets, appointments, monthlyCustomers };

            return Ok(res);
        }
    }
}