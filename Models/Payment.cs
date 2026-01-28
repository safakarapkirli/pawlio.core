using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models; 

public enum PaymentStatus : int
{
    Wait = 0,
    Payed = 1,
    Cancel = 2,
}

public enum PaymentTypes : int
{
    Cash = 0,
    CreditCard = 1,
    Check = 2,  
    Senet = 3,
    Other = 4,
}

public class Payment : BasePayment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    public virtual Basket? Basket { get; set; }

    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    [JsonIgnore]
    public virtual Customer? Customer { get; set; }
}

public class BasePayment : ModelBase
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

    /// <summary>
    /// Ödemenin durumu, bekliyor = 0, ödendi = 1, iptal = 2 vs.
    /// </summary>
    public PaymentStatus Status { get; set; }

    public PaymentTypes PaymentType { get; set; }

    /// <summary>
    /// Ödenecek tutar
    /// </summary>
    [Precision(10, 4)]
    public decimal Amount { get; set; } = 0m;

    /// <summary>
    /// Ödemeye ait not
    /// </summary>
    [MaxLength(200)]
    public string Nots { get; set; } = "";

    public DateTimeOffset? PayDate { get; set; }

    // Pos cihazından ödeme yapılınca gelen bilgiler
    public int AcquirerId { get; set; }
    public string RefNo { get; set; } = "";
    public string AuthCode { get; set; } = "";
    public string ResponseCode { get; set; } = "";
    public int BatchNo { get; set; }
    public int StanNo { get; set; }
    public string CreditCardNo { get; set; } = "";
    public string CreditCardName { get; set; } = "";

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Payment : BasePayment
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }