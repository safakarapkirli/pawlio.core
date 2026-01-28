using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class AnimalAccounting : BaseAnimalAccounting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Accounting? Accounting { get; set; }

    [JsonIgnore]
    public virtual Animal? Animal { get; set; }
}

public class BaseAnimalAccounting : ModelBase
{
    [ForeignKey("AccountingId")]
    public int AccountingId { get; set; }

    [ForeignKey("AnimalId")]
    public int AnimalId { get; set; }
}
