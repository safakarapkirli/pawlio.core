using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class AnimalAppointment : BaseAnimalAppointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Appointment? Appointment { get; set; }

    [JsonIgnore]
    public virtual Animal? Animal { get; set; }
}

public class BaseAnimalAppointment : ModelBase
{
    [ForeignKey("AnimalId")]
    public int AnimalId { get; set; }

    [ForeignKey("AppointmentId")]
    public int AppointmentId { get; set; }
}
