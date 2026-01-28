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
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Pawlio.Controllers
{
    public class ApiController : ControllerBase
    {
        protected readonly PostgreSqlDbContext _context;
        protected readonly IHubContext<MainHub> _mainHub;
        protected readonly IHubContext<PosHub>? _posHub;

        public ApiController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub, IHubContext<PosHub> posHub = null)
        {
            _context = context;
            _mainHub = mainHub;
            _posHub = posHub;
        }

        private TokenUser? _user;

        [NonAction]
        public TokenUser GetUser()
        {
            if (_user != null) return _user;

            var _flavor = Request.Headers["Flavor"];
            int.TryParse(_flavor, out int flavor);

            var _firmId = Request.Headers["FirmId"];
            int.TryParse(_firmId, out int firmId);

            var _branchId = Request.Headers["BranchId"];
            int.TryParse(_branchId, out int branchId);

            var _firmType = Request.Headers["FirmType"];
            int.TryParse(_firmType, out int firmType);

            var _posId = Request.Headers["PosId"];
            int.TryParse(_posId, out int posId);

            string? deviceId = Request.Headers?["DI"];

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = (claimsIdentity?.FindFirst("id")?.Value ?? "0").ToInt();
            var sessionId = claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? "";

            var user = Sessions.GetUser(userId);
            if (user.SessionId != sessionId && posId == 0) throw new Exception("LOGOUT");

            return _user = new TokenUser((Flavor)flavor, userId, sessionId, firmId, branchId, user, posId, deviceId ?? "");
        }

        [NonAction]
        public async Task UpdateCustomers(List<int> customerIds)
        {
            var customers = await _context.Customers.Include(c => c.Balance).Where(c => customerIds.Contains(c.Id)).ToMCustomer().ToListAsync();
            UpdateUI("updateCustomer", 1, customers.ToArray()!);
        }

        [NonAction]
        public async Task UpdateSuppliers(List<int> supplierIds)
        {
            var suppliers = await _context.Suppliers.Include(s => s.Balance).Where(s => supplierIds.Contains(s.Id)).ToListAsync();
            UpdateUI("updateSupplier", 1, suppliers.ToArray());
        }

        [NonAction]
        public async Task UpdateProducts(List<int> productIds)
        {
            var products = await _context.Products.Include(p => p.Amounts).Where((p) => productIds.Contains(p.Id)).ToListAsync();
            UpdateUI("updateProduct", 1, products.ToArray());
        }

        [NonAction]
        public void UpdateUI(string @event, int eventId, Object[] data)
        {
            var user = GetUser();
            object json = new { user = new { userId = user.Id, user.FirmId, user.BranchId }, @event, eventId, data };
            var jsonStr = json.ToJson(formatting: true);
            _mainHub.Clients.Groups(user.FirmId.ToString()).SendAsync("updateUI", jsonStr);
        }
    }
}
