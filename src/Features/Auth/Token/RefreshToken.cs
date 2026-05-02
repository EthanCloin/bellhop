using System.ComponentModel.DataAnnotations;
using Bellhop.Features.Identity;

namespace Bellhop.Features.Auth.Token;

public class RefreshToken
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
