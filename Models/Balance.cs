using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class Balance : BaseBalance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    [JsonIgnore]
    public virtual Customer? Customer { get; set; }

    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }
}

public class BaseBalance : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("BranchId")]
    public int BranchId { get; set; }

    [ForeignKey("BranchId")]
    public int? CustomerId { get; set; }

    [ForeignKey("BranchId")]
    public int? SupplierId { get; set; }

    [Precision(10, 4)]
    public decimal Balance { get; set; } = 0;
}

//---------------- History

// public class _Balance : BaseBalance
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }