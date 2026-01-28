using Pawlio.Models;

public class PhotoRequest : IImage
{
    public int Id { get; set; }

    public string? ImageId { get; set; }

    public byte[]? ImageData { get; set; }

    public string? VideoId { get; set; }

    public byte[]? VideoData { get; set; }
}