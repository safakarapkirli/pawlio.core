using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class Branch : BaseBranch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    /// <summary>
    /// Şubede yetkili olan kullanıcılar
    /// Db de UserFirmBranches dan alınıyor, veri tabanı ile ilişkili değil
    /// Controller tarafından dolduruluyor
    /// </summary>
    /// 
    [NotMapped]
    public virtual List<int>? UserIds { get; set; }
}

public class BaseBranch : ModelBase
{
    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = "";

    [JsonIgnore]
    public bool IsDeleted { get; set; }

    // Randevu ayarları / Şubeye özel
    public byte AppointmentStartTimeHour { get; set; } = 09;
    public byte AppointmentStartTimeMinute { get; set; } = 00;
    public byte AppointmentEndTimeHour { get; set; } = 17;
    public byte AppointmentEndTimeMinute { get; set; } = 00;

    public bool LunchBreak { get; set; } = true;
    public byte LunchBreakStartHour { get; set; } = 12;
    public byte LunchBreakStartMinute { get; set; } = 00;
    public byte LunchBreakEndHour { get; set; } = 13;
    public byte LunchBreakEndMinute { get; set; } = 00;

    public byte AppointmentTime { get; set; } = 15;
    public byte AppointmentCount { get; set; } = 1;
    public byte AppointmentNotifyTime { get; set; } = 15;
}

//---------------- History

// public class _Branch : BaseBranch
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }