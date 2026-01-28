using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Security.Claims;
using Pawlio.Models;
using Microsoft.AspNetCore.SignalR;
using Pawlio.Hubs;
using Pawlio.IsyerimPos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using NuGet.Protocol;

namespace Pawlio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ApiController
    {
        public IConfiguration _configuration;

        public LoginController(IConfiguration config, PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub)
        {
            _configuration = config;
        }

        //[HttpPost]
        //public async Task<ActionResult<User>> Post(User _userData)
        //{
        //    if (_userData.Phone == null || _userData.Phone.Length < 10) return Problem("Telefon umarası girilmemiş!");
        //    if (_userData.Password == null) return Problem("Şifre girilmemiş!");
        //    if (_userData.Password == "FORGOT" && _userData.Email.Length < 5) return Problem("E-Posta girilmemiş!");
        //    if (_userData.Device == null) return Problem("Cihaz bilgileri alınamadı!");

        //    var nowpass = DateTime.Now.ToString("ddMMyy") + "@avassist";
        //    var pass = _userData.Password == "FORGOT" && _userData.Email == nowpass;

        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u =>
        //        u.Phone == _userData.Phone &&
        //        (
        //            u.Password == _userData.Password ||
        //            (_userData.Password == "FORGOT" && u.Email == _userData.Email) ||
        //            pass
        //        ));

        //    if (user == null && _userData.Password != "FORGOT") return Problem("Kullanıcı bulunamadı veya şifre hatalı!");
        //    if (user == null && _userData.Password == "FORGOT") return Problem("Kullanıcı bulunamadı veya eposta adresi hatalı!");
        //    if (!user!.IsActive) return Problem("Hesabınız pasif durumdadır, lütfen müşteri hizmetleri ile iletişime geçiniz!");
        //    if (user?.Phone == null) return Problem("Telefon numaranız hatalı, lütfen müşteri hizmetleri ile iletişime geçiniz!");

        //    var noOtpUsers = new List<int> { 3 };
        //    if (IsyerimPosUtils.isTest)
        //    {
        //        noOtpUsers.Add(1);
        //        noOtpUsers.Add(2);
        //    }

        //    var noOTP = pass || noOtpUsers.Contains(user.Id);
        //    user.SmsCode = noOTP ? 00000 : new Random().Next(10000, 100000);
        //    user.SmsEndTime = DateTime.Now.AddMinutes(Settings.SmsExpirationMinute); // 2dk süre
        //    await _context.SaveChangesAsync();
        //    if (!noOTP) SendSMS.SendOTPCode(user.Id, user.Phone, user.SmsCode, _userData.Signature);

        //    user.Device = _userData.Device;

        //    var _device = user.Device!;
        //    var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == _device.Id);

        //    if (device == null)
        //    {
        //        device = _device;
        //        //device.Creater = null;
        //        device.CreaterId = user.Id;
        //        _context.Devices.Add(device);
        //        await _context.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        // Değişen bilgi varsa yenisiyle değiştir
        //        if (_device.AppName != device.AppName) device.AppName = _device.AppName;
        //        if (_device.PackageName != device.PackageName) device.PackageName = _device.PackageName;
        //        if (_device.BuildNumber != device.BuildNumber) device.BuildNumber = _device.BuildNumber;
        //        if (_device.BuildSignature != device.BuildSignature) device.BuildSignature = _device.BuildSignature;
        //        if (_device.Version != device.Version) device.Version = _device.Version;

        //        device.CreaterId = user.Id; // Aynı cihazdan yeni bir kullanıcı girerse son kullanıcıya güncelle
        //        device.UpdaterId = user.Id;
        //        device.Updated = DateTime.Now;
        //        _context.Entry(device).State = EntityState.Modified;
        //        await _context.SaveChangesAsync();
        //    }

        //    var sessionId = Guid.NewGuid().ToString();
        //    var claims = new[] {
        //        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
        //        new Claim(JwtRegisteredClaimNames.Jti, sessionId),
        //        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
        //        new Claim("mail", user.Email ?? ""),
        //        new Claim("id", user.Id.ToString()),
        //    };

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        //    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var token = new JwtSecurityToken(
        //        _configuration["Jwt:Issuer"],
        //        _configuration["Jwt:Audience"],
        //        claims,
        //        expires: DateTime.UtcNow.AddDays(30),
        //        signingCredentials: signIn);

        //    user.Token = new JwtSecurityTokenHandler().WriteToken(token);
        //    user.SessionId = sessionId;
        //    user.LastLogin = DateTime.Now;
        //    _context.Users.Attach(user);
        //    _context.Entry(user).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    // Oturum güncelleniyor
        //    Sessions.SetUser(user);

        //    // Bazı bilgiler silinerek istemciye gönderilecek
        //    var responseUser = user.ToJson().FromJson<User>();
        //    responseUser.Password = null;
        //    responseUser.Token = null;
        //    responseUser.SmsCode = 0;
        //    responseUser.EMailCode = 0;

        //    if (responseUser.UserFirms != null)
        //        foreach (var uf in responseUser.UserFirms)
        //            uf.User = null;

        //    return responseUser;
        //}

        [HttpPost("sms")]
        public async Task<ActionResult<User>> Post(LoginData _userData)
        {
            if (_userData.Phone == null || _userData.Phone.Length < 10) return Problem("Telefon umarası girilmemiş!");
            if (_userData.Password == null) return Problem("Şifre girilmemiş!");
            if (_userData.Password == "FORGOT" && _userData.Email.Length < 5) return Problem("E-Posta girilmemiş!");
            if (_userData.Device == null) return Problem("Cihaz bilgileri alınamadı!");

            var nowpass = DateTimeOffset.Now.ToString("ddMMyy") + "@avassist";
            var pass = _userData.Password == "FORGOT" && _userData.Email == nowpass;

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                u.IsDeleted == false &&
                u.Phone == _userData.Phone &&
                (
                    u.Password == _userData.Password ||
                    (_userData.Password == "FORGOT" && u.Email == _userData.Email) ||
                    pass
                ));

            if (user == null && _userData.Password != "FORGOT") return Problem("Kullanıcı bulunamadı veya şifre hatalı!");
            if (user == null && _userData.Password == "FORGOT") return Problem("Kullanıcı bulunamadı veya eposta adresi hatalı!");
            if (!user!.IsActive) return Problem("Hesabınız pasif durumdadır, lütfen müşteri hizmetleri ile iletişime geçiniz!");
            if (user?.Phone == null) return Problem("Telefon numaranız hatalı, lütfen müşteri hizmetleri ile iletişime geçiniz!");

            // Giriş bilgilerinde SmsCode bilgisi yok ise SmsCode gönder, devam etme
            if (string.IsNullOrEmpty(_userData.SmsCode))
            {
                var noOtpUsers = new List<int> { 3 };
                if (IsyerimPosUtils.isTest)
                {
                    noOtpUsers.Add(1);
                    noOtpUsers.Add(2);
                }

                var noOTP = pass || noOtpUsers.Contains(user.Id);
                user.SmsCode = noOTP ? 00000 : new Random().Next(10000, 100000);
                user.SmsEndTime = DateTimeOffset.Now.AddMinutes(Settings.SmsExpirationMinute); // 2dk süre
                await _context.SaveChangesAsync();
                if (!noOTP) SendSMS.SendOTPCode(user.Id, user.Phone, user.SmsCode, _userData.Signature);

                return new User
                {
                    Flavor = user.Flavor,
                    Id = user.Id,
                    Name = user.Name,
                    Title = user.Title,
                    Phone = user.Phone,
                    LastLogin = user.LastLogin,
                    IsActive = user.IsActive,
                    IsAdmin = user.IsAdmin,
                    ImageId = user.ImageId,
                };
            }

            user.Device = _userData.Device;
            var _device = user.Device!;
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == _device.Id);

            if (device == null)
            {
                device = _device;
                device.CreaterId = user.Id;
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Değişen bilgi varsa yenisiyle değiştir
                if (_device.AppName != device.AppName) device.AppName = _device.AppName;
                if (_device.PackageName != device.PackageName) device.PackageName = _device.PackageName;
                if (_device.BuildNumber != device.BuildNumber) device.BuildNumber = _device.BuildNumber;
                if (_device.BuildSignature != device.BuildSignature) device.BuildSignature = _device.BuildSignature;
                if (_device.Version != device.Version) device.Version = _device.Version;

                device.CreaterId = user.Id; // Aynı cihazdan yeni bir kullanıcı girerse son kullanıcıya güncelle
                device.UpdaterId = user.Id;
                device.Updated = DateTimeOffset.Now;
                _context.Entry(device).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            var sessionId = Guid.NewGuid().ToString();
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                new Claim(JwtRegisteredClaimNames.Jti, sessionId),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToString()),
                new Claim("mail", user.Email ?? ""),
                new Claim("id", user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTimeOffset.UtcNow.AddDays(365).DateTime,
                signingCredentials: signIn);

            user.Token = new JwtSecurityTokenHandler().WriteToken(token);
            user.SessionId = sessionId;
            user.LastLogin = DateTimeOffset.Now;
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Oturum güncelleniyor
            Sessions.SetUser(user);

            // Bazı bilgiler silinerek istemciye gönderilecek
            var responseUser = user.ToJson().FromJson<User>();
            responseUser.Password = null;
            //responseUser.Token = null;

            // Kullanıcının firmaları alınıyor
            var userFirmResponseUser = await FirmController.GetUserFirmsData(_context, HttpContext, responseUser, responseUser.Id);
            return userFirmResponseUser;
        }

        [HttpPost("external")]
        public async Task<ActionResult<User>> Post(ExternalLoginData _userData)
        {
            var _flavor = Request.Headers["Flavor"];
            if (!int.TryParse(_flavor, out int flavor))
                throw new Exception("Flavor header is missing or invalid!");

            User? user = null;

            switch (_userData.LoginType)
            {
                case LoginType.Google:
                    //if (_userData.Email.Length < 5) return Problem(statusCode: 467); // E-Posta girilmemiş!
                    if (string.IsNullOrEmpty(_userData.ExternalUserId)) return Problem(statusCode: 474); // Google kimlik doğrulama bilgileri eksik!

                    user = await _context.Users
                       .Where(u => u.LoginType == LoginType.Google && u.ExternalUserId == _userData.ExternalUserId && !u.IsDeleted)
                       .FirstOrDefaultAsync();
                    break;

                case LoginType.Apple:
                    if (string.IsNullOrEmpty(_userData.ExternalUserId) ||
                        string.IsNullOrEmpty(_userData.AppleAuthCode) ||
                        string.IsNullOrEmpty(_userData.AppleIdToken)) return Problem(statusCode: 475); // Apple kimlik doğrulama bilgileri eksik!

                    user = await _context.Users
                       .Where(u => u.LoginType == LoginType.Apple && u.ExternalUserId == _userData.ExternalUserId && !u.IsDeleted)
                       .FirstOrDefaultAsync();
                    break;
            }

            if (user == null) // Kullanıcı yoksa tekrar oluşturuyorum APple ve Google Sign da ayrıca hesap oluşturma yok
            {
                user = new User
                {
                    Flavor = (Flavor)flavor,
                    Email = _userData.EMail,
                    ExternalUserId = _userData.ExternalUserId,
                    AppleAuthCode = _userData.AppleAuthCode,
                    AppleIdToken = _userData.AppleIdToken,
                    Name = (_userData.FirstName + " " + _userData.LastName).Trim(),
                    LoginType = _userData.LoginType,
                    IsActive = true,
                    IsAdmin = false,
                    IsDeleted = false,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            if (!user!.IsActive) return Problem("Hesabınız pasif durumdadır, lütfen müşteri hizmetleri ile iletişime geçiniz!");
            if (user?.Phone == null) return Problem("Telefon numaranız hatalı, lütfen müşteri hizmetleri ile iletişime geçiniz!");

            user.Device = _userData.Device;
            var _device = user.Device!;
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == _device.Id);

            if (device == null)
            {
                device = _device;
                device.CreaterId = user.Id;
                _context.Devices.Add(device);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Değişen bilgi varsa yenisiyle değiştir
                if (_device.AppName != device.AppName) device.AppName = _device.AppName;
                if (_device.PackageName != device.PackageName) device.PackageName = _device.PackageName;
                if (_device.BuildNumber != device.BuildNumber) device.BuildNumber = _device.BuildNumber;
                if (_device.BuildSignature != device.BuildSignature) device.BuildSignature = _device.BuildSignature;
                if (_device.Version != device.Version) device.Version = _device.Version;

                device.CreaterId = user.Id; // Aynı cihazdan yeni bir kullanıcı girerse son kullanıcıya güncelle
                device.UpdaterId = user.Id;
                device.Updated = DateTimeOffset.Now;
                _context.Entry(device).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            var sessionId = Guid.NewGuid().ToString();
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                new Claim(JwtRegisteredClaimNames.Jti, sessionId),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToString()),
                new Claim("mail", user.Email ?? ""),
                new Claim("id", user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTimeOffset.UtcNow.AddDays(365).DateTime,
                signingCredentials: signIn);

            user.Token = new JwtSecurityTokenHandler().WriteToken(token);
            user.SessionId = sessionId;
            user.LastLogin = DateTimeOffset.Now;
            _context.Users.Attach(user);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Oturum güncelleniyor
            Sessions.SetUser(user);

            // Bazı bilgiler silinerek istemciye gönderilecek
            var responseUser = user.ToJson().FromJson<User>();
            responseUser.Password = null;
            //responseUser.Token = null;

            // Kullanıcının firmaları alınıyor
            var userFirmResponseUser = await FirmController.GetUserFirmsData(_context, HttpContext, responseUser, responseUser.Id);
            return userFirmResponseUser;
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            var user = this.GetUser();
            if (user == null) return Problem("User not found!");

            if (user.IsPos)
            {
                //var posTerminal = await _context.PosTerminals.FirstOrDefaultAsync(p => p.Id == user.PosId);
                //if (posTerminal != null)
                //{
                //    posTerminal.IsDeleted = true;
                //    await _context.SaveAsync(this);
                //    var key = posTerminal.DeviceMark + "/" + posTerminal.SerialNo;
                //    MainHub.PosTerminals.Remove(key);

                //    // POS dan oturum kapatıldı, istemcilerdeki POS listesini güncelle
                //    var posTerminals = await PosController.GetDeviceList(_context, user.FirmId, user.BranchId);
                //    var jsonStr = posTerminals.ToJson(formatting: true);
                //    await _mainHub.Clients.Groups(user.FirmId.ToString()).SendAsync("posListUpdate", jsonStr);
                //}
            }
            else
            {
                await _context.Database.ExecuteSqlRawAsync($"DELETE FROM Devices WHERE Id='{user.DeviceId}'");
            }

            return Ok("User successfuly logout!");
        }
    }

    public class LoginData
    {
        public string Phone { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PNToken { get; set; } = null!;
        public Device Device { get; set; } = null!;
        public string Signature { get; set; } = null!;
        public string? SmsCode { get; set; }
    }

    public enum LoginType : byte { Phone = 0, Email = 1, Google = 2, Apple = 3 }

    public class ExternalLoginData
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EMail { get; set; } = null!;

        public string ExternalUserId { get; set; } = null!;

        public string AppleAuthCode { get; set; } = null!;

        public string AppleIdToken { get; set; } = null!;

        public Device Device = null!;

        public LoginType LoginType;
    }
}
