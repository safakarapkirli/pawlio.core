using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;
public enum AppointmentStatus : int { Wait = 1, Success = 2, Cancel = 3, CustomerNotCome = 4 }

public class Appointment : BaseAppointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    //[JsonIgnore]
    //public User? User { get; set; }

    [JsonIgnore]
    public Firm? Firm { get; set; }

    [JsonIgnore]
    public Branch? Branch { get; set; }

    [JsonIgnore]
    public Customer? Customer { get; set; }

    [JsonIgnore]
    public Supplier? Supplier { get; set; }

    //[JsonIgnore]
    //public Animal? Animal { get; set; }

    [JsonIgnore]
    public Definition? Subject { get; set; }

    [JsonIgnore]
    public Basket? Basket { get; set; }

    public virtual List<AnimalAppointment> Animals { get; set; } = new List<AnimalAppointment>();
}

public class BaseAppointment : ModelBase
{
    //[ForeignKey("UserId")]
    //public int UserId { get; set; }

    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("BranchId")]
    public int? BranchId { get; set; }

    [ForeignKey("CustomerId")]
    public int? CustomerId { get; set; }

    [ForeignKey("SupplierId")]
    public int? SupplierId { get; set; }

    //[ForeignKey("AnimalId")]
    //public int? AnimalId { get; set; }

    [ForeignKey("SubjectId")]
    public int? SubjectId { get; set; }

    [ForeignKey("BasketId")]
    public int? BasketId { get; set; }

    [MaxLength(200)]
    public string Notes { get; set; } = "";

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public bool AllDay { get; set; }

    [Precision(9, 6)]
    public decimal? Lat { get; set; } = 0m;

    [Precision(9, 6)]
    public decimal? Lon { get; set; } = 0m;

    public int JobId { get; set; }

    public int Status { get; set; } = 1;

    [JsonIgnore]
    public bool IsDeleted { get; set; }
}

//---------------- History

// public class _Appointment : BaseAppointment
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     public int Id { get; set; }
// }