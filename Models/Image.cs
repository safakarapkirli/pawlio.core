 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public enum ImageType : int { User, Firm, Customer, Animal, Product, Accounting }

public class Image : BaseImage
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = "";

    public virtual Accounting? Accounting { get; set; }

    public virtual List<AnimalImage>? Animals { get; set; }
}

public class BaseImage : ModelBase
{
    public int ImageType { get; set; }

    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("BranchId")]
    public int? BranchId { get; set; }

    [ForeignKey("CustomerId")]
    public int? CustomerId { get; set; }

    //[ForeignKey("AnimalId")]
    //public int? AnimalId { get; set; }

    [ForeignKey("SupplierId")]
    public int? SupplierId { get; set; }

    [ForeignKey("ProductId")]
    public int? ProductId { get; set; }

    [ForeignKey("AccountingId")]
    public int? AccountingId { get; set; }

    [ForeignKey("ExaminationId")]
    public int? ExaminationId { get; set; }
}
