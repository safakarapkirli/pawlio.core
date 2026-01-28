using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Pawlio.Controllers;

namespace Pawlio.Models;
public class User : BaseUser, IImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public virtual List<UserFirm>? UserFirms { get; set; }

    [NotMapped]
    public virtual List<MUser>? AssociatedUsers { get; set; }
}

public class BaseUser : ModelBase
{

    [MaxLength(16)]
    public string IdentityNumber { get; set; } = "";

    [MaxLength(50)]
    public string Nationality { get; set; } = "";

    public DateTimeOffset? BirthDate { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = "";

    [MaxLength(50)]
    public string Title { get; set; } = "";

    [MaxLength(16)]
    public string Phone { get; set; } = "";

    [MaxLength(50)]
    public string Email { get; set; } = "";

    public byte UserTypeId { get; set; }

    public byte JobId { get; set; }

    // Login

    public LoginType LoginType { get; set; }

    [MaxLength(128)]
    public string? ExternalUserId { get; set; }

    [MaxLength(128)]
    public string? AppleAuthCode { get; set; }

    [MaxLength(2048)]
    public string? AppleIdToken { get; set; }



    [MaxLength(36)]
    public string Password { get; set; } = "";

    [MaxLength(36)]
    public string? ImageId { get; set; }

    [NotMapped]
    public byte[]? ImageData { get; set; }

    [MaxLength(1000)]
    public string About { get; set; } = "";

    [MaxLength(1000)]
    public string AboutNew { get; set; } = "";

    public bool AcceptCitizen { get; set; } = false;

    public bool IsAdmin { get; set; }

    /// <summary>
    /// Aktif değil ise sisteme giriş yapamayacak
    /// </summary>
    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    // Location
    [Precision(9, 6)]
    public decimal? Lat { get; set; } = 0m;

    [Precision(9, 6)]
    public decimal? Lon { get; set; } = 0m;

    // SMS
    public bool SendSms { get; set; }

    [JsonIgnore]
    public int SmsCode { get; set; }

    [JsonIgnore]
    public DateTimeOffset? SmsEndTime { get; set; }

    // EMAIL
    public bool SendEMail { get; set; }

    [JsonIgnore]
    public int EMailCode { get; set; }

    [JsonIgnore]
    public DateTimeOffset? EMailEndTime { get; set; }

    //[MaxLength(2048)]
    //public string? PNToken { get; set; }

    [MaxLength(2048)]
    public string? Token { get; set; }

    [MaxLength(36)]
    public string SessionId { get; set; } = string.Empty;

    public DateTimeOffset? LastLogin { get; set; }

    [JsonIgnore]
    public bool IsDeleted { get; set; }

    [NotMapped]
    public Device? Device { get; set; }

    [NotMapped]
    public string Signature { get; set; } = string.Empty;
}

//---------------- History

// public class _User : BaseUser
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }
//     public int Id { get; set; }
// }

public class MUser
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public byte UserTypeId { get; set; }
    public byte JobId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ImageId { get; set; }
    public bool IsActive { get; set; }
}