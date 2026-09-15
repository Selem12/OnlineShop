using System;

namespace AuthApi.DTOs
{
    public record RegisterRequest(string Email, string Password, string? PhoneNumber);
    public record LoginRequest(string Email, string Password);
    public record VerifyEmailRequest(string Email, string Code);
    public record VerifyPhoneRequest(string PhoneNumber, string Code);
    public record UserDto(Guid Id, string Email, string? PhoneNumber, bool EmailVerified, bool PhoneVerified);
    public record AuthResponse(bool Success, string Message, UserDto? User = null);
}
