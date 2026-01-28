
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Pawlio.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.Identity.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ApiController
    {
        public static Dictionary<int, Basket> userBaskets = new Dictionary<int, Basket>();

        public BasketController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet("{id}")]
        public async Task<ActionResult<Basket>> Get(int id)
        {
            var user = GetUser();
            var basket = await _context.Baskets
                .Include(b => b.Accountings)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(d => d.FirmId == user.FirmId && d.BranchId == user.BranchId && !d.IsDeleted && d.Id == id);

            if (basket == null) return Problem($"{id} nolu sepet bulunamadı!");
            return basket;
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] BasketRequest request)
        {
            var user = GetUser();
            var basket = request.Basket;

            if (request.PosTerminalId != null)
            {
                // 31.08.2024 Şafak
                // Sepetin POS terminaline gönderilmesi isteniyor ise PosTerminalId dolu gelmeli
                //var posTerminal = MainHub.PosTerminals.Values.FirstOrDefault(p => p.Id == request.PosTerminalId);
                //if (posTerminal == null) return Problem($"{request.PosTerminalId} ID li POS terminali bulunamadı!");

                // Sepete rasgele bir değer atıyorum, daha sonra POS termimnalinden ödeme cavabı gelince aynı sepet mi diye kontrol ediyorum
                // basket.Id = new Random().Next(int.MaxValue);
                userBaskets[user.Id] = basket;

                //// İşlemleri geçici olarak başka yere atıyorum
                //var accountings = basket.Accountings;
                //// Sepetteki işlemlerin yerine boş işlemleri ekliyorum
                //basket.Accountings = new List<Accounting>();
                //// İşlemlerin olmadığı sepeti json a çeviriyorum
                //var basketJson = basket.ToJson();
                //// Tekrar işlemleri sepete alıyorum
                //basket.Accountings = accountings;

                var miniBasket = basket.ToJson().FromJson<Basket>();
                foreach (var a in miniBasket.Accountings!)
                {
                    // POS terminaline yüksek boyutlu veri göndermemek için resimleri siliyorum
                    a.Images!.Clear();
                }

                var basketJson = miniBasket.ToJson();

                //await _mainHub.Clients.Client(posTerminal.ConnectionId).SendAsync("payStart", basketJson);
                return Ok();
            }

            if (basket.Accountings == null) return Problem("İşlem listesi alınamadı!");
            if (basket.Payments == null) return Problem("Ödeme listesi alınamadı!");
            //if (basket.Accountings.Count == 0) return Problem("İşlem listesi boş!");

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    List<int> customerIds = new List<int>();
                    List<int> supplierIds = new List<int>();
                    List<int> productIds = new List<int>();

                    var payments = basket.Payments;
                    var accountings = basket.Accountings;

                    // Hesaplamanın doğru olması için kar ve vergiyi sunbucuda tekrar hesaplıyorum
                    //basket.TotalProfit = 0;
                    //basket.TotalTax = 0;
                    //foreach (var a in accountings)
                    //{
                    //    basket.TotalProfit += a.Quantity * a.Profit;
                    //    basket.TotalTax += a.Quantity * a.Tax;
                    //}

                    basket.Payments = null;
                    basket.Accountings = null;
                    //basket.Examinations = null;
                    _context.Baskets.Add(basket);
                    await _context.SaveAsync(this);

                    if (payments != null) // Varsa ödemeler kaydediliyor
                    {
                        foreach (var p in payments)
                        {
                            if (p.CustomerId != null && p.CustomerId.HasValue) customerIds.Add(p.CustomerId.Value);
                            if (p.SupplierId != null && p.SupplierId.HasValue) supplierIds.Add(p.SupplierId.Value);
                            p.BasketId = basket.Id;
                            _context.Payments.Add(p);
                        }

                        await _context.SaveAsync(this);
                    }

                    //if (examinations != null) // Varsa muayene bilgisi kaydediliyor
                    //{
                    //    foreach (var e in examinations)
                    //    {
                    //        if (!customerIds.Contains(e.CustomerId)) customerIds.Add(e.CustomerId);
                    //        e.BasketId = basket.Id;
                    //        _context.Examinations.Add(e);

                    //        if (e.Appointment != null) // randevu kaydediliyor
                    //        {
                    //            var appointment = e.Appointment;
                    //            appointment.BasketId = basket.Id;
                    //            e.Appointment = null;
                    //            _context.Appointments.Add(appointment);
                    //            await _context.SaveChangesAsync();
                    //            e.AppointmentId = appointment.Id;
                    //        }

                    //        await _context.SaveAsync(this);

                    //        if (e.Images != null) // İşleme ait fotoğraflar kaydediliyor
                    //            foreach (var base64image in e.Images)
                    //            {
                    //                var imageData = Convert.FromBase64String(base64image);
                    //                var img = new ImageModel { ImageId = Guid.NewGuid().ToString(), ImageData = imageData };
                    //                (img as IImage).Save(this, _context, user, ImageType.Examination, customerId: e.CustomerId, animalId: e.AnimalId, examinationId: e.Id);
                    //            }
                    //    }
                    //}

                    foreach (var accounting in accountings)
                    {
                        if (accounting.Type == AccountingTypes.AddStock || accounting.Type == AccountingTypes.SaleProduct) productIds.Add(accounting.EventId);
                        if (accounting.CustomerId != null && accounting.CustomerId.HasValue) customerIds.Add(accounting.CustomerId.Value);
                        if (accounting.SupplierId != null && accounting.SupplierId.HasValue) supplierIds.Add(accounting.SupplierId.Value);

                        if (accounting.Appointment != null) // randevu kaydediliyor
                        {
                            var appointment = accounting.Appointment;
                            accounting.Appointment = null;

                            appointment.BasketId = basket.Id;
                            appointment.CreaterId = user.Id;
                            appointment.FirmId = user.FirmId;
                            appointment.BranchId = user.BranchId;
                            _context.Appointments.Add(appointment);
                            await _context.SaveChangesAsync();

                            accounting.AppointmentId = appointment.Id;
                        }

                        accounting.BasketId = basket.Id;
                        accounting.CreaterId = user.Id;
                        accounting.Created = DateTimeOffset.UtcNow;
                        accounting.FirmId = user.FirmId;
                        accounting.BranchId = user.BranchId;
                        _context.Accountings.Add(accounting);
                        await _context.SaveChangesAsync();

                        if (accounting.Images != null) // İşleme ait fotoğraflar kaydediliyor
                            foreach (var base64image in accounting.Images)
                            {
                                // String den byte[] dönüşümü
                                var imageData = Convert.FromBase64String(base64image);

                                // Ürünle alaklı bir işlemse ürün id sini al
                                int? productId = null;
                                if (accounting.Type == AccountingTypes.SaleProduct || accounting.Type == AccountingTypes.AddStock)
                                    productId = accounting.EventId;

                                var img = new ImageModel { ImageId = Guid.NewGuid().ToString(), ImageData = imageData };
                                (img as IImage).Save(this, _context, user, ImageType.Accounting,
                                    customerId: accounting.CustomerId,
                                    animalIds: accounting.Animals?.Select(a => a.AnimalId).ToList(),
                                    supplierId: accounting.SupplierId,
                                    productId: productId,
                                    accountingId: accounting.Id);
                            }
                    }

                    transaction.Commit();

                    //if (examinations != null)
                    //    foreach (var examination in examinations) // Muayene olayları oluşturuluyor
                    //        await this.AddEvent(_context, EventTypes.Examination, new Event
                    //        {
                    //            EventId = examination.Id,
                    //            EventSubId = examination.Id,
                    //            Title = examination.Diagnosis.Left(50) ?? "",
                    //            Detail = examination.Treatment.Left(50) ?? "",
                    //            Icon = "stethoscope",
                    //        });

                    foreach (var accounting in accountings) // Olaylar oluşturuluyor
                        await this.AddEvent(_context, EventTypes.AccountingCreate, new Event
                        {
                            Flavor = user.Flavor,
                            EventId = accounting.Id,
                            EventSubId = accounting.EventId,
                            //Title = accounting.Title,
                            //Detail = accounting.Detail,
                        });

                    if (customerIds.Count > 0) await UpdateCustomers(customerIds);
                    if (supplierIds.Count > 0) await UpdateSuppliers(supplierIds);
                    if (productIds.Count > 0) await UpdateProducts(productIds);
                    if (accountings.Count == 0) return Ok("");
                    return Ok($"{accountings.Count} işlem başarılı oldu!");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Basket>> DeleteBasket(int id)
        {
            var user = GetUser();
            var basket = await _context.Baskets
                .Include(b => b.Accountings)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id && b.FirmId == user.FirmId && b.BranchId == user.BranchId && !b.IsDeleted);

            if (basket == null) throw new Exception($"{id} nolu sepet bulunamadı!");

            _context.Database.BeginTransaction();
            try
            {
                // Muhasebe işlemleri iptal eidliyor
                if (basket.Accountings != null)
                {
                    foreach (var a in basket.Accountings)
                        a.Status = AccountingStatus.Cancel;

                    await _context.SaveAsync(this);
                }

                // Ödeme işlemleri iptal ediliyor
                if (basket.Payments != null)
                {
                    foreach (var p in basket.Payments)
                        p.Status = PaymentStatus.Cancel;

                    await _context.SaveAsync(this);
                }

                // Sepet iptal ediliyor
                // basket.IsDeleted = true;
                // await _context.SaveAsync(this);

                // İşlemler başarılı, transaction tamamlanıyor
                _context.Database.CommitTransaction();
            }
            catch
            {
                // Hata oluştu tüm işlemleri geri al
                _context.Database.RollbackTransaction();
                throw;
            }

            return basket;
        }

        [HttpGet("accounting/{id}/{status}")]
        public async Task<ActionResult<Accounting>> ChangeAccountingStatus(int id, AccountingStatus status)
        {
            var user = GetUser();
            var accounting = await _context.Accountings.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId && d.BranchId == user.BranchId && !d.IsDeleted);
            if (accounting == null) return Problem("Muhasebe işlemi bulunamadı!");
            // if (accounting.Status == AccountingStatus.Cancel) return Problem("İşlem daha önce iptal edilmiş!");
            accounting.Status = status;
            await _context.SaveAsync(this);

            // SignalR ile bilgiler tüm istemcilerde güncelleniyor
            if (accounting.CustomerId != null) await UpdateCustomers(new List<int> { accounting.CustomerId.Value });
            if (accounting.SupplierId != null) await UpdateSuppliers(new List<int> { accounting.SupplierId.Value });
            if (accounting.Type == AccountingTypes.AddStock || accounting.Type == AccountingTypes.SaleProduct)
                await UpdateProducts(new List<int> { accounting.EventId });

            return accounting;
        }

        [HttpGet("payment/{id}/{status}")]
        public async Task<ActionResult<Payment>> ChangePaymentStatus(int id, PaymentStatus status)
        {
            var user = GetUser();
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id && p.FirmId == user.FirmId && p.BranchId == user.BranchId && !p.IsDeleted);
            if (payment == null) return Problem("Ödeme işlemi bulunamadı!");
            // if (payment.Status == PaymentStatus.Cancel) return Problem("İşlem daha önce iptal edilmiş!");
            payment.Status = status; //PaymentStatus.Cancel;
            await _context.SaveAsync(this);

            // SignalR ile bilgiler tüm istemcilerde güncelleniyor
            if (payment.CustomerId != null) await UpdateCustomers(new List<int> { payment.CustomerId.Value });
            if (payment.SupplierId != null) await UpdateSuppliers(new List<int> { payment.SupplierId.Value });

            return payment;
        }

        /// <summary>
        /// POS tan yapılan işlemi iptal etmeden önce Payment'i tekrar almak için
        /// İşlemin durumu vs. değişmiş olabilir
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpGet("payment/{id}")]
        //public async Task<ActionResult<Payment>> GetPayment(int id)
        //{
        //    var user = GetUser();
        //    var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id && p.FirmId == user.FirmId && p.BranchId == user.BranchId && !p.IsDeleted);
        //    if (payment == null) return Problem("Ödeme işlemi bulunamadı!");
        //    return payment;
        //}

        [HttpPost("posResult")]
        public async Task<ActionResult> BasketTerminalResult([FromBody] BasketTerminalRequest request)
        {
            var user = GetUser();
            var newBasket = request.Basket;

            void SendSignal()
            {
                _mainHub.Clients.Groups("user-" + user.Id).SendAsync(
                    request.Cancel ? "cancelComplate" : "payComplate",
                    request.Success.ToString().ToLower(), request.Message, newBasket.Uid);
            }

            if (!userBaskets.TryGetValue(user.Id, out var basket))
                return Problem("POS terminaline gönderilen sepet bulunamadı!");

            if (request.Cancel)
            {
                if (request.Success)
                {
                    await DeleteBasket(basket.Id);
                    SendSignal();
                    return Ok("İşlem iptal edildi.");
                }
                else
                {
                    SendSignal();
                    return Ok("İşlem iptal edilemedi!");
                }
            }

            if (request.Success)
            {
                // Spet aynı sepetse resimlerinde olduğu işlemleri alıyorum
                // Aynı sepet değilse işlemler resimsiz olarak kaydedilecek
                if (basket.Uid == newBasket.Uid)
                {
                    // Resimlerinde olduğu işlemler sepete ekleniyor
                    newBasket.Accountings = basket.Accountings;
                }

                // Ödemeler POS terminalinden geldi, işlemleri kaydedilen sepetten aldım, ödemeyi yapıyorum
                await Add(new BasketRequest { Basket = newBasket });
                SendSignal();
                return Ok("Ödeme başarıyla alındı.");
            }
            else
            {
                SendSignal();
                return Ok("İşlem başarısız!");
            }

        }

        [HttpPost("posCancelStart")]
        public async Task<ActionResult> Cancel([FromBody] BasketRequest request)
        {
            var user = GetUser();
            //var basket = request.Basket;
            //var posTerminal = MainHub.PosTerminals.Values.FirstOrDefault(p => p.Id == request.PosTerminalId);
            //if (posTerminal == null) return Problem($"{request.PosTerminalId} ID li POS terminali bulunamadı!");

            //userBaskets[user.Id] = basket;
            //var basketJson = basket.ToJson();

            //await _mainHub.Clients.Client(posTerminal.ConnectionId).SendAsync("cancelStart", basketJson);
            return Ok();
        }
    }

    public class BasketRequest
    {
        public Basket Basket { get; set; } = null!;

        public int? PosTerminalId { get; set; }
    }

    public class BasketTerminalRequest
    {
        public Basket Basket { get; set; } = null!;

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool Cancel { get; set; }
    }
}
