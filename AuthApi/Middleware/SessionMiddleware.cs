using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AuthApi.Services;

namespace AuthApi.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISessionService _sessionService;

        public SessionMiddleware(RequestDelegate next, ISessionService sessionService)
        {
            _next = next;
            _sessionService = sessionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sessionId = context.Request.Cookies["session_id"];
            if (!string.IsNullOrEmpty(sessionId))
            {
                var session = await _sessionService.GetSessionAsync(sessionId);
                if (session != null)
                {
                    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()) };
                    var identity = new ClaimsIdentity(claims, "Session");
                    context.User = new System.Security.Claims.ClaimsPrincipal(identity);
                    
                    // Update last accessed for sliding expiration
                    await _sessionService.ValidateAndRefreshAsync(sessionId, TimeSpan.FromMinutes(15));
                }
            }
            await _next(context);
        }
    }
}
