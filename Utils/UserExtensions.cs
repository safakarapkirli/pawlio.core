using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pawlio;
using Pawlio.Models;

public static class Sessions
{
    public static Dictionary<int, User> users = new Dictionary<int, User>();

    public static User GetUser(int userId)
    {
        User? user;

        if (!users.TryGetValue(userId, out user))
        {
            var _context = new PostgreSqlDbContext();
            user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new Exception($"{userId} nolu kullanıcı bulunamadı!");
            users.Add(userId, user);
        }

        if (!user.IsActive) throw new Exception("Kullanıcı hesabı pasif durumdadır, giriş yapılamaz!");
        return user;
    }

    public static void SetUser(User user)
    {
        users.Remove(user.Id);
        users.Add(user.Id, user);
    }
}

public class TokenUser
{
    public Flavor Flavor { get; set; }
    public int Id { get; set; }
    public string SessionId { get; set; }
    public int FirmId { get; set; }
    public int BranchId { get; set; }
    public FirmTypes FirmType { get; set; }
    public int PosId { get; set; }
    public bool IsPos { get => PosId > 0; }
    public string DeviceId { get; set; }
    public string Language { get; set; }
    public User User { get; set; }

    public bool isVeterinary
    {
        get => Flavor == Flavor.Pawlio; //FirmType == FirmTypes.Veterinary;
    }

    public bool isEn => Language == "en";
    public bool isTr => Language == "tr";

    public TokenUser(Flavor flavor, int id, string sessionId, int firmId, int branchId, User user, int posId, string deviceId, string language)
    {
        Flavor = flavor;
        Id = id;
        FirmId = firmId;
        BranchId = branchId;
        SessionId = sessionId;
        User = user;
        PosId = posId;
        DeviceId = deviceId;
        Language = language;
    }
}
