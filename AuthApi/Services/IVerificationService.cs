using System;
using System.Threading.Tasks;

namespace AuthApi.Services
{
    public interface IVerificationService
    {
        string GenerateCode();
        string HashCode(string code);
        bool VerifyCode(string code, string hash);
        Task SendEmailCodeAsync(string email, string code);
        Task SendPhoneCodeAsync(string phoneNumber, string code);
    }
}
