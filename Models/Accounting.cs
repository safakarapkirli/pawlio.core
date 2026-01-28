using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public enum AccountingStatus : int
{
    Wait = 0,
    Complated = 1,
    Cancel = 2,
}

public enum AccountingTypes : int
{
    /// <summary>
    /// Stok girişi
    /// </summary>
    AddStock = 1,
    /// <summary>
    /// Ürün satışı
    /// </summary>
    SaleProduct = 2,
    /// <summary>
    /// Hizmet. Saç kesimi vs.
    /// </summary>
    Service = 3,
    /// <summary>
    /// Aşılama
    /// </summary>
    Vaccine = 4,
    /// <summary>
    /// Tohumlama
    /// </summary>
    Insemination = 5,
    /// <summary>
    /// Muayene
    /// </summary>
    Examination = 6,
    /// <summary>
    /// Diğer işlemler, ödeme alma/verme gibi
    /// </summary>
    Other = 7,
}

public class Accounting : BaseAccounting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    public virtual Basket? Basket { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Supplier? Supplier { get; set; }

    // Cihazdan bu bilgi dolu geliyor, gelen bilgiyi almak için
    //[JsonIgnore]
    public virtual Appointment? Appointment { get; set; }

    public virtual List<ExaminationSymptom>? Symptoms { get; set; }

    public virtual List<AnimalAccounting>? Animals { get; set; }

    [JsonIgnore]
    public virtual List<Image>? ImageModels { get; set; }

    [NotMapped]
    public virtual List<string>? Images { get; set; }
}

public class BaseAccounting : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("BranchId")]
    public int BranchId { get; set; }

    [ForeignKey("BasketId")]
    public int BasketId { get; set; }

    [ForeignKey("SupplierId")]
    public int? SupplierId { get; set; }

    [ForeignKey("CustomerId")]
    public int? CustomerId { get; set; }

    [ForeignKey("AppointmentId")]
    public int? AppointmentId { get; set; }

    public AccountingStatus Status { get; set; }

    public AccountingTypes Type { get; set; }

    public int EventId { get; set; } // İlişki tablodaki işlemin Id si, Ürün, tohumlama vs. id si

    [MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(200)]
    public string Detail { get; set; } = "";

    /// <summary>
    /// İşlem adedi
    /// </summary>
    [Precision(10, 4)]
    public decimal Quantity { get; set; } = 0m;

    /// <summary>
    /// Alış fiyatı
    /// </summary>
    [Precision(10, 4)]
    public decimal Buying { get; set; } = 0m;

    /// <summary>
    /// İskonto, yüzde değil, indirim yapılan tutar
    /// </summary>
    [Precision(10, 4)]
    public decimal Discount { get; set; } = 0m;

    /// <summary>
    /// Satış fiyatı, toplam değil, toplam ayrıca hesaplanmalı
    /// </summary>
    [Precision(10, 4)]
    public decimal Amount { get; set; } = 0m;

    /// <summary>
    /// Kâr, toplam değil, toplam ayrıca hesaplanmalı
    /// </summary>
    [Precision(10, 4)]
    public decimal Profit { get; set; } = 0m;

    /// <summary>
    /// Vergi tutarı, toplam değil, toplam ayrıca hesaplanmalı
    /// </summary>
    [Precision(10, 4)]
    public decimal Tax { get; set; } = 0m;

    /// <summary>
    /// Vergi oranı, KDV yüzdesi
    /// </summary>
    [Precision(7, 2)]
    public decimal TaxRate { get; set; } = 0m;

    /// <summary>
    /// Datanın versiyonu. Yeni versiyon uygulamada geliştime olunca Json u hangi versiyona göre okuyacağını anlamak için
    /// </summary>
    public int DataVersion { get; set; }

    /// <summary>
    /// Olaya ait işlemin Json datası
    /// </summary>
    [MaxLength(2048)]
    public string Data { get; set; } = "";

    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;

    public new DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Accounting : BaseAccounting
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }