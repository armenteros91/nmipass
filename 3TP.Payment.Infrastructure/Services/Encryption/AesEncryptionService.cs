using System.Security.Cryptography;
using Amazon.SecretsManager.Model.Internal.MarshallTransformations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ThreeTP.Payment.Application.Helpers;
using ThreeTP.Payment.Application.Interfaces;

namespace ThreeTP.Payment.Infrastructure.Services.Encryption
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly ILogger<AesEncryptionService> _logger;

        public AesEncryptionService(IConfiguration config, ILogger<AesEncryptionService> logger)
        {
            _logger = logger;

            var rawKey = config["Encryption:Key"];
            var rawIv = config["Encryption:IV"];

            if (string.IsNullOrWhiteSpace(rawKey) || string.IsNullOrWhiteSpace(rawIv))
            {
                throw new ArgumentException("Encryption key or IV is missing in configuration.");
            }

            using var sha256 = SHA256.Create();

            _key = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawKey));
            _iv = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawIv))[..16]; // 16 bytes IV

            if (_key.Length != 16 && _key.Length != 24 && _key.Length != 32)
            {
                _logger.LogError("AES key must be 16, 24, or 32 bytes. Actual size: {Length}", _key.Length);
                throw new ArgumentException("Invalid AES key size.");
            }

            if (_iv.Length != 16)
            {
                _logger.LogError("AES IV must be 16 bytes. Actual size: {Length}", _iv.Length);
                throw new ArgumentException("Invalid AES IV size.");
            }

            _logger.LogInformation("AesEncryptionService initialized successfully with AES-{KeySize}-CBC", _key.Length * 8);
        }

        public string Encrypt(string plainText)
        {
            try
            {
                using var aes = Aes.Create();
                using var encryptor = aes.CreateEncryptor(_key, _iv);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption failed");
                throw;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                using var aes = Aes.Create();
                using var decryptor = aes.CreateDecryptor(_key, _iv);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption failed");
                throw;
            }
        }

        public string Hash(string input)
        {
            return Utils.ComputeSha256(input);
        }
    }
}
