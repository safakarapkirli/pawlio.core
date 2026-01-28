using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

// ÜRÜN

public class ProductAmount : BaseProductAmount
{
    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }
}

public class BaseProductAmount
{
    [ForeignKey("BranchId")]
    public int BranchId { get; set; }

    [ForeignKey("ProductId")]
    public int ProductId { get; set; }

    /// <summary>
    /// Stok miktarı
    /// </summary>
    [Precision(10, 4)]
    public decimal Amount { get; set; } = 0m;

    // Burası daha sonra şubeye göre olacak şekilde ayarlanabilir
    ///// <summary>
    ///// Kritik stok miktarı
    ///// </summary>
    //[Precision(7, 2)]
    //public decimal CriticalAmount { get; set; }

    ///// <summary>
    ///// Kritik stok miktarı
    ///// </summary>
    //public bool CriticalAmountAlert { get; set; } = true;
}

//---------------- History

//public class _ProductAmount : BaseProductAmount
//{
//    [Key]
//    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//    public int bId { get; set; }

//    public DateTime? Updated { get; set; }
//}