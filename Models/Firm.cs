using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace Pawlio.Models;

public enum FirmTypes : int { Common = 0, Veterinary  }

public class Firm : BaseFirm, IImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    //public City? City { get; set; }

    //public District? District { get; set; }

    //public Package? Package { get; set; }

    [JsonIgnore]
    public virtual List<Definition>? Definitions { get; set; }

    public virtual List<Branch>? Branches { get; set; }

    [NotMapped]
    public virtual List<int>? UserIds { get; set; }
}

public class BaseFirm : ModelBase
{
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(16)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    //[ForeignKey("CityId")]
    //public int? CityId { get; set; }

    //[ForeignKey("DistrictId")]
    //public int? DistrictId { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [Precision(9, 6)]
    public decimal? Lat { get; set; } = 0m;

    [Precision(9, 6)]
    public decimal? Lon { get; set; } = 0m;

    //public int PackageId { get; set; }

    public DateTimeOffset TimeOut { get; set; }

    [MaxLength(36)]
    public string? ImageId { get; set; }

    [NotMapped]
    public byte[]? ImageData { get; set; }

    [JsonIgnore]
    public bool IsDeleted { get; set; }

    public FirmTypes FirmType { get; set; } = FirmTypes.Common;
}

//---------------- History

// public class _Firm : BaseFirm
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }