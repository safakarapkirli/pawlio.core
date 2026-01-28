using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class AnimalImage : BaseAnimalImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Image? Image { get; set; }

    [JsonIgnore]
    public virtual Animal? Animal { get; set; }
}

public class BaseAnimalImage : ModelBase
{
    [ForeignKey("ImageId")]
    [MaxLength(36)]
    public string ImageId { get; set; } = string.Empty;

    [ForeignKey("AppointmentId")]
    public int AnimalId { get; set; }
}
