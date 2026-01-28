using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using Microsoft.AspNetCore.SignalR;
using Pawlio.Hubs;

namespace Pawlio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ApiController
    {
        public ImageController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet("{uid}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Get(string uid)
        {
            var image = await _context.Images.FirstOrDefaultAsync(x => x.Id == uid);
            if (image == null) return NotFound();

            var fileName = IImage.getFileName(image);
            if (fileName == null || !System.IO.File.Exists(fileName))
                return NotFound();

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            string contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            var stream = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return File(stream, contentType, enableRangeProcessing: false);
        }

        [HttpGet("{uid}/{w}x{h}")]
        public async Task<ActionResult> Get(string uid, int w, int h)
        {
            //var user = this.GetUser();
            var image = await _context.Images.FirstOrDefaultAsync(x => x.Id == uid);
            if (image == null) return NotFound();

            var fileName = IImage.getFileName(image);
            if (fileName == null) return Problem("Resim türü geçerli değil!");
            if (!System.IO.File.Exists(fileName)) return NotFound();

            var dir = Path.GetDirectoryName(fileName) + $"-{w}x{h}";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var thumbFile = dir += "/" + Path.GetFileName(fileName);
            thumbFile = Path.ChangeExtension(thumbFile, ".png");

            if (!System.IO.File.Exists(thumbFile))
            {
                var img = await SixLabors.ImageSharp.Image.LoadAsync(fileName);
                img.Mutate(x => x.Resize(w, h));
                using (var stream = new FileStream(thumbFile, FileMode.Create))
                {
                    img.Save(stream, new PngEncoder());
                    img.Dispose();
                    stream.Close();
                }
            }

            var file = System.IO.File.ReadAllBytes(thumbFile);
            return File(file, "image/png");
        }

        //[HttpGet("video/{uid}")]
        //public async Task<ActionResult> GetVideo(string uid)
        //{
        //    var fileName = IImage.getVideoFileName(uid);
        //    if (!System.IO.File.Exists(fileName)) return NotFound();
        //    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? contentType);
        //    var file = System.IO.File.ReadAllBytes(fileName);
        //    return File(file, contentType!);
        //}
    }
}