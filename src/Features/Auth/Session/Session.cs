using System.ComponentModel.DataAnnotations;
using Bellhop.Features.Identity;

namespace Bellhop.Features.Auth.Session;

public class Session
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    public string SessionToken { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
