using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public enum DefinitionValueType : int
{
    None = 0,
    String = 1,
    Int = 2,
    Decimal = 3,
    Service = 9,
    Vaccine = 10,
    Insemination = 11,
    Examination = 12,
}


public class Definition : BaseDefinition
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Definition? Parent { get; set; }

    [JsonProperty("c")]
    public virtual List<Definition>? SubDefinitions { get; set; }

    //[JsonIgnore]
    //public new DateTime Created { get; set; }
}

public class BaseDefinition : ModelBase
{
    [MaxLength(50)]
    public string Key { get; set; } = "";

    [MaxLength(100)]
    [JsonProperty("tr")]
    public string NameTr { get; set; } = "";

    [MaxLength(100)]
    [JsonProperty("en")]
    public string NameEn { get; set; } = "";

    [JsonProperty("vt")]
    public DefinitionValueType ValueType { get; set; }

    [MaxLength(200)]
    [JsonProperty("v")]
    public string? Value { get; set; }

    [MaxLength(200)]
    [JsonProperty("dtr")]
    public string? DetailsTr { get; set; }

    [MaxLength(200)]
    [JsonProperty("den")]
    public string? DetailsEn { get; set; }

    [ForeignKey("ParentId")]
    [JsonProperty("pid")]
    public int? ParentId { get; set; }

    [ForeignKey("FirmId")]
    [JsonProperty("fid")]
    public int FirmId { get; set; }

    [JsonProperty("add")]
    public bool AddSubDefinitions { get; set; } = false;

    [JsonProperty("s")]
    public bool Static { get; set; } = false;

    [MaxLength(20)]
    [JsonProperty("i")]
    public string? Icon { get; set; }
}

//---------------- History

// public class _Definition : BaseDefinition
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }
//     public int Id { get; set; }
// }