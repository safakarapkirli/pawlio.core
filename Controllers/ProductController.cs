using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Pawlio.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ApiController
    {
        public ProductController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet("/{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var user = GetUser();
            var product = await _context.Products.Include(p => p.Amounts).FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId && !d.IsDeleted);
            if (product == null) return Problem($"{id} nolu ürün bulunamadı!");
            return Ok(product);
        }

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var user = GetUser();
            var products = await _context.Products.Include(p => p.Amounts).Where(d => d.FirmId == user.FirmId && !d.IsDeleted).ToListAsync();
            return products;
        }

        //[HttpGet("barcode/{barcode}")]
        //public async Task<ActionResult<Product>> BarcodeSearch(string barcode)
        //{
        //    var user = GetUser();
        //    var product = await _context.Products.Include(p => p.Amounts).FirstOrDefaultAsync(d => d.Barcode == barcode && d.FirmId == user.FirmId && !d.IsDeleted);
        //    if (product == null) return Problem($"{barcode} borkodlu ürün bulunamadı!");
        //    return product;
        //}

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            var user = GetUser();

            if (product.ImageData != null)
                product.ImageId = Guid.NewGuid().ToString();

            product.FirmId = user.FirmId;
            _context.Products.Add(product);
            await _context.SaveAsync(this);

            if (product.ImageData != null)
                (product as IImage).Save(this, _context, user, ImageType.Product, productId: product.Id);

            await this.AddEvent(_context, EventTypes.ProductCreate, new Event
            {
                Flavor = user.Flavor,
                EventId = product.Id,
                //Title = product.Name,
                //Detail = "Yeni ürün eklendi",
            });

            UpdateUI("updateProduct", 0, new[] { product });
            return product;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
        {
            var user = GetUser();
            if (id != product.Id) return Problem("Hatalı istek!");

            if (product.ImageData != null)
                product.ImageId = Guid.NewGuid().ToString();

            product.FirmId = user.FirmId;
            //product.Amount = _context.Stocks.Where(s => s.ProductId == id && s.FirmId == user.FirmId).Sum(s => s.Amount);
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveAsync(this);

            if (product.ImageData != null)
                (product as IImage).Save(this, _context, user, ImageType.Product, productId: product.Id);

            await this.AddEvent(_context, EventTypes.ProductUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = product.Id,
                //Title = product.Name,
                //Detail = "Ürün bilgileri güncellendi",
            });

            product = await _context.Products.Include(p => p.Amounts).FirstAsync(d => d.Id == id);
            UpdateUI("updateProduct", 1, new[] { product });
            return product;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var user = GetUser();
            var product = await _context.Products.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId);
            if (product == null) return NotFound();

            product.IsDeleted = true;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.ProductDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = product.Id,
                //Title = $"Ürün Kodu: {product.Id}",
                //Detail = "Ürün silindi",
            });

            UpdateUI("updateProduct", 2, new[] { product });
            return NoContent();
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<dynamic>> Detail(int id)
        {
            var user = GetUser();
            var product = await _context.Products
                .Include(p => p.Amounts)
                .Include(p => p.PriceHistory)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (product == null) return NotFound();

            var accoutings = await _context.Accountings
                .Include(a => a.Animals)!.ThenInclude(an => an.Animal)
                .Include(a => a.ImageModels)
                .Where(a => a.EventId == id && !a.IsDeleted)
                .OrderByDescending(a => a.Id)
                .Take(20)
                .ToAccounting()
                .ToListAsync();

            var totals = await _context.Accountings
                .Where(a =>
                    a.EventId == id &&
                    (a.Type == AccountingTypes.AddStock || a.Type == AccountingTypes.SaleProduct) &&
                    !a.IsDeleted
                 )
                .GroupBy(a => a.Type)
                .Select(g => new { id = g.Key, quantity = g.Sum(a => a.Quantity), amount = g.Sum(a => a.Amount), buying = g.Sum(a => a.Buying), count = g.Count() })
                .ToListAsync();

            return new
            {
                product.PriceHistory,
                totals,
                accoutings,
            };
        }

        [HttpPost("updatePrice/{id}/{price}")]
        public async Task<ActionResult<Product>> updatePrice(int id, decimal price)
        {
            var user = GetUser();

            var product = await _context.Products.FirstOrDefaultAsync(d => d.Id == id);
            if (product == null) return Problem("Ürün bulunamadı!");

            product.Price = price;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.ProductPriceUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = id,
                //Title = product.Name,
                //Detail = "Ürün fiyatı güncellendi",
            });

            UpdateUI("updateProduct", 1, new[] { product });
            return Ok("Ürün fiyatı güncellendi!");
        }

        [HttpPost("updateBuying/{id}/{price}")]
        public async Task<ActionResult<Product>> updateBuyingPrice(int id, decimal price)
        {
            var user = GetUser();

            var product = await _context.Products.FirstOrDefaultAsync(d => d.Id == id);
            if (product == null) return Problem("Ürün bulunamadı!");

            product.Buying = price;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.ProductBuyingPriceUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = id,
                //Title = product.Name,
                //Detail = "Ürün alış fiyatı güncellendi",
            });

            UpdateUI("updateProduct", 1, new[] { product });
            return Ok("Ürün alış fiyatı güncellendi!");
        }

        [HttpPost("updateAmount/{id}/{branchId}/{amount}")]
        public async Task<ActionResult<Product>> UpdateAmount(int id, int branchId, decimal amount)
        {
            var user = GetUser();
            var sqlAmount = amount.ToMoney();

            var sql = @$"
		UPDATE ProductAmounts SET Amount = {sqlAmount} WHERE BranchId = {branchId} AND ProductId = {id};
		IF ROW_COUNT() = 0 THEN
			INSERT INTO ProductAmounts (BranchId, ProductId, Amount) VALUES ({branchId}, {id}, {sqlAmount});
		END IF;";

            await _context.Database.ExecuteSqlRawAsync(sql);

            var product = await _context.Products
                .Include(p => p.Amounts)
                .Include(p => p.PriceHistory)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (product == null) return Problem("Ürün bulunamadı!");

            await this.AddEvent(_context, EventTypes.ProductStockUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = id,
                //Title = product.Name,
                //Detail = "Ürün stok miktarı güncellendi",
            });

            UpdateUI("updateProduct", 1, new[] { product });
            return Ok("Ürün stok miktarı güncellendi!");
        }

        [HttpPost("branchMove")]
        public async Task<ActionResult<Product>> BranchMove([FromBody] dynamic data)
        {
            int productId = data.productId;
            int sorrceBranchId = data.sourceBranchId;
            int targetBranchId = data.targetBranchId;
            decimal amount = data.amount;

            var user = GetUser();
            var sqlAmount = amount.ToMoney();

            var branches = _context.Branches.Where(b => b.Id == sorrceBranchId || b.Id == targetBranchId).ToList();
            var source = branches.FirstOrDefault(b => b.Id == sorrceBranchId);
            if (source == null) return Problem("Kaynak şube bulunamadı!");
            var target = branches.FirstOrDefault(b => b.Id == targetBranchId);
            if (target == null) return Problem("Hedef şube bulunamadı!");

            var sql = @$"
    START TRANSACTION;
		UPDATE ProductAmounts SET Amount = Amount - {sqlAmount} WHERE BranchId = {sorrceBranchId} AND ProductId = {productId};
		IF ROW_COUNT() = 0 THEN
			INSERT INTO ProductAmounts (BranchId, ProductId, Amount) VALUES ({sorrceBranchId}, {productId}, -{sqlAmount});
		END IF;
		UPDATE ProductAmounts SET Amount = Amount + {sqlAmount} WHERE BranchId = {targetBranchId} AND ProductId = {productId};
		IF ROW_COUNT() = 0 THEN
			INSERT INTO ProductAmounts (BranchId, ProductId, Amount) VALUES ({targetBranchId}, {productId}, {sqlAmount});
		END IF;
    COMMIT;";

            await _context.Database.ExecuteSqlRawAsync(sql);

            var product = await _context.Products
                .Include(p => p.Amounts)
                .Include(p => p.PriceHistory)
                .FirstOrDefaultAsync(d => d.Id == productId);

            if (product == null) return Problem("Ürün bulunamadı!");

            await this.AddEvent(_context, EventTypes.ProductBranchMove, new Event
            {
                Flavor = user.Flavor,
                EventId = productId,
                //Title = product.Name,
                //Detail = source.Name + " şubesinden " + target.Name + " şubesine stok gönderildi",
            });

            UpdateUI("updateProduct", 1, new[] { product });
            return Ok("Şubeler arası stok taşıma işlemi yapıldı!");
        }
    }
}
