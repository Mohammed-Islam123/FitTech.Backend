namespace Identity.Domain.Entities;

public class MedicalFile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;   // relative path served as public URL
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}