using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class AnimalWeight : BaseAnimalWeight
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public virtual Animal? Animal { get; set; }
}

public class BaseAnimalWeight : ModelBase
{
    [Precision(7, 2)]
    public decimal Weight { get; set; } = 0m;

    [ForeignKey("AnimalId")]
    public int AnimalId { get; set; }

    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    [MaxLength(200)]
    public string Notes { get; set; } = "";
}

//---------------- History

// public class _AnimalWeight : BaseAnimalWeight
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }
//     public int Id { get; set; }
// }