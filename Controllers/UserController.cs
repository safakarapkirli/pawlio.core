using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pawlio.Hubs;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ApiController
    {
        public UserController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        //async Task<bool> checkUserFromNvi(long TCNumber, List<string> names, List<string> surnames, DateTime birthDate)
        //{
        //    var name = string.Join(" ", names);
        //    var surname = string.Join(" ", surnames);
        //    var kps = new KPSKimlik.KPSPublicSoapClient(new KPSKimlik.KPSPublicSoapClient.EndpointConfiguration());
        //    var res = await kps.TCKimlikNoDogrulaAsync(TCNumber, name, surname, birthDate.Year);

        //    if (!res.Body.TCKimlikNoDogrulaResult)
        //    {
        //        if (names.Count == 1) return false;
        //        surnames.Insert(0, names[names.Count - 1]);
        //        names.RemoveAt(names.Count - 1);
        //        return await checkUserFromNvi(TCNumber, names, surnames, birthDate);
        //    }

        //    return true;
        //}

        [HttpGet]
        public async Task<ActionResult<User>> GetMyUserInfo()
        {
            var _user = this.GetUser();
            var user = await _context.Users.FirstAsync(u => u.Id == _user.Id);
            user.Password = "";
            user.IdentityNumber = "";
            return user;
        }

        [HttpDelete]
        public async Task<ActionResult<string>> Delete()
        {
            var user = GetUser();
            var dbUser = await _context.Users.FirstOrDefaultAsync(d => d.Id == user.Id && !d.IsDeleted);
            if (dbUser == null) return NotFound();

            dbUser.IsDeleted = true;
            await _context.SaveAsync(this);

            UpdateUI("user", 2, new[] { dbUser.ToMUser()! });
            return NoContent();
        }

        [HttpPost("updatePhoto")]
        public async Task<ActionResult> UpdatePhoto(PhotoRequest photoData)
        {
            var user = this.GetUser();
            photoData.Id = user.Id;
            photoData.ImageId = Guid.NewGuid().ToString();

            var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id);
            existUser.ImageId = photoData.ImageId;
            _context.Entry(existUser).State = EntityState.Modified;
            await _context.SaveAsync(this);

            if (photoData.ImageData != null)
                (photoData as IImage).Save(this, _context, user, ImageType.User);

            return Ok(photoData.ImageId);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            var existUser = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Email.ToLower() == user.Email.ToLower() || u.Phone == user.Phone) && !u.IsDeleted //  || u.IdentityNumber == user.IdentityNumber
            );

            if (existUser != null) return Problem("E-Posta Adresi veya Telefon Numarası kullanılıyor, giriş yapmayı deneyin!");

            var names = user.Name.Trim().ToUpper().Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (names.Count < 2) return Problem("Kullanıcı adı hatalı!");

            //if (user.Nationality == "TR")
            //{
            //    var surnames = new List<string>();
            //    surnames.Insert(0, names[names.Count - 1]);
            //    names.RemoveAt(names.Count - 1);
            //    if (!long.TryParse(user.IdentityNumber, out var TCNo)) return Problem("T.C. Kimlik Numarası hatalı!");
            //    var checkStatus = await checkUserFromNvi(TCNo, names, surnames, user.BirthDate);
            //    if (!checkStatus) return Problem("T.C. Kimlik No, Ad Soyad veya Doğum Tarihi hatalı!");
            //}

            user.IsAdmin = false;
            user.IsActive = true;
            _context.Users.Add(user);
            await _context.SaveAsync(this);

            // Olay oluştur
            // await this.AddEvent(_context, EventTypes.UserCreate, new Event { UserId = user.Id, FirmId = 0,   });
            await _context.SaveChangesAsync();
            return user;
        }

        // [AllowAnonymous]
        // [HttpPost("forgot/{email}")]
        // public async Task<ActionResult> ForgotPassword(string email)
        // {
        //     var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        //     if (user == null) return Problem("User not found!");
        //     //user.ForgotPasswordCheckCode = new Random().Next(10000, 99999).ToString();
        //     //user.ForgotPasswordDateTime = DateTime.Now;
        //     await _context.SaveChangesAsync();

        //     //var mailStatus = Send.SendMail(user.Email, "Forgot Password", "Your code is " + user.ForgotPasswordCheckCode);
        //     //if (!mailStatus.IsDone) return Problem(mailStatus.Message);

        //     //await FirebaseUtils.client.ResetEmailPasswordAsync(email);
        //     return Ok("Verification code sent to your email address!");
        // }

        // [HttpPost("updateAbout")]
        // public async Task<ActionResult> UpdateAbout([FromBody] object data)
        // {
        //     string about = data.ToDynamic()!.about;
        //     if (about.Length > 1000) about = about.Left(1000)!;
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id);
        //     existUser.AboutNew = ""; // Onay istenmasi için burada yeni hakkında yazısı olmalı
        //     existUser.About = about; // Oay istenmesi için burayı kaldır
        //     _context.Entry(existUser).State = EntityState.Modified;
        //     await _context.SaveAsync(this);
        //     return Ok(about);
        // }

        // [HttpGet("changePassword/{lastPassword}/{newPassword}")]
        // public async Task<ActionResult> ChangePassword(string lastPassword, string newPassword)
        // {
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id && !u.IsDeleted);
        //     if (existUser.Password != lastPassword) return Problem("Şifre hatalı! Şifrenizi kontrol edip tekrar deneyin!");
        //     existUser.Password = newPassword;
        //     _context.Entry(existUser).State = EntityState.Modified;
        //     await _context.SaveAsync(this);
        //     return Ok("Şifre güncellendi!");
        // }

        // [HttpGet("changeAcceptCitizenStatus/{status}")]
        // public async Task<ActionResult<bool>> ChangeAcceptCitizenStatus( bool status)
        // {
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id);
        //     existUser.AcceptCitizen = status;
        //     await _context.SaveAsync(this);
        //     return existUser.AcceptCitizen;
        // }

        // [AllowAnonymous]
        // [HttpGet("checkCode/{userId}/{method}/{code}")]
        // public async Task<ActionResult<string>> CheckCode(int userId, int method, int code)
        // {
        //     //var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == userId);

        //     switch (method)
        //     {
        //         case 0:
        //             if (existUser.SmsCode != code) return Problem("Doğrulama kodu hatalı!");
        //             if (existUser.SmsEndTime == null) return Problem("Doğrulama tarihi hatası!");
        //             if (existUser.SmsEndTime < DateTimeOffset.UtcNow) return Problem("Doğrulama süresi geçti, tekrar deneyiniz!");
        //             existUser.SmsCode = 0;
        //             existUser.SmsEndTime = null;
        //             await _context.SaveChangesAsync();
        //             return existUser.Token!;

        //         case 1:
        //             if (existUser.EMailCode != code) return Problem("Doğrulama kodu hatalı!");
        //             if (existUser.EMailEndTime == null) return Problem("Doğrulama tarihi hatası!");
        //             if (existUser.EMailEndTime < DateTimeOffset.UtcNow) return Problem("Doğrulama süresi geçti, tekrar deneyiniz!");
        //             existUser.EMailCode = 0;
        //             existUser.EMailEndTime = null;
        //             await _context.SaveChangesAsync();
        //             return existUser.Token!;
        //     }

        //     return Problem("Kod gönderim tipi hatalı!");
        // }

        // [HttpGet("changePhoneNumber/{password}/{newPhoneNumber}")]
        // public async Task<ActionResult> ChangePhoneNumber(string password, string newPhoneNumber)
        // {
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id && !u.IsDeleted);
        //     if (existUser == null) return Problem("Kullanıcı bulunamadı!");

        //     if (existUser.Password != password)
        //         return Problem("Şifre hatalı! Şifrenizi kontrol edip tekrar deneyin!");

        //     if (existUser.SmsEndTime != null && existUser.SmsEndTime > DateTimeOffset.Now)
        //         return Ok($"Şifre daha önce gönderilmiş! Daha önce girilen şifreyi girin veya {Settings.SmsExpirationMinute}dk bekleyin.");

        //     if (await _context.Users.Where(u => u.Phone == newPhoneNumber).CountAsync() > 0)
        //         return Problem("Telefon numarası zaten kullanılıyor!");

        //     existUser.SmsCode = new Random().Next(10000, 100000);
        //     existUser.SmsEndTime = DateTimeOffset.Now.AddMinutes(Settings.SmsExpirationMinute);
        //     _context.Entry(existUser).State = EntityState.Modified;
        //     await _context.SaveAsync(this);

        //     return Ok("Cep telefonunuza doğrulama kodu gönderildi!");
        // }

        // [HttpGet("changePhoneNumberComplate/{password}/{newPhoneNumber}/{code}")]
        // public async Task<ActionResult> ChangePhoneNumberComplate(string password, string newPhoneNumber, int code)
        // {
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id && !u.IsDeleted);
        //     if (existUser == null) return Problem("Kullanıcı bulunamadı!");

        //     if (existUser.Password != password)
        //         return Problem("Şifre hatalı! Şifrenizi kontrol edip tekrar deneyin!");

        //     if (existUser.SmsCode != code || existUser.SmsEndTime == null || existUser.SmsEndTime.Value < DateTimeOffset.Now)
        //         return Problem("Doğrulama kodu hatalı veya kullanım süresi dolmuş!");

        //     if (await _context.Users.Where(u => u.Phone == newPhoneNumber).CountAsync() > 0)
        //         return Problem("Telefon numarası zaten kullanılıyor!");

        //     existUser.SmsCode = 0;
        //     existUser.SmsEndTime = null;
        //     existUser.Phone = newPhoneNumber;
        //     _context.Entry(existUser).State = EntityState.Modified;
        //     await _context.SaveAsync(this);

        //     return Ok("Telefon numarası güncellendi");
        // }

        // // EPosta güncelleme

        // [HttpGet("changeEMail/{password}/{newEMail}")]
        // public async Task<ActionResult> ChangeEmail(string password, string newEMail)
        // {
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id && !u.IsDeleted);
        //     if (existUser == null) return Problem("Kullanıcı bulunamadı!");

        //     if (existUser.Password != password)
        //         return Problem("Doğrulama kodu hatalı! Kodu kontrol edip tekrar deneyin!");

        //     if (await _context.Users.Where(u => u.Email == newEMail).CountAsync() > 0)
        //         return Problem("E-Posta adresi zaten kullanılıyor!");

        //     existUser.EMailCode = new Random().Next(10000, 100000);
        //     existUser.EMailEndTime = DateTimeOffset.Now.AddMinutes(Settings.EMailExpirationMinute);
        //     _context.Entry(existUser).State = EntityState.Modified;
        //     await _context.SaveAsync(this);

        //     return Ok("Doğrulama kodu E-Posta adresinize gönderildi!");
        // }

        // [HttpGet("changeEMailComplate/{password}/{newEMail}/{code}")]
        // public async Task<ActionResult> ChangeEMailComplate(string password, string newEMail, int code)
        // {
        //     var user = this.GetUser();
        //     var existUser = await _context.Users.FirstAsync(u => u.Id == user.Id && !u.IsDeleted);
        //     if (existUser == null) return Problem("Kullanıcı bulunamadı!");

        //     if (existUser.Password != password)
        //         return Problem("Doğrulama kodu hatalı! Kodu kontrol edip tekrar deneyin!");

        //     if (existUser.EMailCode != code || existUser.EMailEndTime == null || existUser.EMailEndTime.Value < DateTimeOffset.Now)
        //         return Problem("Doğrulama kodu hatalı veya kullanım süresi dolmuş!");

        //     if (await _context.Users.Where(u => u.Email == newEMail).CountAsync() > 0)
        //         return Problem("E-Posta numarası zaten kullanılıyor!");

        //     existUser.EMailCode = 0;
        //     existUser.EMailEndTime = null;
        //     existUser.Email = newEMail;
        //     _context.Entry(existUser).State = EntityState.Modified;
        //     await _context.SaveAsync(this);

        //     return Ok("E-Posta adresi güncellendi");
        // }

        // [AllowAnonymous]
        // [HttpPost("checkUserWithSave")]
        // public async Task<ActionResult> CheckUserWithSave(User user)
        // {
        //     var existUser = await _context.Users.FirstOrDefaultAsync(u => (u.Email.ToLower() == user.Email.ToLower() || u.Phone == user.Phone) && !u.IsDeleted);
        //     if (existUser != null) return Problem("E-Posta Adresi veya Telefon Numarası kullanılıyor, giriş yapmayı deneyin!");
        //     var SmsCode = new Random().Next(10000, 100000);
        //     SendSMS.SendOTPCode(user.Id, user.Phone, SmsCode, user.Signature);
        //     return Ok(SmsCode);
        // }

        [HttpPost("updatePushNotificationToken")]
        public async Task<ActionResult> UpdatePushNotificationToken([FromBody] string token)
        {
            var user = this.GetUser();
            var device = await _context.Devices.FirstOrDefaultAsync(u => u.Id == user.DeviceId && u.CreaterId == user.Id);
            if (device == null) return Problem("Cihaz bulunamadı!");
            device.PNToken = token;
            await _context.SaveAsync(this);
            return Ok("PN Token güncellendi!");
        }
    }
}
