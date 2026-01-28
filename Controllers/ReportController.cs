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
    public class ReportController : ApiController
    {
        public ReportController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpPost]
        [Route("accountings")]
        public async Task<ActionResult<List<dynamic>>> AccountingsReport(dynamic data)
        {
            var user = this.GetUser();

            int? userId = data.userId;
            int? branchId = data.branchId;
            AccountingTypes? accountingType = data.accountingType;
            PaymentTypes? paymentType = data.paymentType;
            DateTime startDate = data.startDate;
            DateTime endDate = data.endDate;
            int count = data.count ?? 10;
            decimal? minAmount = data.minAmount;
            decimal? maxAmount = data.maxAmount;

            List<string> sortParams = new List<string>();
            foreach (string item in data.sortParams) sortParams.Add(item);

            var basketIds = new List<int>();

            if (paymentType != null)
            {
                var paymentBasketIds = await _context.Payments
                    .Where(p =>
                        p.FirmId == user.FirmId &&
                        (userId == null || p.CreaterId == userId) &&
                        (branchId == null || p.BranchId == branchId) &&
                        p.PaymentType == paymentType && p.Created >= startDate && p.Created <= endDate && 
                        !p.IsDeleted
                    )
                    .Select(p => p.BasketId)
                    .ToListAsync();

                foreach(var id in paymentBasketIds)
                    if (!basketIds.Contains(id)) 
                        basketIds.Add(id);
            }

            if (accountingType != null)
            {
                var accountingIds = await _context.Accountings
                    .Where(p =>
                        p.FirmId == user.FirmId &&
                        (userId == null || p.CreaterId == userId) &&
                        (branchId == null || p.BranchId == branchId) &&
                        p.Type == accountingType && p.Created >= startDate && p.Created <= endDate && 
                        !p.IsDeleted
                    )
                    .Select(p => p.BasketId)
                    .ToListAsync();

                foreach (var id in accountingIds)
                    if (!basketIds.Contains(id))
                        basketIds.Add(id);
            }

            var query = _context.Baskets!.AsNoTracking()
                .Include(b => b.Accountings)!.ThenInclude(a => a.Symptoms)
                .Include(b => b.Accountings)!.ThenInclude(a => a.Animals)!.ThenInclude(a => a.Animal)
                .Include(b => b.Accountings)!.ThenInclude(a => a.ImageModels)
                .Include(b => b.Payments)
                .Where(b =>
                    b.FirmId == user.FirmId &&
                    (userId == null || b.CreaterId == userId) &&
                    (branchId == null || b.BranchId == branchId) &&
                    ((accountingType == null && paymentType == null) || basketIds.Contains(b.Id)) &&
                    (b.Created >= startDate && b.Created <= endDate) &&
                    (minAmount == null || b.TotalAmount >= minAmount) &&
                    (maxAmount == null || b.TotalAmount <= maxAmount) &&
                    !b.IsDeleted
                );

            if (sortParams.Contains("date")) query = query.OrderBy(b => b.Created);
            if (sortParams.Contains("date-desc")) query = query.OrderByDescending(b => b.Created);
            if (sortParams.Contains("amount")) query = query.OrderBy(b => b.TotalAmount);
            if (sortParams.Contains("amount-desc")) query = query.OrderByDescending(b => b.TotalAmount);
            if (sortParams.Contains("profit")) query = query.OrderBy(b => b.TotalProfit);
            if (sortParams.Contains("profit-desc")) query = query.OrderByDescending(b => b.TotalProfit);

            var baskets = await query.Take(count).ToBasket().ToListAsync<dynamic>();
            return baskets;
        }

        [HttpPost]
        [Route("case")]
        public async Task<ActionResult<dynamic>> AccountingsCase(dynamic data)
        {
            var user = this.GetUser();
            int? userId = data.userId;
            int? branchId = data.branchId;
            DateTime startDate = data.startDate;
            DateTime endDate = data.endDate;

            var basketIds = new List<int>();

            var payments = await _context.Payments.AsNoTracking()
                .Where(b =>
                    b.FirmId == user.FirmId &&
                    (userId == null || b.CreaterId == userId) &&
                    (branchId == null || b.BranchId == branchId) &&
                    (b.Created >= startDate && b.Created <= endDate) &&
                    !b.IsDeleted)
                .ToListAsync();

            var accountings = await _context.Accountings.AsNoTracking()
                .Where(b =>
                    b.FirmId == user.FirmId &&
                    (userId == null || b.CreaterId == userId) &&
                    (branchId == null || b.BranchId == branchId) &&
                    (b.Created >= startDate && b.Created <= endDate) &&
                    !b.IsDeleted)
                .ToListAsync();

            var paymetTotals = payments.GroupBy(a => a.PaymentType).Select(g => new { Type = g.Key, Count = g.Count(), Amount = g.Sum(a => a.Amount) });
            var accountingTotals = accountings.GroupBy(a => a.Type).Select(g => new { Type = g.Key, Count = g.Count(), Amount = g.Sum(a => a.Amount * a.Quantity) });

            return new { payments = paymetTotals, accountings = accountingTotals };
        }
    }
}
