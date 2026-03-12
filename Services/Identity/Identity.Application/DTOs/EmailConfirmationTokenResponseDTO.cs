namespace Identity.Application.DTOs ;
public class EmailConfirmationTokenResponseDTO
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
}
