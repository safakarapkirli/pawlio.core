using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

// ÜRÜN

public class Product : BaseProduct, IImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Definition? Category { get; set; }

    [JsonIgnore]
    public virtual Definition? Mark { get; set; }

    [JsonIgnore]
    public virtual Definition? Unit { get; set; }

    [JsonIgnore]
    public virtual Definition? PackContent { get; set; }

    [JsonIgnore]
    public virtual Definition? Concentration { get; set; }

    [JsonIgnore]
    public virtual Definition? TaxRate { get; set; }

    public List<ProductAmount> Amounts { get; set; } = new List<ProductAmount>();

    public List<ProductPriceHistory>? PriceHistory { get; set; }
}

public class BaseProduct : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    /// <summary>
    /// Kategori
    /// </summary>
    [ForeignKey("CategoryId")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Marka
    /// </summary>
    [ForeignKey("MarkId")]
    public int? MarkId { get; set; }

    /// <summary>
    /// Birim
    /// </summary>
    [ForeignKey("UnitId")] 
    public int? UnitId { get; set; }

    /// <summary>
    /// Paket İçeriği
    /// </summary>
    [ForeignKey("PackContentId")]
    public int? PackContentId { get; set; }

    /// <summary>
    /// Konsantrasyon
    /// </summary>
    [ForeignKey("ConcentrationId")]
    public int? ConcentrationId { get; set; }

    /// <summary>
    /// Vergi Oranı
    /// </summary>
    [ForeignKey("TaxRateId")]
    public int? TaxRateId { get; set; }

    /// <summary>
    /// Ürün adı
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = "";

    /// <summary>
    /// Ürün detayı
    /// </summary>
    [MaxLength(200)]
    public string Nots { get; set; } = "";

    /// <summary>
    /// Pakette ne kadar var?
    /// </summary>
    [Precision(7, 2)]
    public decimal PackContentAmount { get; set; } = 0m;

    /// <summary>
    /// Konsantrasyon ne kadar?
    /// </summary>
    [Precision(7, 2)]
    public decimal ConcentrationAmount { get; set; } = 0m;

    /// <summary>
    /// Barkod numarası
    /// </summary>
    [MaxLength(100)]
    public string Barcode { get; set; } = "";

    /// <summary>
    /// Seri numarası
    /// </summary>
    [MaxLength(50)]
    public string SerialNumber { get; set; } = "";

    /// <summary>
    /// Tarbil sistemi seri numarası
    /// </summary>
    [MaxLength(50)]
    public string TarbilSerialNumber { get; set; } = "";

    /// <summary>
    /// Alış Fiyatı
    /// </summary>
    [Precision(10, 4)]
    public decimal Buying { get; set; } = 0m;

    /// <summary>
    /// Fiyat
    /// </summary>
    [Precision(10, 4)]
    public decimal Price { get; set; } = 0m;

    /// <summary>
    /// Stok miktarı
    /// </summary>
    //[Precision(7, 2)]
    //public decimal Amount { get; set; } = 0m;

    /// <summary>
    /// Kritik stok miktarı
    /// </summary>
    [Precision(7, 2)]
    public decimal CriticalAmount { get; set; } = 0m;

    /// <summary>
    /// Kritik stok miktarı
    /// </summary>
    public bool CriticalAmountAlert { get; set; } = true;

    [MaxLength(36)]
    public string? ImageId { get; set; }

    [NotMapped]
    public byte[]? ImageData { get; set; }

    public new DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Product : BaseProduct
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }