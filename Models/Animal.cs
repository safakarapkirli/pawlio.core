using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pawlio.Models;

public class Animal : BaseAnimal, IImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public virtual Customer? Owner { get; set; }

    public virtual Firm? Firm { get; set; }

    public Definition? Category { get; set; }

    public Definition? Type { get; set; }

    public Definition? Race { get; set; }

    public Definition? Color { get; set; }

    public virtual List<AnimalWeight>? WeightHistory { get; set; }
}

public class BaseAnimal : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("OwnerId")]
    public int OwnerId { get; set; }

    [MaxLength(50)]
    public string? Name { get; set; }

    [ForeignKey("CategoryId")]
    public int? CategoryId { get; set; }

    [ForeignKey("TypeId")]
    public int? TypeId { get; set; }

    [ForeignKey("RaceId")]
    public int? RaceId { get; set; }

    [MaxLength(30)]
    public string IdNumber { get; set; } = "";

    public DateTimeOffset? BirthDate { get; set; }

    public bool? Gender { get; set; }

    [MaxLength(30)]
    public string MotherIdNumber { get; set; } = "";

    [MaxLength(30)]
    public string FatherIdNumber { get; set; } = "";

    [ForeignKey("ColorId")]
    public int? ColorId { get; set; }

    //[Precision(7, 2)]
    //public decimal Weight { get; set; } = 0;

    /// <summary>
    /// Saldırgan mı?
    /// </summary>
    public bool Attacker { get; set; }

    /// <summary>
    /// Kısırlaştılılmış mı?
    /// </summary>
    public bool Neutered { get; set; }

    /// <summary>
    /// Huyu nasıl?
    /// </summary>
    [MaxLength(200)]
    public string Habit { get; set; } = "";

    /// <summary>
    /// Ayırt edici özelliği nedir?
    /// </summary>
    [MaxLength(200)]
    public string DistinctiveFeature { get; set; } = "";

    [MaxLength(200)]
    public string Notes { get; set; } = "";

    [MaxLength(36)]
    public string? ImageId { get; set; }

    [NotMapped]
    public byte[]? ImageData { get; set; }

    public new DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Animal : BaseAnimal
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }
//     public int Id { get; set; }
// }

public class MAnimal
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int TypeId { get; set; }
    public int RaceId { get; set; }
    public int ColorId { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string? ImageId { get; set; }
}
