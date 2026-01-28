using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class ExaminationSymptom : BaseExaminationSymptom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Accounting? Accounting { get; set; }

    [JsonIgnore]
    public virtual Symptom? Symptom { get; set; }
}

public class BaseExaminationSymptom : ModelBase
{
    [ForeignKey("AccountingId")]
    public int AccountingId { get; set; }

    [ForeignKey("SymptomId")]
    public int SymptomId { get; set; }
}
