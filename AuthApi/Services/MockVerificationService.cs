using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthApi.Services
{
    public class MockVerificationService : IVerificationService
    {
        public string GenerateCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        public string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(bytes);
        }

        public bool VerifyCode(string code, string hash)
        {
            return HashCode(code) == hash;
        }

        public Task SendEmailCodeAsync(string email, string code)
        {
            Console.WriteLine($"\n[MOCK EMAIL] To: {email} | Code: {code}\n");
            return Task.CompletedTask;
        }

        public Task SendPhoneCodeAsync(string phoneNumber, string code)
        {
            Console.WriteLine($"\n[MOCK SMS] To: {phoneNumber} | Code: {code}\n");
            return Task.CompletedTask;
        }
    }
}
