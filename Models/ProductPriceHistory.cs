using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class ProductPriceHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    //[JsonIgnore]
    //public virtual Firm? Firm { get; set; }
    //[ForeignKey("FirmId")]
    //public int FirmId { get; set; }

    //[JsonIgnore]
    //public virtual Branch? Branch { get; set; }
    //[ForeignKey("BranchId")]
    //public int BranchId { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [ForeignKey("ProductId")]
    public int ProductId { get; set; }

    [Precision(10, 4)]
    public decimal Amount { get; set; } = 0m;

    public int? CreaterId { get; set; }

    public virtual DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
}
