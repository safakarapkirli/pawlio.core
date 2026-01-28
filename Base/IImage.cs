using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pawlio.Controllers;

namespace Pawlio.Models;

public interface IImage
{
    public string? ImageId { get; set; }

    public byte[]? ImageData { get; set; }

    static string? getFileName(Image image)
    {
        var path = Path.GetFullPath("AppData");
        switch (image.ImageType)
        {
            case (int)ImageType.User: return path + $"/users/{image.Id}.png";
            case (int)ImageType.Firm: return path + $"/{image.FirmId}/images/firms/{image.Id}.png";
            case (int)ImageType.Customer: return path + $"/{image.FirmId}/images/customers/{image.Id}.png";
            case (int)ImageType.Animal: return path + $"/{image.FirmId}/images/animals/{image.Id}.png";
            case (int)ImageType.Product: return path + $"/{image.FirmId}/images/products/{image.Id}.png";
            case (int)ImageType.Accounting: return path + $"/{image.FirmId}/images/accountings/{image.Id}.png";
            //case (int)ImageType.Examination: return path + $"/{image.FirmId}/images/examinations/{image.Id}.png";
            default: return null;
        }
    }

    public void Save(ApiController controller, PostgreSqlDbContext _dbContext, TokenUser user, ImageType imageType,
        int? customerId = null, List<int>? animalIds = null, int? supplierId = null, int? productId = null, int? accountingId = null, int? examinationId = null)
    {
        if (ImageId == null || ImageId.Length != 36) throw new Exception("Id, GUID olmalıdır!");
        if (ImageData == null) throw new Exception("Resim içeriği gönderilmeli!");

        if (animalIds == null) animalIds = new List<int>();
        var animals = animalIds.Select(a => new AnimalImage { Flavor = user.Flavor, AnimalId = a }).ToList();
        var image = new Image
        {
            Flavor = user.Flavor,
            Id = ImageId,
            CreaterId = user.Id,
            FirmId = user.FirmId,
            BranchId = user.BranchId,
            CustomerId = customerId,
            SupplierId = supplierId,
            ProductId = productId,
            AccountingId = accountingId,
            ExaminationId = examinationId,
            ImageType = (int)imageType,
            Animals = animals,
        };

        var file = getFileName(image);
        if (file == null) throw new Exception("Resim türü geçerli değil!");

        var path = Path.GetDirectoryName(file);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path!);
        
        File.WriteAllBytes(file, ImageData);
        _dbContext.Images.Add(image);
        _dbContext.Save(controller);

        //var img = await _dbContext.Images.OrderByDescending(i => i.Id).FirstOrDefaultAsync(x => x.RowId == user.Id && x.ImageType == ImageType.Customer);
        //if (img != null)
        //    _dbContext.Database.ExecuteSqlRaw($"UPDATE Images SET Deleted=1 WHERE Id<{img.Id} AND RowId={img.RowId} AND ImageType={(int)ImageType.Customer} AND Deleted=0");
        //return Ok("Yükleme başarılı");
    }
}

public class ImageModel : IImage
{
    public string? ImageId { get; set; }
    public byte[]? ImageData { get; set; }
}