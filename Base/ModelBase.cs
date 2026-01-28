using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public class ModelBase
{
    public required Flavor Flavor { get; set; } = Flavor.Pawlio;

    [JsonIgnore]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public int? CreaterId { get; set; }

    [JsonIgnore]
    public DateTimeOffset? Updated { get; set; }

    [JsonIgnore]
    public int? UpdaterId { get; set; }
}