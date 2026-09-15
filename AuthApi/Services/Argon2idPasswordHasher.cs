using System;
using System.Text;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace AuthApi.Services
{
    public class Argon2Options
    {
        public int MemoryKb { get; set; } = 65536;
        public int Iterations { get; set; } = 3;
        public int Parallelism { get; set; } = 4;
    }

    public class Argon2idPasswordHasher : IPasswordHasher
    {
        private readonly Argon2Options _options;

        public Argon2idPasswordHasher(IOptions<Argon2Options> options)
        {
            _options = options.Value;
        }

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = _options.Parallelism,
                Iterations = _options.Iterations,
                MemorySize = _options.MemoryKb
            };

            var hash = argon2.GetBytes(32);
            return $"$argon2id$v=19$m={_options.MemoryKb},t={_options.Iterations},p={_options.Parallelism}$" +
                   $"{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split('$');
            if (parts.Length != 6) return false;

            var paramsPart = parts[3].Split(',');
            var m = int.Parse(paramsPart[0].Split('=')[1]);
            var t = int.Parse(paramsPart[1].Split('=')[1]);
            var p = int.Parse(paramsPart[2].Split('=')[1]);
            var salt = Convert.FromBase64String(parts[4]);
            var originalHash = Convert.FromBase64String(parts[5]);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = p,
                Iterations = t,
                MemorySize = m
            };

            var newHash = argon2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
        }
    }
}
