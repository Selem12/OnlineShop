using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Models;

namespace AuthApi.Services
{
    public class InMemorySessionService : ISessionService
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = new();

        public Task<string> CreateSessionAsync(Guid userId, TimeSpan idleTimeout, TimeSpan absoluteTimeout)
        {
            var sessionId = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var session = new Session
            {
                Id = sessionId,
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.Add(absoluteTimeout),
                LastAccessedAt = DateTime.UtcNow
            };

            _sessions[sessionId] = session;
            return Task.FromResult(sessionId);
        }

        public Task<Session?> GetSessionAsync(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (session.ExpiresAt > DateTime.UtcNow)
                {
                    return Task.FromResult<Session?>(session);
                }
                _sessions.TryRemove(sessionId, out _);
            }
            return Task.FromResult<Session?>(null);
        }

        public Task<bool> ValidateAndRefreshAsync(string sessionId, TimeSpan idleTimeout)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (session.ExpiresAt > DateTime.UtcNow && 
                    (session.LastAccessedAt == null || session.LastAccessedAt.Value.Add(idleTimeout) > DateTime.UtcNow))
                {
                    session.LastAccessedAt = DateTime.UtcNow;
                    return Task.FromResult(true);
                }
                _sessions.TryRemove(sessionId, out _);
            }
            return Task.FromResult(false);
        }

        public Task RevokeSessionAsync(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
            return Task.CompletedTask;
        }

        public Task RevokeAllUserSessionsAsync(Guid userId)
        {
            var userSessions = _sessions.Where(s => s.Value.UserId == userId).ToList();
            foreach (var session in userSessions)
            {
                _sessions.TryRemove(session.Key, out _);
            }
            return Task.CompletedTask;
        }
    }
}
