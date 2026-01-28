using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class Basket : BaseBasket
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    [JsonIgnore]
    public virtual Customer? Customer { get; set; }

    public virtual List<Accounting>? Accountings { get; set; }

    public virtual List<Payment>? Payments { get; set; }

    [NotMapped]
    public string Uid { get; set; } = string.Empty;
}

public class BaseBasket : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("BranchId")]
    public int BranchId { get; set; }

    [ForeignKey("SupplierId")]
    public int? SupplierId { get; set; }

    [ForeignKey("CustomerId")]
    public int? CustomerId { get; set; }

    //[Precision(10, 4)]
    //public int Quantity { get; set; }

    [Precision(10, 4)]
    public decimal TotalAmount { get; set; } = 0m;

    [Precision(10, 4)]
    public decimal TotalProfit { get; set; } = 0m;

    [Precision(10, 4)]
    public decimal TotalTax { get; set; } = 0m;

    // Vuk507 fatura no
    public string InvoiceNo { get; set; } = "";

    public new DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Basket : BaseBasket
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }