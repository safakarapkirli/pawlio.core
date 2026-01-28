using Pawlio;
using Pawlio.Models;

public static class ModelExtensions
{
    public static MUser? ToMUser(this User customer) => customer == null ? null : new MUser
    {
        Id = customer.Id,
        Title = customer.Title,
        Nationality = customer.Nationality,
        JobId = customer.JobId,
        UserTypeId = customer.UserTypeId,
        Name = customer.Name,
        Phone = customer.Phone,
        Email = customer.Email,
        ImageId = customer.ImageId,
        IsActive = customer.IsActive,
    };

    public static MCustomer? ToMCustomer(this Customer customer) => customer == null ? null : new MCustomer
    {
        Id = customer.Id,
        Personal = customer.Personal,
        Name = customer.Name,
        Phone = customer.Phone ?? "",
        Email = customer.Email ?? "",
        ImageId = customer.ImageId,
        Balance = customer.Balance,
    };

    public static IQueryable<MCustomer?> ToMCustomer(this IQueryable<Customer> source) =>
        source.Select(c => c.ToMCustomer());

    public static MAnimal? ToMAnimal(this Animal animal) => animal == null ? null : new MAnimal
    {
        Id = animal.Id,
        OwnerId = animal.OwnerId,
        Name = animal.Name ?? "",
        CategoryId = animal.CategoryId ?? 0,
        TypeId = animal.TypeId ?? 0,
        RaceId = animal.RaceId ?? 0,
        ColorId = animal.ColorId ?? 0,
        IdNumber = animal.IdNumber ?? "",
        ImageId = animal.ImageId,
    };

    public static IQueryable<MAnimal?> ToMAnimal(this IQueryable<Animal> source) =>
        source.Select(a => a.ToMAnimal());

    //public static Accounting? ToAccounting(this Accounting accounting) => accounting == null ? null : new Accounting
    //{
    //    Id = accounting.Id,
    //    FirmId = accounting.FirmId,
    //    BranchId = accounting.BranchId,
    //    CreaterId = accounting.CreaterId,
    //    Amount = accounting.Amount,
    //    //AnimalId = accounting.AnimalId,
    //    AppointmentId = accounting.AppointmentId,
    //    BasketId = accounting.BasketId,
    //    Buying = accounting.Buying,
    //    Created = accounting.Created,
    //    CustomerId = accounting.CustomerId,
    //    Data = accounting.Data,
    //    DataVersion = accounting.DataVersion,
    //    Date = accounting.Date,
    //    Discount = accounting.Discount,
    //    Detail = accounting.Detail,
    //    EventId = accounting.EventId,
    //    EventSubId = accounting.EventSubId,
    //    Profit = accounting.Profit,
    //    Quantity = accounting.Quantity,
    //    Tax = accounting.Tax,
    //    TaxRate = accounting.TaxRate,
    //    Title = accounting.Title,
    //    Type = accounting.Type,
    //    TypeId = accounting.TypeId,
    //    Appointment = accounting.Appointment,
    //    Branch = accounting.Branch,
    //    Firm = accounting.Firm,
    //    Icon = accounting.Icon,
    //    ImageModels = accounting.ImageModels,
    //    SupplierId = accounting.SupplierId,
    //    Updated = accounting.Updated,
    //    UpdaterId = accounting.UpdaterId,
    //    Animals = accounting.Animals,
    //    Images = accounting.ImageModels?.Select(im => im.Id).ToList(),
    //};

    //public static IQueryable<Accounting?> ToAccounting(this IQueryable<Accounting> source) =>
    //    source.Select(a => a.ToAccounting());

    public static dynamic? ToAccounting(this Accounting a) => a == null ? null : new
    {
        a.Flavor,
        a.Id,
        a.FirmId,
        a.BranchId,
        a.Created,
        a.CreaterId,
        a.CustomerId,
        a.SupplierId,
        a.Status,
        a.Type,
        a.BasketId,
        a.EventId,
        a.Buying,
        a.Amount,
        a.Quantity,
        a.Profit,
        a.Tax,
        a.TaxRate,
        a.Date,
        a.AppointmentId,
        a.Title,
        a.Detail,
        a.Discount,
        a.Data,
        a.DataVersion,
        a.Symptoms,
        Images = a.ImageModels?.Select(i => i.Id),
        Animals = a.Animals?.Select(an => new { an.Flavor, an.Id, an.AccountingId, an.AnimalId, Animal = an.Animal?.ToMAnimal() })
    };

    public static IQueryable<dynamic?> ToAccounting(this IQueryable<Accounting> source) =>
        source.Select(a => a.ToAccounting());

    public static IQueryable<dynamic> ToBasket(this IQueryable<Basket> source) =>
        source.Select(b => new
        {
            b.Flavor,
            b.Id,
            b.FirmId,
            b.BranchId,
            b.CreaterId,
            b.Created,
            b.TotalAmount,
            b.TotalProfit,
            b.TotalTax,
            b.CustomerId,
            b.SupplierId,
            b.Payments,
            b.InvoiceNo,
            Accountings = b.Accountings == null ? null : b.Accountings.Select(a => a.ToAccounting())
        });

    public static IQueryable<dynamic> ToAppointment(this IQueryable<Appointment> source) =>
        source.Select(a => new
        {
            a.Flavor,
            a.Id,
            a.FirmId,
            a.BranchId,
            a.CustomerId,
            a.CreaterId,
            a.Status,
            a.StartTime,
            a.EndTime,
            a.Notes,
            a.AllDay,
            a.SubjectId,
            a.Lat,
            a.Lon,
            a.Created,
            Animals = a.Animals == null ? null : a.Animals.Select(ap => new
            {
                ap.Flavor,
                ap.Id,
                ap.AppointmentId,
                ap.AnimalId,
                Animal = ap.Animal == null ? null : ap.Animal.ToMAnimal()
            })
        });

    //public static List<Package> GetPackages(Firm firm, string packageName)
    //{
    //    var packages = Settings.Packages.ToJson().JsonTo<List<Package>>()!;
    //    foreach (var p in packages)
    //    {
    //        p.IsDisabled = p.Id < firm.PackageId;
    //        foreach (var s in p.SubPackages)
    //        {
    //            s.AppleIapId = s.AppleIapId.Replace("com.hizliis", packageName);
    //        }
    //    }

    //    return packages;
    //}

    public static IQueryable<UserFirm> ToUserFirm(this IQueryable<UserFirm> source, string packageName) =>
        source.Select(uf => new UserFirm
        {
            Flavor = uf.Flavor,
            Id = uf.Id,
            FirmId = uf.FirmId,
            CreaterId = uf.CreaterId,
            Firm = uf.Firm == null ? null : new Firm
            {
                Flavor = uf.Firm.Flavor,
                Id = uf.Firm.Id,
                Name = uf.Firm.Name,
                //CityId = uf.Firm.CityId,
                //DistrictId = uf.Firm.DistrictId,
                Created = uf.Firm.Created,
                CreaterId = uf.Firm.CreaterId,
                Phone = uf.Firm.Phone,
                Email = uf.Firm.Email,
                ImageId = uf.Firm.ImageId,
                Branches = uf.Firm.Branches == null ? null : uf.Firm.Branches.Where(b => !b.IsDeleted).ToList(),
                //PackageId = uf.Firm.PackageId,
                TimeOut = uf.Firm.TimeOut,
                FirmType = uf.Firm.FirmType,
            },
            Branches = uf.Branches!.Where(b => !b.Branch!.IsDeleted).OrderBy(b => b.Id).ToList(),
            IsDefault = uf.IsDefault,
            IsAdmin = uf.IsAdmin,
            UserId = uf.User!.Id,
            User = new User
            {
                Flavor = uf.User.Flavor,
                Id = uf.User.Id,
                Nationality = uf.User.Nationality,
                Title = uf.User.Title,
                JobId = uf.User.JobId,
                UserTypeId = uf.User.UserTypeId,
                Name = uf.User.Name,
                Email = uf.User.Email,
                Phone = uf.User.Phone,
                ImageId = uf.User.ImageId,
                IsActive = uf.User.IsActive,
            },
            //Packages = GetPackages(uf.Firm!, packageName),
        });
}
