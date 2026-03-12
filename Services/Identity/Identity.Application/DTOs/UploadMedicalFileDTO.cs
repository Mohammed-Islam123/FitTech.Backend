using Microsoft.AspNetCore.Http;

namespace Identity.Application.DTOs;


public class UploadMedicalFileDTO
{
    public Guid UserId { get; set; }
    public IFormFile File { get; set; } = null!;

}
