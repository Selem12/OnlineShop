using System;
using System.Collections.Generic;

namespace AuthApi.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class User : BaseEntity
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool EmailVerified { get; set; } = false;
        public bool PhoneVerified { get; set; } = false;
        public string? EmailVerificationTokenHash { get; set; }
        public DateTime? EmailTokenExpiresAt { get; set; }
        public string? PhoneVerificationTokenHash { get; set; }
        public DateTime? PhoneTokenExpiresAt { get; set; }
        
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }

    public class Session : BaseEntity
    {
        public string Id { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        
        public User User { get; set; } = null!;
    }
}
