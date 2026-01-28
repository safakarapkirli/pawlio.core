using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

// TEDARİKÇİ

public enum SupplierType : int { Individual, Corporate }

public class Supplier : BaseSupplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    public virtual Balance? Balance { get; set; }
}

public class BaseSupplier : ModelBase
{
    public SupplierType TypeId { get; set; }

    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = "";

    [MaxLength(16)]
    public string? IdNumber { get; set; }

    [MaxLength(100)]
    public string? TaxOffice { get; set; }

    [MaxLength(100)]
    public string? FirmName { get; set; }

    [MaxLength(16)]
    public string? Phone { get; set; }

    [MaxLength(10)]
    public string? Mobile { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? WebSite { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [Precision(9, 6)]
    public decimal? Lat { get; set; } = 0m;

    [Precision(9, 6)]
    public decimal? Lon { get; set; } = 0m;

    [MaxLength(200)]
    public string? Notes { get; set; }

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Supplier : BaseSupplier
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }