using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class UserFirm : BaseUserFirm
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public virtual User? User { get; set; }

    public virtual Firm? Firm { get; set; }

    public List<UserFirmsBranch>? Branches { get; set; }

    //[NotMapped]
    //public List<Package>? Packages { get; set; }
}

public class BaseUserFirm : ModelBase
{
    [ForeignKey("UserId")]
    public int UserId { get; set; }

    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    public bool IsDefault { get; set; }

    public bool IsAdmin { get; set; }
}

//---------------- History

// public class _UserFirm : BaseUserFirm
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }