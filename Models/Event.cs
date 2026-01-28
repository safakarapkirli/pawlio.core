using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pawlio.Models;

public enum EventTypes : int
{
    User = 1,
    Firm = 2,
    Branch = 3,
    Customer = 4,
    Animal = 5,
    Product = 6,
    Supplier = 7,
    Stock = 8,
    Appointment = 9,
    /// <summary>
    /// Ürün satışı
    /// </summary>
    Sale = 10,
    /// <summary>
    /// Hizmet. Saç kesimi vs.
    /// </summary>
    Service = 11,
    /// <summary>
    /// Aşılama
    /// </summary>
    Vaccine = 12,
    /// <summary>
    /// Tohumlama
    /// </summary>
    Insemination = 13,
    /// <summary>
    /// Muhasebe işlemi
    /// </summary>
    Accounting = 14,
    /// <summary>
    /// Muhasebe işlemi
    /// </summary>
    Examination = 15,
    /// <summary>
    /// Kullanıcı Kaydı
    /// </summary>
    UserCreate = 90,

    AppointmentCreate = 101,
    AppointmentUpdate = 102,
    AppointmentDelete = 103,
    CustomerCreate = 104,
    CustomerUpdate = 105,
    CustomerDelete = 106,
    AnimalCreate = 107,
    AnimalUpdate = 108,
    AnimalDelete = 109,
    FirmCreate = 110,
    FirmUpdate = 111,
    FirmDelete = 112,
    BranchCreate = 113,
    BranchUpdate = 114,
    BranchDelete = 115,
    
    ProductCreate = 116,
    ProductUpdate = 117,
    ProductDelete = 118,
    ProductStockUpdate = 130,
    ProductPriceUpdate = 131,
    ProductBuyingPriceUpdate = 132,
    ProductBranchMove = 133,

    SupplierCreate = 119,
    SupplierUpdate = 120,
    SupplierDelete = 121,
    //StockIn = 122,
    //StockOut = 123,
    AnimalWeightAdd = 124,
    AccountingCreate = 125,
    AccountingUpdate = 126,
    AccountingDelete = 127,
    FirmUserCreate = 128,
    FirmUserDelete = 129,
}

public class Event : BaseEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [JsonIgnore]
    public virtual Firm? Firm { get; set; }

    [JsonIgnore]
    public virtual Branch? Branch { get; set; }

    //[JsonIgnore]
    //public virtual EventType Type { get; set; } = null!;
}

public class BaseEvent : ModelBase
{
    [ForeignKey("UserId")]
    public int UserId { get; set; }

    [ForeignKey("FirmId")]
    public int FirmId { get; set; }

    [ForeignKey("BranchId")]
    public int? BranchId { get; set; }

    [ForeignKey("CustomerId")]
    public int? CustomerId { get; set; }

    //[ForeignKey("AnimalId")]
    //public int? AnimalId { get; set; }

    //[ForeignKey("TypeId")]
    public int? TypeId { get; set; }

    public int EventId { get; set; } // İlişki tablodaki işlemin Id si

    public int EventSubId { get; set; } // İlişki tablodaki işleme ait VARSA ek int alan

    //[MaxLength(100)]
    //public string Title { get; set; } = "";

    //[MaxLength(200)]
    //public string Detail { get; set; } = "";

    //[MaxLength(32)]
    //public string Icon { get; set; } = "";
}
