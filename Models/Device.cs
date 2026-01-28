using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace Pawlio.Models;

public class Device : BaseDevice
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonIgnore]
    public new DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
}

public class BaseDevice : ModelBase
{

    [MaxLength(2000)]
    public string PNToken { get; set; } = string.Empty;

    [MaxLength(100)]
    public string AppName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PackageName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Version { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BuildNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string BuildSignature { get; set; } = string.Empty;
}

//---------------- History

// public class _Device : BaseDevice
// {
//     [Key]
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public int bId { get; set; }

//     [MaxLength(36)]
//     public string Id { get; set; } = null!;
// }