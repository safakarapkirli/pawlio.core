using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class UserFirmsBranch : BaseUserFirmsBranch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    [JsonIgnore]
    public virtual UserFirm? UserFirm { get; set; }
}

public class BaseUserFirmsBranch : ModelBase
{
    [ForeignKey("BranchId")]
    public int BranchId { get; set; }

    [ForeignKey("UserFirmId")]
    public int UserFirmId { get; set; }

    public bool IsDefault { get; set; }

    public int PositionId { get; set; }
}

//---------------- History

// public class _UserFirmsBranch : BaseUserFirmsBranch
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }