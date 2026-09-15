using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims;
using AuthApi.Data;

namespace AuthApi.Policies
{
    public class VerifiedEmailRequirement : IAuthorizationRequirement { }

    public class VerifiedEmailHandler : AuthorizationHandler<VerifiedEmailRequirement>
    {
        private readonly AppDbContext _db;
        public VerifiedEmailHandler(AppDbContext db) => _db = db;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifiedEmailRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var user = await _db.Users.FindAsync(userId);
                if (user != null && user.EmailVerified)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
