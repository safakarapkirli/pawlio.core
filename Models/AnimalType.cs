using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

//namespace Pawlio.Models;

//public class AnimalCategory
//{
//    [Key]
//    public int Id { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameTr { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameEn { get; set; }

//    public virtual ICollection<AnimalType> Types { get; set; } = [];
//}

//public class AnimalType
//{
//    [Key]
//    public int Id { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameTr { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameEn { get; set; }

//    public int CategoryId { get; set; }

//    [ForeignKey("CategoryId")]
//    public virtual AnimalCategory Category { get; set; } = null!;

//    public virtual ICollection<AnimalBreed> Breeds { get; set; } = [];

//    public virtual ICollection<AnimalColor> Colors { get; set; } = [];
//}

//public class AnimalBreed
//{
//    [Key]
//    public int Id { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameTr { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameEn { get; set; }

//    public int AnimalTypeId { get; set; }

//    [ForeignKey("AnimalTypeId")]
//    public virtual AnimalType AnimalType { get; set; } = null!;
//}

//public class AnimalColor
//{
//    [Key]
//    public int Id { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameTr { get; set; }

//    [Required]
//    [MaxLength(100)]
//    public required string NameEn { get; set; }

//    [MaxLength(200)]
//    public required string HexCode { get; set; } = "";

//    public int AnimalTypeId { get; set; }

//    [ForeignKey("AnimalTypeId")]
//    public virtual AnimalType AnimalType { get; set; } = null!;
//}

// JSON Deserialize Classes
// animal-types.json dosyasından okuyup ilk kayıtları oluşturmak için
public class AnimalCategoryJson
{
    [JsonProperty("tr")]
    public required string Tr { get; set; }

    [JsonProperty("en")]
    public required string En { get; set; }

    [JsonProperty("types")]
    public List<AnimalTypeJson> Types { get; set; } = [];
}

public class AnimalTypeJson
{
    [JsonProperty("id")]
    public required int Id { get; set; }

    [JsonProperty("tr")]
    public required string Tr { get; set; }

    [JsonProperty("en")]
    public required string En { get; set; }

    [JsonProperty("breeds")]
    public List<BreedJson> Breeds { get; set; } = [];

    [JsonProperty("colors")]
    public List<ColorJson> Colors { get; set; } = [];
}

public class BreedJson
{
    [JsonProperty("tr")]
    public required string Tr { get; set; }

    [JsonProperty("en")]
    public required string En { get; set; }
}

public class ColorJson
{
    [JsonProperty("tr")]
    public required string Tr { get; set; }

    [JsonProperty("en")]
    public required string En { get; set; }

    [JsonProperty("hex")]
    public required string Hex { get; set; }
}

// *********************************************

public class AnimalVaccineJson
{
    public int Id { get; set; } // JSON'daki id ile eşleşir

    [JsonPropertyName("type")]
    public string TypeName { get; set; }

    // Navigation Property: Bir hayvanın birçok aşısı olabilir
    public List<VaccineJson> Vaccines { get; set; } = new();
}

public class VaccineJson
{
    public int Id { get; set; } // Veritabanı için gerekli Primary Key

    public TranslationJson Name { get; set; } = null!;

    [JsonPropertyName("repeat_days")]
    public int RepeatDays { get; set; } = 0;

    [JsonPropertyName("initial_doses")]
    public int InitialDoses { get; set; } = 0;

    [JsonPropertyName("interval_days")]
    public int IntervalDays { get; set; } = 0;

    public TranslationJson Note { get; set; }
}

public class AnimalServiceJson
{
    public int Id { get; set; } // JSON'daki id ile eşleşir

    [JsonPropertyName("type")]
    public string TypeName { get; set; }

    // Navigation Property: Bir hayvanın birçok aşısı olabilir
    public List<ServiceJson> Services { get; set; } = new();
}

public class ServiceJson
{
    public int Id { get; set; } // Veritabanı için gerekli Primary Key

    public TranslationJson Name { get; set; } = null!;

    [JsonPropertyName("repeat_days")]

    public TranslationJson Note { get; set; }
}

public class TranslationJson
{
    [JsonPropertyName("tr")]
    public string Tr { get; set; }

    [JsonPropertyName("en")]
    public string En { get; set; }
}