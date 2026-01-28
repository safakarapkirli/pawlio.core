using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawlio.Hubs;
using Pawlio.Models;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FirmController : ApiController
    {
        public FirmController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        public static async Task<User> GetUserFirmsData(PostgreSqlDbContext _context, HttpContext _httpContext, User? user, int userId)
        {
            if (!_httpContext.Request.Headers.TryGetValue("PN", out var packageName)) packageName = "com.hizliis";

            if (user == null)
            {
                user = await _context.Users.FindAsync(userId);
                if (user == null) throw new Exception($"{userId} nolu kullanıcı bulunamadı!");
            }

            user.UserFirms = await _context.UserFirms.AsNoTracking()
                .Include(uf => uf.Branches)!.ThenInclude(b => b.Branch)
                .Include(uf => uf.Firm).ThenInclude(f => f!.Branches)
                .Where(uf => uf.UserId == user.Id && !uf.Firm!.IsDeleted)
                .ToUserFirm(packageName!)
                .ToListAsync();

            var firmsIds = user.UserFirms!.Select(uf => uf.FirmId).ToList();

            var firmsUsers = await _context.UserFirmsBranches.AsNoTracking()
                .Include(ub => ub.UserFirm) //.ThenInclude(uf => uf!.User)
                .Where(ub => firmsIds.Contains(ub.UserFirm!.FirmId)) // && !ub.UserFirm!.User!.IsDeleted)
                .ToListAsync();

            foreach (var uf in user.UserFirms)
            {
                uf.User = null;
                uf.Firm!.UserIds = firmsUsers?
                    .Where(fu => fu.UserFirm!.FirmId == uf.FirmId)
                    .Select(fu => fu.UserFirm!.UserId)!
                    .DistinctBy(u => u)
                    .ToList() ?? [];

                foreach (var b in uf.Firm.Branches!)
                {
                    b.UserIds = firmsUsers?
                        .Where(fu => fu.UserFirm!.FirmId == uf.FirmId && fu.BranchId == b.Id)
                        .Select(fu => fu.UserFirm!.UserId)!
                        .DistinctBy(u => u)
                        .ToList() ?? [];
                }
            }

            var associatedUserIds = firmsUsers!.Select(fu => fu.UserFirm!.UserId).Distinct().ToList();
            user.AssociatedUsers = await _context.Users
                .AsNoTracking()
                .Where(u => associatedUserIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => u.ToMUser()!)
                .ToListAsync();

            var json = user.ToJson();
            return user;
        }

        [HttpGet]
        public async Task<ActionResult<User>> Get()
        {
            var user = GetUser();
            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }

        // ---------------------------- USER FIRM CONNECTION -----------------------------------

        //async Task<List<UserFirm>> UsersFromFirm(int firmId)
        //{
        //    if (!HttpContext.Request.Headers.TryGetValue("PN", out var packageName)) packageName = "com.hizliis";

        //    var user = this.GetUser();
        //    var userFirms = await _context.UserFirms
        //        .Include(uf => uf.Branches)!.ThenInclude(b => b.Branch)
        //        .Include(uf => uf.Firm).ThenInclude(f => f!.Branches)
        //        .Where(uf => uf.FirmId == firmId && !uf.Firm!.IsDeleted)
        //        .ToUserFirm(packageName!)
        //        .ToListAsync();

        //    var uf = userFirms.FirstOrDefault(uf => uf.UserId == user.Id);
        //    if (uf == null) throw new Exception($"{firmId} nolu firma için tanımınız bulunmuyor!");
        //    //if (uf.PositionId > 0) throw new Exception($"{firmId} nolu firma için yönetim yetkiniz bulunmuyor!");
        //    return userFirms;
        //}

        [HttpGet("users/{firmId}")]
        public async Task<ActionResult<List<UserFirm>>> GetFirmUsers(int firmId)
        {
            if (!HttpContext.Request.Headers.TryGetValue("PN", out var packageName)) packageName = "com.hizliis";

            var user = this.GetUser();
            var userFirms = await _context.UserFirms
                .Include(uf => uf.Branches)!.ThenInclude(b => b.Branch)
                .Include(uf => uf.Firm).ThenInclude(f => f!.Branches)
                .Where(uf => uf.FirmId == firmId && !uf.Firm!.IsDeleted)
                .ToUserFirm(packageName!)
                .ToListAsync();

            var uf = userFirms.FirstOrDefault(uf => uf.UserId == user.Id);
            if (uf == null) throw new Exception($"{firmId} nolu firma için tanımınız bulunmuyor!");
            //if (uf.PositionId > 0) throw new Exception($"{firmId} nolu firma için yönetim yetkiniz bulunmuyor!");

            return userFirms;
        }

        [HttpDelete("users/{firmId}/{userId}")]
        public async Task<ActionResult> DeleteFirmUsers(int firmId, int userId)
        {
            var user = GetUser();

            var userFirms = await _context.UserFirms.Where(uf => uf.FirmId == firmId).ToListAsync();
            var deleteUserFirm = userFirms.FirstOrDefault(uf => uf.UserId == userId);
            if (deleteUserFirm == null) return Problem($"{firmId} nolu firma için {userId} nolu kullanıcı tanımı bulunmuyor!");

            _context.UserFirms.Remove(deleteUserFirm);
            await _context.SaveAsync(this);

            var @event = new Event
            {
                Flavor = user.Flavor,
                UserId = user.Id,
                FirmId = firmId,
                EventId = firmId,
                TypeId = (int)EventTypes.FirmUserDelete,
                //Title = deleteUserFirm.User?.Name ?? "<İsimsiz>",
                //Detail = "Kullanıcı firmadan çıkarıldı!",
            };
            _context.Events.Add(@event);
            await _context.SaveAsync(this);

            return Ok();
        }

        [HttpPost("userbranch/{userId}")]
        public async Task<ActionResult<User>> ChangeUserBranches(int userId, [FromBody] List<UserFirmsBranch> req)
        {
            var user = GetUser();

            var userFirms = (await GetFirmUsers(user.FirmId))?.Value;
            if (userFirms == null) return Problem("Firma kullanıcıları alınamadı!");

            var uf = userFirms.FirstOrDefault(x => x.UserId == userId);
            var branches = uf!.Branches!;

            var branchesId = req.Select(x => x.BranchId).ToList();
            // Eski şubelerden iptal olanlar varsa kaldır
            for (var i = branches.Count - 1; i >= 0; i--)
            {
                var b = branches[i];
                if (!branchesId.Contains(b.BranchId))
                    _context.UserFirmsBranches.Remove(b);
                else
                    // Yeni gelen bilgilerdeki positionId set ediliyor
                    b.PositionId = req.First(ufb => ufb.BranchId == b.BranchId).PositionId;
            }

            // Yeni eklenen şubeler
            foreach (var ufbranch in req)
            {
                var b = branches.FirstOrDefault(br => br.BranchId == ufbranch.BranchId);
                if (b != null) continue;
                ufbranch.UserFirmId = uf.Id;
                _context.UserFirmsBranches.Add(ufbranch);
            }

            await _context.SaveAsync(this);
            //return await UsersFromFirm(user.FirmId);

            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }

        //[HttpGet("{firmId}")]
        //public async Task<ActionResult<UserFirm?>> GetUserFirm(int firmId)
        //{
        //    var user = this.GetUser();
        //    var res = await _context.UserFirms
        //        .Include(uf => uf.Firm).ThenInclude(f => f!.Branches) // Firmaya ait tüm şubeler
        //        .Include(uf => uf.Branches) // Kullanıcının yetkili olduğu şubeler
        //        .Where(uf => uf.UserId == user.Id && uf.FirmId == firmId)
        //        .FirstOrDefaultAsync();

        //    if (res == null) return Problem("Kullanıcıya ait firma bulunamadı!");

        //    // Henüz şube yok ise şube işlemlerine gerek yok
        //    if (res.Branches == null || res.Branches.Count == 0) return res;

        //    res.Branches!.Sort((a, b) => a.BranchId.CompareTo(b.BranchId));

        //    //var branchIds = res.Firm?.Branches?.Select(a => a.Id).ToList() ?? new List<int> { };
        //    //if (branchIds.Count > 0) {
        //    //    var branchUsers = await _context.Users.Where(u => branchIds.Contains(u.Id)).Select(u => new { u.Id, user = u.ToMUser()! }).ToListAsync();
        //    //    foreach (var b in res.Firm!.Branches!)
        //    //        b.Users = branchUsers.Where(bu => bu.Id == b.Id).Select(bu => bu.user).ToList();
        //    //}

        //    return res;
        //}

        // Branch update içine alındı
        //[HttpGet("position/{firmId}/{branchId}/{userId}/{positionId}")]
        //public async Task<ActionResult<List<UserFirm>>> ChangeUserPosition(int firmId, int branchId, int userId, int positionId)
        //{
        //    var userFirms = await UsersFromFirm(firmId);
        //    var uf = userFirms.FirstOrDefault(x => x.UserId == userId);

        //    if (uf == null) return Problem($"{firmId} nolu firmada {userId} nolu kullanıcı bulunamadı!");
        //    //var eskiYetki = uf.PositionId;
        //    //uf.PositionId = positionId;

        //    var branch = await _context.UserFirmsBranches.FirstOrDefaultAsync(ufb => ufb.Id == uf.Id);
        //    if (branch == null) return Problem("Kullanıcı şubeye tanımlı değil!");

        //    branch.PositionId = positionId;
        //    await _context.SaveAsync(this);

        //    var @event = await this.AddEvent(_context, EventTypes.Firm, new Event
        //    {
        //        EventId = uf.Id,
        //        Title = uf.User?.Name ?? "<İsimsiz>",
        //        Detail = "Kullanıcı yetkileri güncellendi!",
        //    });

        //    return userFirms;
        //}

        // ---------------------------- FIRMS -----------------------------------

        //public List<Package> GetPackages(Firm firm)
        //{
        //    var packages = Settings.Packages.ToJson().FromJson<List<Package>>();

        //    foreach (var p in packages)
        //    {
        //        p.IsDisabled = p.Id < firm.PackageId;
        //    }

        //    return packages;
        //}

        //[HttpGet]
        //public async Task<ActionResult<List<UserFirm>>> GetFirms()
        //{
        //    if (!HttpContext.Request.Headers.TryGetValue("PN", out var packageName)) packageName = "com.hizliis";

        //    var user = this.GetUser();
        //    if (_context.Firms == null) return NotFound();
        //    var res = await _context.UserFirms
        //        .Include(uf => uf.Branches)!.ThenInclude(b => b.Branch)
        //        .Include(uf => uf.Firm).ThenInclude(f => f!.Branches)
        //        .Where(uf => uf.UserId == user.Id && !uf.Firm!.IsDeleted)
        //        .ToUserFirm(packageName!)
        //        .ToListAsync();

        //    var json = res.ToJson();
        //    return res;
        //}

        [HttpPost("{id}")]
        public async Task<ActionResult<User>> UpdateFirm(int id, [FromBody] Firm data)
        {
            var user = this.GetUser();
            if (id != data.Id) return Problem("Hatalı istek!");

            var firm = await _context.Firms.FirstOrDefaultAsync(f => f.Id == id);
            if (firm == null) return NotFound();

            // Paket tipi ve timeOut süresi değişmesin diye db den eski kaydı alıp güncelliyorum
            if (firm.ImageData != null) firm.ImageId = Guid.NewGuid().ToString();
            firm.Name = data.Name;
            firm.Phone = data.Phone;
            firm.Email = data.Email;
            firm.Address = data.Address;
            //firm.CityId = data.CityId;
            //firm.DistrictId = data.DistrictId;
            firm.Lat = data.Lat;
            firm.Lon = data.Lon;
            firm.UpdaterId = user.Id;
            firm.Updated = DateTimeOffset.Now;
            await _context.SaveAsync(this);

            if (firm.ImageData != null)
                (firm as IImage).Save(this, _context, user, ImageType.Firm);

            // Event olayını burda elle ekliyorum, AddEvent için firmanın seçilmiş olması gerekiyor 
            var @event = new Event
            {
                Flavor = user.Flavor,
                UserId = user.Id,
                FirmId = firm.Id,
                EventId = firm.Id,
                TypeId = (int)EventTypes.FirmUpdate,
                //Title = firm.Name, 
                //Detail = "Firma bilgileri güncellendi!" 
            };
            _context.Events.Add(@event);
            await _context.SaveAsync(this);

            //var res = await GetFirms();
            //return res.Value!;

            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }

        [HttpPost]
        public async Task<ActionResult<User>> AddFirm(Firm firm)
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                var user = GetUser();

                if (firm.ImageData != null) firm.ImageId = Guid.NewGuid().ToString();
                firm.CreaterId = user.Id;
                //firm.PackageId = 1; // Standart paket
                firm.TimeOut = DateTimeOffset.Now.AddMonths(2).Date; // Firmanın ücretsiz kullanım süresi
                _context.Firms.Add(firm);
                await _context.SaveAsync(this);

                // 01.03.2024 Şafak
                // Geçici olarak oluştrululan firmayı user atıyorum, Events te kullanılıyor
                user.FirmId = firm.Id;

                // Varsayılan tanımlar
                var defs = await GetDefaultDefinitions(firm.Id);
                _context.Definitions.AddRange(defs);
                await _context.SaveAsync(this);

                // Firma-User bağlantısı
                var userFirm = new UserFirm { Flavor = firm.Flavor, FirmId = firm.Id, UserId = user.Id, CreaterId = user.Id, IsAdmin = true };
                _context.UserFirms.Add(userFirm);

                // Ana şube
                var branch = new Branch { Flavor = firm.Flavor, FirmId = firm.Id, Name = "Ana Şube", CreaterId = user.Id };
                _context.Branches.Add(branch);

                // Firma-user ve şube kaydediliyor
                await _context.SaveAsync(this);

                // Şube-user bağlantısı
                _context.UserFirmsBranches.Add(new UserFirmsBranch { Flavor = firm.Flavor, UserFirmId = userFirm.Id, BranchId = branch.Id });
                await _context.SaveAsync(this);

                if (firm.ImageData != null) (firm as IImage).Save(this, _context, user, ImageType.Firm);

                // Event olayını burda elle ekliyorum, AddEvent için firmanın seçilmiş olması gerekiyor 
                var @event = new Event { Flavor = firm.Flavor, UserId = user.Id, FirmId = firm.Id, BranchId = branch.Id, EventId = firm.Id, TypeId = (int)EventTypes.FirmCreate };
                _context.Events.Add(@event);
                await _context.SaveAsync(this);

                // Herşey başarılı, kaydet
                await _context.Database.CommitTransactionAsync();

                return await GetUserFirmsData(_context, HttpContext, null, user.Id);
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        public static async Task<List<Definition>> GetDefaultDefinitions(int firmId)
        {
            var path = Path.GetFullPath("AppData");
            string jsonFilePath = path + "/animal-types.json";
            string jsonVaccinesPath = path + "/animal-vaccines.json";
            string jsonServicesPath = path + "/animal-services.json";

            var jsonData = await System.IO.File.ReadAllTextAsync(jsonFilePath);
            var categories = JsonConvert.DeserializeObject<List<AnimalCategoryJson>>(jsonData);

            var jsonVaccinesData = await System.IO.File.ReadAllTextAsync(jsonVaccinesPath);
            var vaccines = JsonConvert.DeserializeObject<List<AnimalVaccineJson>>(jsonVaccinesData);

            var jsonServicesData = await System.IO.File.ReadAllTextAsync(jsonServicesPath);
            var services = JsonConvert.DeserializeObject<List<AnimalServiceJson>>(jsonServicesData);

            var musteriTanımlari = new Definition
            {
                Flavor = Flavor.Pawlio,
                FirmId = firmId,
                Key = "customer",
                NameTr = "Müşteri Tanımları",
                NameEn = "Customer Definitions",
                DetailsTr = "Müşteri işlemlerine ait tanımlar (Gruplar, kaynaklar, ...)",
                DetailsEn = "Definitions for customer operations (Groups, sources, ...)",
                Icon = "user",
                Static = true,
                SubDefinitions = new List<Definition>() {
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "groups",
                        NameTr = "Müşteri Grubu",
                        NameEn = "Customer Group",
                        DetailsTr = "Müşterileri daha kolay yönetmek için gruplara ayırın",
                        DetailsEn = "Categorize customers into groups for easier management",
                        Icon = "users",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>() {
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "personal",
                                NameTr = "Bireysel",
                                NameEn = "Personal",
                                DetailsTr = "Küçük işletme veya hobi olarak hayvan besleyen kişiler",
                                DetailsEn = "Individuals who keep animals for small business or as a hobby",
                                Icon = "users",
                                Static = true,
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "bussiness",
                                NameTr = "Büyük İşletme",
                                NameEn = "Bussiness",
                                DetailsTr = "Çok sayıda hayvana sahip işletmeler",
                                DetailsEn = "Businesses with a large number of animals",
                                Icon = "medicalClinic",
                                Static = true,
                            }
                        }
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "source",
                        NameTr = "Müşteri Kaynağı",
                        NameEn = "Customer Source",
                        DetailsTr = "Müşterinin kazanılma kaynağı",
                        DetailsEn = "Source of customer acquisition",
                        Icon = "source",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>() {
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "referral",
                                NameTr = "Referans",
                                NameEn = "Referral",
                                DetailsTr = "Mevcut müşterilerden gelen yeni müşteriler",
                                DetailsEn = "New customers coming from existing customers",
                                Icon = "userCheck",
                                Static = true,
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "advertisement",
                                NameTr = "Reklam",
                                NameEn = "Advertisement",
                                DetailsTr = "Reklam kampanyaları aracılığıyla kazanılan müşteriler",
                                DetailsEn = "Customers acquired through advertising campaigns",
                                Icon = "megaphone",
                                Static = true,
                            }
                        }
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "status",
                        NameTr = "Görüşme Durumları",
                        NameEn = "Meeting Status",
                        DetailsTr = "Müşteri ile görüşme durumları",
                        DetailsEn = "Meeting status",
                        Icon = "phone",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>() {
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Cevap Bekleniyor",
                                NameEn = "Waiting for Response",
                                Icon = "phone",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Görüşüldü",
                                NameEn = "Meeting",
                                Icon = "phone",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Numara Hatalı",
                                NameEn = "Invalid Number",
                                Icon = "phone",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Telefonu açmadı",
                                NameEn = "Phone Not Answered",
                                Icon = "phone",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Ulaşılamadı",
                                NameEn = "Not Reachable",
                                Icon = "phone",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Yüz Yüze Görüşülecek",
                                NameEn = "Face to Face",
                                Icon = "phone",
                            },
                        }
                    }
                }
            };

            var urunTanimlari = new Definition
            {
                Flavor = Flavor.Pawlio,
                FirmId = firmId,
                Key = "product",
                NameTr = "Ürün Tanımları",
                NameEn = "Products Definitions",
                DetailsTr = "Ürün ve tedarikçiler için tanınlar",
                DetailsEn = "Definitions for products and suppliers",
                Icon = "sourcetree",
                Static = true,
                SubDefinitions = new List<Definition>()
                {
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "unit",
                        NameTr = "Birimler",
                        NameEn = "Units",
                        DetailsTr = "Ürün için kullnılan birim (adet, litre, kg vs.)",
                        DetailsEn = "Units used for products (piece, liter, kg, etc.)",
                        Icon = "sourcetree",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>(){
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Adet",
                                NameEn = "Piece",
                                DetailsTr = "Adet bazlı ürün",
                                DetailsEn = "Product based on piece",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Kutu",
                                NameEn = "Box",
                                DetailsTr = "Kutu bazlı ürün",
                                DetailsEn = "Box based on piece",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Gram (gr)",
                                NameEn = "Gram (gr)",
                                DetailsTr = "Ağırlıkça gram bazlı ürün",
                                DetailsEn = "Product based on gram by weight",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Kilogram (kg)",
                                NameEn = "Kilogram (kg)",
                                DetailsTr = "Ağırlıkça kilogram bazlı ürün",
                                DetailsEn = "Product based on kilogram by weight",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Libre (lbs)",
                                NameEn = "Pound (lbs)",
                                DetailsTr = "Ağırlıkça libre bazlı ürün",
                                DetailsEn = "Product based on pound by weight",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Mililitre (ml)",
                                NameEn = "Milliliter (ml)",
                                DetailsTr = "Hacimce litre bazlı ürün",
                                DetailsEn = "Product based on liter by volume",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Litre (l)",
                                NameEn = "Liter (l)",
                                DetailsTr = "Hacimce litre bazlı ürün",
                                DetailsEn = "Product based on liter by volume",
                            },
                        }
                    },
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "concentration",
                        NameTr = "Konsantrasyon",
                        NameEn = "Concentration",
                        DetailsTr = "Ürünün konsantrasyon birimi",
                        DetailsEn = "Concentration unit of the product",
                        Icon = "sourcetree",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>(){
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Yok",
                                NameEn = "None",
                                DetailsTr = "Konsantrasyon birimi yok",
                                DetailsEn = "No concentration unit",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "mg/ml",
                                NameEn = "mg/ml",
                                DetailsTr = "Mililitre başına miligram",
                                DetailsEn = "Milligrams per milliliter",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "IU/ml",
                                NameEn = "IU/ml",
                                DetailsTr = "Mililitre başına uluslararası birim",
                                DetailsEn = "International units per milliliter",
                            },
                        }
                    },
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "category",
                        NameTr = "Kategoriler",
                        NameEn = "Categories",
                        DetailsTr = "Ürünün kategorileri",
                        DetailsEn = "Category of products",
                        Icon = "sourcetree",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>() {
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Tıbbi Ürünler ve İlaçlar",
                                NameEn = "Medical Products and Pharmaceuticals",
                                DetailsTr = "Reçeteli ilaçlar, antibiyotikler, antiparaziterler ve enjekte edilebilir çözeltiler.",
                                DetailsEn = "Prescription drugs, antibiotics, antiparasitics, and injectable solutions."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Aşılar",
                                NameEn = "Vaccines",
                                DetailsTr = "Kedi, köpek ve egzotik hayvanlar için koruyucu bağışıklık aşıları.",
                                DetailsEn = "Protective immunity vaccines for cats, dogs, and exotic animals."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Beslenme ve Diyet",
                                NameEn = "Nutrition and Diet",
                                DetailsTr = "Reçeteli (klinik) mamalar, yaş mamalar ve günlük tüketim mamaları.",
                                DetailsEn = "Prescription (clinical) diets, wet foods, and daily maintenance foods."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Vitamin ve Takviyeler",
                                NameEn = "Vitamins and Supplements",
                                DetailsTr = "Eklem, tüy ve bağışıklık sistemi destekleyici besin takviyeleri.",
                                DetailsEn = "Nutritional supplements supporting joints, coat, and the immune system."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Dermatolojik Ürünler",
                                NameEn = "Dermatological Products",
                                DetailsTr = "Tıbbi şampuanlar, deri bakım kremleri ve losyonlar.",
                                DetailsEn = "Medicated shampoos, skin care creams, and lotions."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Hijyen ve Bakım",
                                NameEn = "Hygiene and Grooming",
                                DetailsTr = "Kedi kumları, göz/kulak temizleyiciler ve tüy bakım ürünleri.",
                                DetailsEn = "Cat litter, eye/ear cleaners, and grooming supplies."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Aksesuar ve Ekipman",
                                NameEn = "Accessories and Equipment",
                                DetailsTr = "Tasmalar, taşıma çantaları, yataklar ve interaktif oyuncaklar.",
                                DetailsEn = "Collars, carriers, beds, and interactive toys."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Klinik Sarf Malzemeleri",
                                NameEn = "Clinical Consumables",
                                DetailsTr = "Operasyon sırasında kullanılan enjektör, pamuk, sütur gibi malzemeler.",
                                DetailsEn = "Materials used during operations such as syringes, cotton, and sutures."
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                NameTr = "Laboratuvar ve Tanı Kitleri",
                                NameEn = "Laboratory and Diagnostic Kits",
                                DetailsTr = "Hızlı tanı test kitleri ve laboratuvar sarf malzemeleri.",
                                DetailsEn = "Rapid diagnostic test kits and laboratory consumables."
                            }
                        }
                    },
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "mark",
                        NameTr = "Markalar",
                        NameEn = "Marks",
                        DetailsTr = "Ürünün markaları",
                        DetailsEn = "Mark of the product",
                        Icon = "sourcetree",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>(){
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Royal Canin", NameEn = "Royal Canin" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Hill's Pet Nutrition", NameEn = "Hill's Pet Nutrition" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Pro Plan (Purina)", NameEn = "Pro Plan (Purina)" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Zoetis", NameEn = "Zoetis" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "MSD Animal Health", NameEn = "MSD Animal Health" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Boehringer Ingelheim", NameEn = "Boehringer Ingelheim" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Virbac", NameEn = "Virbac" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Ceva Sante Animale", NameEn = "Ceva Sante Animale" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Bayer (Elanco)", NameEn = "Bayer (Elanco)" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "N&D (Farmina)", NameEn = "N&D (Farmina)" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Vetaş", NameEn = "Vetas" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Teknovet", NameEn = "Teknovet" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Bavet", NameEn = "Bavet" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Provet", NameEn = "Provet" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Brit Care", NameEn = "Brit Care" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Advance", NameEn = "Advance" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "IDEXX", NameEn = "IDEXX" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "GimCat / GimDog", NameEn = "GimCat / GimDog" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Beaphar", NameEn = "Beaphar" },
                            new Definition { Flavor = Flavor.Pawlio, FirmId = firmId, NameTr = "Biocan", NameEn = "Biocan" }
                        }
                    },
                },
            };

            var muayeneTanimlari = new Definition
            {
                Flavor = Flavor.Pawlio,
                FirmId = firmId,
                Key = "examination",
                NameTr = "Muayene Tanımları",
                NameEn = "Examination Definitions",
                DetailsTr = "Muayenelere ait tanımlar (Hizmetler, aşılar, ...)",
                DetailsEn = "Definitions for examinations (Services, vaccines, ...)",
                Icon = "stethoscope",
                Static = true,
                AddSubDefinitions = true,
                ValueType = DefinitionValueType.Examination,
                SubDefinitions = new List<Definition>() {
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "general",
                        NameTr = "Genel Muayene",
                        NameEn = "General Examination",
                        Icon = "clipboardCheck",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "emergency",
                        NameTr = "Acil Müdahale / Muayene",
                        NameEn = "Emergency Examination",
                        Icon = "stethoscopes",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "eye_exam",
                        NameTr = "Göz Muayenesi",
                        NameEn = "Ophthalmic Examination",
                        Icon = "eye",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "dental_exam",
                        NameTr = "Diş ve Ağız Muayenesi",
                        NameEn = "Dental Examination",
                        Icon = "tooth",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "orthopedic",
                        NameTr = "Ortopedik Muayene",
                        NameEn = "Orthopedic Examination",
                        Icon = "bone",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "dermatologic",
                        NameTr = "Dermatolojik Muayene",
                        NameEn = "Dermatological Examination",
                        Icon = "microscope",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "cardiology",
                        NameTr = "Kardiyolojik Muayene",
                        NameEn = "Cardiological Examination",
                        Icon = "heartPulse",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "check_up",
                        NameTr = "Check-Up (Kapsamlı Kontrol)",
                        NameEn = "Comprehensive Check-Up",
                        Icon = "activity",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 365, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "post_op",
                        NameTr = "Operasyon Sonrası Kontrol",
                        NameEn = "Post-Operative Control",
                        Icon = "userCheck",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "neurological",
                        NameTr = "Nörolojik Muayene",
                        NameEn = "Neurological Examination",
                        Icon = "brain",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "obstetric",
                        NameTr = "Jinekoloji ve Obstetrik Muayene",
                        NameEn = "Obstetric and Gynecological Exam",
                        Icon = "baby",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "oncology",
                        NameTr = "Onkolojik Muayene",
                        NameEn = "Oncological Examination",
                        Icon = "microscope",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "otoscopic",
                        NameTr = "Kulak (Otoskopik) Muayene",
                        NameEn = "Otoscopic Examination",
                        Icon = "ear",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "geriatric",
                        NameTr = "Geriatrik (Yaşlı Hayvan) Muayenesi",
                        NameEn = "Geriatric Examination",
                        Icon = "timer",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 180, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    },
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "follow_up",
                        NameTr = "Rutin Kontrol / Takip",
                        NameEn = "Follow-up Visit",
                        Icon = "calendarClock",
                        ValueType = DefinitionValueType.Examination,
                        Value = (new { Id = 0, Version = 1, RenewalDay = 0, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0, Diagnosis = "", Treatment = "" } as object).ToJson(),
                    }
                }
            };

            var muhasebeTanimlari = new Definition
            {
                Flavor = Flavor.Pawlio,
                FirmId = firmId,
                Key = "accounting",
                NameTr = "Muhasebe Tanımları",
                NameEn = "Accounting Definitions",
                DetailsTr = "Muhasebe işlemlerine ait tanımlar Vergi oranları, ...)",
                DetailsEn = "Definitions for accounting operations (Tax rates, ...)",
                Icon = "calculator",
                Static = true,
                SubDefinitions = new List<Definition>() {
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "taxrate",
                        NameTr = "Vergi Oranı",
                        NameEn = "Tax Rate",
                        DetailsTr = "Ürün ve hizmetler için vergi oranları",
                        DetailsEn = "Tax rates for products and services",
                        Icon = "percent",
                        Static = true,
                        AddSubDefinitions = true,
                        SubDefinitions = new List<Definition>() {
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "tax0",
                                NameTr = "Vergi %0",
                                NameEn = "Tax 0%",
                                DetailsTr = "Vergi uygulanmayan ürün ve hizmetler",
                                DetailsEn = "Products and services with no tax applied",
                                Icon = "percent",
                                ValueType = DefinitionValueType.Int,
                                Value = "0",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "tax10",
                                NameTr = "Vergi %10",
                                NameEn = "Tax 10%",
                                DetailsTr = "Vergi oranı %10 olan ürün ve hizmetler",
                                DetailsEn = "Products and services with 10% tax rate",
                                Icon = "percent",
                                ValueType = DefinitionValueType.Int,
                                Value = "10",
                            },
                            new Definition {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "tax20",
                                NameTr = "Vergi %20",
                                NameEn = "Tax 20%",
                                DetailsTr = "Vergi oranı %20 olan ürün ve hizmetler",
                                DetailsEn = "Products and services with 20% tax rate",
                                Icon = "percent",
                                ValueType = DefinitionValueType.Int,
                                Value = "20",
                            }
                        }
                    },
                }
            };

            var randevuTanimlari = new Definition
            {
                Flavor = Flavor.Pawlio,
                FirmId = firmId,
                Key = "appointment",
                NameTr = "Randevu İşlemleri",
                NameEn = "Appointment",
                DetailsTr = "Randevu işlemlerine ait tanımlar",
                DetailsEn = "Appointment definitions",
                Icon = "calendar",
                Static = true,
                SubDefinitions = new List<Definition>
                {
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        NameTr = "Muayene Randevusu",
                        NameEn = "Examination Appointment",
                        Icon = "calendar",
                    },
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        NameTr = "Aşılama Randevusu",
                        NameEn = "Vaccination Appointment",
                        DetailsTr = "",
                        DetailsEn = "",
                        Icon = "calendar",
                    },
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        NameTr = "Tohumlama Randevusu",
                        NameEn = "Artificial Insemination Appointment",
                        Icon = "calendar",
                    },
                    new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        NameTr = "Kısırlaştırma Randevusu",
                        NameEn = "Spaying Appointment",
                        Icon = "calendar",
                    }
                }
            };

            var hayvanTanimlari = new Definition
            {
                Flavor = Flavor.Pawlio,
                FirmId = firmId,
                Key = "animal",
                NameTr = "Hasta Tanımları",
                NameEn = "Animal Definitions",
                DetailsTr = "Hastalara ait tanımlar (Türler, ırklar, ...)",
                DetailsEn = "Definitions for animals (Types, breeds, ...)",
                Icon = "dog",
                Static = true,
                SubDefinitions = categories!.Select(c => new Definition
                {
                    Flavor = Flavor.Pawlio,
                    FirmId = firmId,
                    Key = c.En.ToLower().Split(' ')[0],
                    NameTr = c.Tr,
                    NameEn = c.En,
                    Static = true,
                    AddSubDefinitions = false,
                    SubDefinitions = c.Types.Select(t => new Definition
                    {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = t.En.ToLower().Split(' ')[0],
                        NameTr = t.Tr,
                        NameEn = t.En,
                        Static = true,
                        AddSubDefinitions = false,
                        SubDefinitions = new List<Definition>()
                        {
                            new Definition
                            {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "breed",
                                NameTr = "Irklar",
                                NameEn = "Breeds",
                                Static = true,
                                AddSubDefinitions = true,
                                SubDefinitions = t.Breeds!.Select(b => new Definition
                                {
                                    Flavor = Flavor.Pawlio,
                                    FirmId = firmId,
                                    NameTr = b.Tr,
                                    NameEn = b.En,
                                }).ToList(),
                            },
                            new Definition
                            {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "color",
                                NameTr = "Renkler",
                                NameEn = "Colors",
                                Static = true,
                                AddSubDefinitions = true,
                                SubDefinitions = t.Colors!.Select(b => new Definition
                                {
                                    Flavor = Flavor.Pawlio,
                                    FirmId = firmId,
                                    NameTr = b.Tr,
                                    NameEn = b.En,
                                    Value = b.Hex ?? "",
                                    ValueType = DefinitionValueType.String,
                                }).ToList(),
                            },
                            new Definition
                            {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "vaccine",
                                NameTr = "Aşılar",
                                NameEn = "Vaccines",
                                Static = true,
                                AddSubDefinitions = true,
                                ValueType = DefinitionValueType.Vaccine,
                                SubDefinitions = vaccines!.FirstOrDefault(v => v.Id == t.Id)?.Vaccines.Select(v => new Definition
                                {
                                    Flavor = Flavor.Pawlio,
                                    FirmId = firmId,
                                    NameTr = v.Name.Tr,
                                    NameEn = v.Name.En,
                                    DetailsTr = v.Note.Tr,
                                    DetailsEn = v.Note.En,
                                    ValueType = DefinitionValueType.Vaccine,
                                    Value = (new {
                                        Id = 0,
                                        Version = 2,
                                        v.InitialDoses,
                                        v.IntervalDays,
                                        v.RepeatDays,
                                        Buying = 0,
                                        Price = 0,
                                        FixPrice = 0,
                                        TaxRateId = 0,
                                    } as object).ToJson(),
                                }).ToList(),
                            },
                            new Definition
                            {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "service",
                                NameTr = "Hizmetler",
                                NameEn = "Services",
                                Static = true,
                                AddSubDefinitions = true,
                                ValueType = DefinitionValueType.Service,
                                SubDefinitions = services!.FirstOrDefault(v => v.Id == t.Id)?.Services.Select(v => new Definition
                                {
                                    Flavor = Flavor.Pawlio,
                                    FirmId = firmId,
                                    NameTr = v.Name.Tr,
                                    NameEn = v.Name.En,
                                    DetailsTr = v.Note.Tr,
                                    DetailsEn = v.Note.En,
                                    ValueType = DefinitionValueType.Service,
                                    Value = (new {
                                        Id = 0,
                                        Version = 2,
                                        Buying = 0,
                                        Price = 0,
                                        FixPrice = 0,
                                        TaxRateId = 0,
                                    } as object).ToJson(),
                                }).ToList(),
                            },
                            new Definition
                            {
                                Flavor = Flavor.Pawlio,
                                FirmId = firmId,
                                Key = "insemination",
                                NameTr = "Tohumlama",
                                NameEn = "Natural Insemination",
                                Static = true,
                                AddSubDefinitions = true,
                                ValueType = DefinitionValueType.Insemination,
                                SubDefinitions =  new List<Definition> {
                                    new Definition
                                    {
                                        Flavor = Flavor.Pawlio,
                                        FirmId = firmId,
                                        NameTr = "Doğal Tohumlama",
                                        NameEn = "Natural Mating",
                                        ValueType = DefinitionValueType.Insemination,
                                        Value = (new { Id = 0, Version = 2, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0 } as object).ToJson(),
                                    },
                                    new Definition
                                    {
                                        Flavor = Flavor.Pawlio,
                                        FirmId = firmId,
                                        NameTr = "Suni Tohumlama",
                                        NameEn = "Artificial Insemination",
                                        ValueType = DefinitionValueType.Insemination,
                                        Value = (new { Id = 0, Version = 2, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0 } as object).ToJson(),
                                    }
                                }
                            }
                        }
                    }).ToList(),
                }).ToList()
            };

            // Procio için genel hizmet tanımları
            var hizmetTanimlari = new Definition
            {
                Flavor = Flavor.Procio,
                FirmId = firmId,
                Key = "service",
                NameTr = "Hizmet Tanımları",
                NameEn = "Service Definitions",
                DetailsTr = "Hizmet işlemlerine ait tanımlar)",
                DetailsEn = "Definitions for service",
                Static = true,
                ValueType = DefinitionValueType.Service,
                SubDefinitions = new List<Definition>() {
                    new Definition {
                        Flavor = Flavor.Pawlio,
                        FirmId = firmId,
                        Key = "service",
                        NameTr = "Genel Hizmet",
                        NameEn = "General Service",
                        ValueType = DefinitionValueType.Service,
                        Value = (new { Id = 0, Version = 2, Buying = 0, Price = 0, FixPrice = 0, TaxRateId = 0 } as object).ToJson(),
                    },
                }
            };

            return new List<Definition>
            {
                musteriTanımlari,
                urunTanimlari,
                muayeneTanimlari,
                muhasebeTanimlari,
                randevuTanimlari,
                hayvanTanimlari
            };
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFirm(int id)
        {
            var user = this.GetUser();
            if (_context.Firms == null) return NotFound();

            var firm = await _context.Firms.FirstOrDefaultAsync(d => d.Id == id && d.CreaterId == user.Id);
            if (firm == null) return NotFound();

            firm.IsDeleted = true;
            await _context.SaveAsync(this);

            Cancel(firm.Id);

            await this.AddEvent(_context, EventTypes.FirmDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = firm.Id,
                //Title = firm.Name,
                //Detail = "Firma silindi!",
            });

            return NoContent();
        }

        // ---------------------------- BRANCHS -----------------------------------

        //[HttpGet("branch")]
        //public async Task<ActionResult<List<Branch>>> GetBranches()
        //{
        //    var user = this.GetUser();
        //    return await _context.Branches.Where(uf => uf.FirmId == user.FirmId && !uf.IsDeleted).ToListAsync();
        //}

        [HttpPost("branch")]
        public async Task<ActionResult<User>> AddBranch(Branch branch)
        {
            var user = this.GetUser();
            var userFirm = await _context.UserFirms.FirstOrDefaultAsync(uf => uf.UserId == user.Id && uf.FirmId == user.FirmId);
            if (userFirm == null) return Problem($"{user.FirmId} nolu firma üzerinde yetkili değilsiniz!");

            var firm = await _context.Firms.FirstOrDefaultAsync(f => f.Id == user.FirmId && !f.IsDeleted);
            if (firm == null) return Problem("Firma bulunamadı!");
            var branchs = await _context.Branches.Where(b => b.FirmId == user.FirmId).ToListAsync();

            //var package = Settings.getPackage(firm.PackageId);
            //if (branchs.Count >= package.BranchCount) return Problem($"'{package.Name} Paket' {package.BranchCount} şube ile sınırlıdır. Yeni şube eklemek için paketinizi yükseltmelisiniz!");

            branch.CreaterId = user.Id;
            branch.FirmId = user.FirmId;
            _context.Branches.Add(branch);
            await _context.SaveAsync(this);

            // Kaydedilen şube kullanıcıya bağlanıyor
            _context.UserFirmsBranches.Add(new UserFirmsBranch { Flavor = user.Flavor, UserFirmId = userFirm.Id, BranchId = branch.Id });
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.BranchCreate, new Event
            {
                Flavor = user.Flavor,
                EventId = branch.Id,
                //Title = branch.Name,
                //Detail = "Yeni şube eklendi!",
            });

            //return branch;
            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }

        [HttpPost("branch/{id}")]
        public async Task<ActionResult<User>> UpdateBranch(int id, [FromBody] Branch branch)
        {
            var user = this.GetUser();
            if (id != branch.Id) return Problem();
            branch.UpdaterId = user.Id;
            branch.Updated = DateTimeOffset.Now;
            _context.Entry(branch).State = EntityState.Modified;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.BranchUpdate, new Event
            {
                Flavor = user.Flavor,
                EventId = branch.Id,
                //Title = branch.Name,
                //Detail = "Şube bilgileri güncellendi!",
            });

            //return branch;
            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }

        [HttpDelete("branch/{id}")]
        public async Task<ActionResult<User>> DeleteBranch(int id)
        {
            var user = this.GetUser();

            var branches = await _context.Branches.Where(d => d.FirmId == user.FirmId && !d.IsDeleted).ToListAsync();
            var branch = branches.FirstOrDefault(d => d.Id == id);
            if (branch == null) return Problem("Şube bulunamadı!");
            if (branches.Count <= 1) return Problem($"Firmaya ait başka şube olmadığı için '{branch.Name}' silinemiyor!");

            branch.IsDeleted = true;
            await _context.SaveAsync(this);

            await this.AddEvent(_context, EventTypes.BranchDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = branch.Id,
                //Title = branch.Name,
                //Detail = "Şube silindi!",
            });

            //return NoContent();
            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }

        // ---------------------------- FIRM SHARE -----------------------------------

        static Dictionary<string, dynamic> sharedFirms = new Dictionary<string, dynamic>();
        static readonly Timer timer = new Timer(removeShares, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

        static void addShares(string key, dynamic data)
        {
            lock (sharedFirms)
                sharedFirms.Add(key, data);
        }

        static void removeShares(object? state)
        {
            lock (sharedFirms)
            {
                var olds = sharedFirms.Where(s => s.Value.date < DateTimeOffset.Now).ToList();
                if (olds.Count > 0)
                    foreach (var o in olds)
                        sharedFirms.Remove(o.Key);
            }
        }

        [HttpPost("share")]
        public async Task<ActionResult<string>> Share(object _data)
        {
            var data = _data as dynamic;
            int firmId = data.firmId;
            List<int> branches = new List<int>();

            foreach (int b in (JArray)data.branches)
                branches.Add(b);

            if (branches.Count == 0)
                return Problem("Paylaşılacak şube(ler) seçilmemiş!");

            var user = this.GetUser();

            var firm = await _context.UserFirms
                .Include(uf => uf.Firm)
                .FirstOrDefaultAsync(uf => uf.UserId == user.Id && uf.FirmId == firmId);

            if (firm == null) return Problem("Firma için yetki bulunamadı!");
            if (firm.Firm == null) return Problem("Firma bulunamadı!");

            var userFirms = await _context.UserFirms.Include(uf => uf.Firm).Where(uf => uf.FirmId == firmId).ToListAsync();

            //var package = Settings.getPackage(firm.Firm.PackageId);
            //if (userFirms.Count >= package.UserCount)
            //    return Problem($"'{package.Name} Paket' {package.UserCount} kullanıcı ile sınırlıdır. Yeni kullanıcı eklemek için paketinizi yükseltmelisiniz!");

            string key = firm.Firm.Id.Digit(4, '0') + "-" + new Random().Next(1000, 10000);
            addShares(key, new { date = DateTimeOffset.Now.AddMinutes(5), firmId = firm.FirmId, data, branches, used = false });
            return key;
        }

        [HttpDelete("share/{firmId}")]
        public ActionResult Cancel(int firmId)
        {
            lock (sharedFirms)
            {
                var olds = sharedFirms.Where(s => s.Value.firmId == firmId).ToList();
                if (olds.Count > 0)
                    foreach (var o in olds)
                        sharedFirms.Remove(o.Key);
            }

            return Ok();
        }

        [HttpGet("connect/{key}")]
        public async Task<ActionResult<User>> Connect(string key)
        {
            var user = this.GetUser();
            if (!sharedFirms.TryGetValue(key, out var sharedData))
                return Ok("Girilen anahtar ile eşleşen firma bulunamadı!");

            int firmId = sharedData.firmId;
            var userFirms = await _context.UserFirms.Include(uf => uf.Firm).Where(uf => uf.FirmId == firmId).ToListAsync();
            var userFirm = userFirms.FirstOrDefault(uf => uf.UserId == user.Id);

            if (userFirm != null) return Ok("Eklenmek istenilen firma zaten kullanımda!");

            var firm = await _context.Firms.FirstOrDefaultAsync(f => f.Id == firmId);
            if (firm == null) return Ok("Firma bulunamadı!");

            //var package = Settings.getPackage(firm.PackageId);
            //if (userFirms.Count >= package.UserCount)
            //    return Ok($"'{package.Name} Paket' {package.UserCount} kullanıcı ile sınırlıdır. Yeni kullanıcı eklemek için paketinizi yükseltmelisiniz!");

            userFirm = new UserFirm
            {
                Flavor = user.Flavor,
                FirmId = firmId,
                UserId = user.Id
            };

            _context.UserFirms.Add(userFirm);
            await _context.SaveAsync(this);

            foreach (int i in sharedData.branches)
                _context.UserFirmsBranches.Add(new UserFirmsBranch
                {
                    Flavor = user.Flavor,
                    UserFirmId = userFirm.Id,
                    BranchId = i
                });

            await _context.SaveAsync(this);

            foreach (int i in sharedData.branches)
                await this.AddEvent(_context, EventTypes.FirmUserCreate, new Event
                {
                    Flavor = user.Flavor,
                    EventId = firm.Id,
                    BranchId = i,
                    FirmId = firm.Id,
                    //Title = firm.Name,
                    //Detail = "Firmaya ile bağlantı kuruldu!",
                });

            //return Ok("OK");
            return await GetUserFirmsData(_context, HttpContext, null, user.Id);
        }
    }
}
