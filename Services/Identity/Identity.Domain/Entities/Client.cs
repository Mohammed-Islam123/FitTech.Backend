namespace Identity.Domain.Entities;
public class Client
{
    public string ClientId { get; set; } = null!;
    public string ClientName { get; set; } = null!; // e.g., "Web", "Android", "iOS"
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
