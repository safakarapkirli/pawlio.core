using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Pawlio.Models;

public class Customer : BaseCustomer, IImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    //public virtual City? City { get; set; }

    //public virtual District? District { get; set; }

    public virtual Definition? Group { get; set; }

    public virtual Definition? Source { get; set; }

    public virtual Firm? Firm { get; set; }

    public virtual List<Animal>? Animals { get; set; }

    public virtual Balance? Balance { get; set; }
}

public class BaseCustomer : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    public bool Personal { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [MaxLength(16)]
    public string? IdNumber { get; set; }

    [MaxLength(16)]
    public string? Phone { get; set; }

    public bool PhoneVerified { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    public bool EmailVerified { get; set; }

    public DateTimeOffset? BirthDate { get; set; }

    //[ForeignKey("CityId")]
    //public int? CityId { get; set; }

    //[ForeignKey("DistrictId")]
    //public int? DistrictId { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    public bool SendSms { get; set; }

    public bool SendEmail { get; set; }

    [ForeignKey("GroupId")]
    public int? GroupId { get; set; }

    [ForeignKey("SourceId")]
    public int? SourceId { get; set; }

    [MaxLength(20)]
    public string? Job { get; set; }

    [MaxLength(36)]
    public string? ImageId { get; set; }

    [NotMapped]
    public byte[]? ImageData { get; set; }

    public new DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Customer : BaseCustomer
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }
//     public int Id { get; set; }
// }

public class MCustomer
{
    public int Id { get; set; }
    public bool Personal { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ImageId { get; set; }
    public Balance? Balance { get; set; }
}