using System;
using System.Threading.Tasks;
using AuthApi.Models;

namespace AuthApi.Services
{
    public interface ISessionService
    {
        Task<string> CreateSessionAsync(Guid userId, TimeSpan idleTimeout, TimeSpan absoluteTimeout);
        Task<Session?> GetSessionAsync(string sessionId);
        Task<bool> ValidateAndRefreshAsync(string sessionId, TimeSpan idleTimeout);
        Task RevokeSessionAsync(string sessionId);
        Task RevokeAllUserSessionsAsync(Guid userId);
    }
}
