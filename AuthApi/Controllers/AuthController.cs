using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthApi.Data;
using AuthApi.Services;
using AuthApi.DTOs;
using AuthApi.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly ISessionService _sessionService;
        private readonly IVerificationService _verification;

        public AuthController(AppDbContext db, IPasswordHasher hasher, ISessionService sessionService, IVerificationService verification)
        {
            _db = db;
            _hasher = hasher;
            _sessionService = sessionService;
            _verification = verification;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new AuthResponse(false, "Email already exists"));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = _hasher.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber
            };

            var emailCode = _verification.GenerateCode();
            user.EmailVerificationTokenHash = _verification.HashCode(emailCode);
            user.EmailTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var phoneCode = _verification.GenerateCode();
                user.PhoneVerificationTokenHash = _verification.HashCode(phoneCode);
                user.PhoneTokenExpiresAt = DateTime.UtcNow.AddMinutes(10);
                await _verification.SendPhoneCodeAsync(request.PhoneNumber, phoneCode);
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await _verification.SendEmailCodeAsync(user.Email, emailCode);

            return Ok(new AuthResponse(true, "Registration successful. Please verify your email."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new AuthResponse(false, "Invalid credentials"));

            var sessionId = await _sessionService.CreateSessionAsync(user.Id, TimeSpan.FromMinutes(15), TimeSpan.FromHours(1));
            
            Response.Cookies.Append("session_id", sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            var userDto = new UserDto(user.Id, user.Email, user.PhoneNumber, user.EmailVerified, user.PhoneVerified);
            return Ok(new AuthResponse(true, "Login successful", userDto));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.EmailVerificationTokenHash == null || user.EmailTokenExpiresAt < DateTime.UtcNow)
                return BadRequest(new AuthResponse(false, "Invalid or expired token"));

            if (!_verification.VerifyCode(request.Code, user.EmailVerificationTokenHash))
                return BadRequest(new AuthResponse(false, "Incorrect code"));

            user.EmailVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailTokenExpiresAt = null;
            await _db.SaveChangesAsync();

            return Ok(new AuthResponse(true, "Email verified successfully"));
        }

        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (user == null || user.PhoneVerificationTokenHash == null || user.PhoneTokenExpiresAt < DateTime.UtcNow)
                return BadRequest(new AuthResponse(false, "Invalid or expired token"));

            if (!_verification.VerifyCode(request.Code, user.PhoneVerificationTokenHash))
                return BadRequest(new AuthResponse(false, "Incorrect code"));

            user.PhoneVerified = true;
            user.PhoneVerificationTokenHash = null;
            user.PhoneTokenExpiresAt = null;
            await _db.SaveChangesAsync();

            return Ok(new AuthResponse(true, "Phone verified successfully"));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var sessionId = Request.Cookies["session_id"];
                if (sessionId != null) await _sessionService.RevokeSessionAsync(sessionId);
            }
            Response.Cookies.Delete("session_id");
            return Ok(new AuthResponse(true, "Logged out"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            
            var user = await _db.Users.FindAsync(Guid.Parse(userId));
            if (user == null) return NotFound();

            return Ok(new AuthResponse(true, "Success", new UserDto(user.Id, user.Email, user.PhoneNumber, user.EmailVerified, user.PhoneVerified)));
        }
    }
}
