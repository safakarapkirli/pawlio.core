using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public enum Flavor : byte
{
    Pawlio = 0,
    Procio = 1,
}
