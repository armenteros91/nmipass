using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ThreeTP.Payment.Infrastructure.Services.Encryption;

namespace ThreeTP.Payment.Infrastructure.Tests.Services.Encryption
{
    public class AesEncryptionServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AesEncryptionService>> _mockLogger;
        private readonly AesEncryptionService _service;

        public AesEncryptionServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AesEncryptionService>>();

            // Mock configuration values for Key and IV
            _mockConfiguration.SetupGet(x => x["Encryption:Key"]).Returns("TestKey123456789012345678901234");
            _mockConfiguration.SetupGet(x => x["Encryption:IV"]).Returns("TestIV1234567890");

            _service = new AesEncryptionService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public void Encrypt_Decrypt_ShouldReturnOriginalText()
        {
            // Arrange
            var originalText = "This is a secret message.";

            // Act
            var encryptedText = _service.Encrypt(originalText);
            var decryptedText = _service.Decrypt(encryptedText);

            // Assert
            Assert.Equal(originalText, decryptedText);
        }

        [Fact]
        public void ValidateEncryption_WithValidEncryption_ShouldReturnTrue()
        {
            // Arrange
            var plainText = "This is a test string.";
            var encryptedText = _service.Encrypt(plainText);

            // Act
            var isValid = _service.ValidateEncryption(plainText, encryptedText);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateEncryption_WithInvalidEncryption_ShouldReturnFalse()
        {
            // Arrange
            var plainText = "This is a test string.";
            var incorrectEncryptedText = "ThisIsDefinitelyNotTheCorrectEncryption";

            // Act
            var isValid = _service.ValidateEncryption(plainText, incorrectEncryptedText);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateEncryption_WithEmptyPlainText_ShouldReturnTrueForEncryptedEmptyString()
        {
            // Arrange
            var plainText = "";
            var encryptedText = _service.Encrypt(plainText);

            // Act
            var isValid = _service.ValidateEncryption(plainText, encryptedText);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateEncryption_WithDifferentPlainText_ShouldReturnFalse()
        {
            // Arrange
            var plainText1 = "This is a test string.";
            var plainText2 = "This is a different test string.";
            var encryptedText1 = _service.Encrypt(plainText1);

            // Act
            var isValid = _service.ValidateEncryption(plainText2, encryptedText1);

            // Assert
            Assert.False(isValid);
        }
    }
}
