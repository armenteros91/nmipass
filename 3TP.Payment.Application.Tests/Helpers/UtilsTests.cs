using NUnit.Framework;
using FluentAssertions;
using System;
using ThreeTP.Payment.Application.Helpers;

namespace ThreeTP.Payment.Application.Tests.Helpers
{
    [TestFixture]
    public class UtilsTests
    {
        [Test]
        public void GenerateApiKey_ShouldReturnValidBase64StringAndCorrectLength()
        {
            // Act
            string apiKey = Utils.GenerateApiKey();

            // Assert
            apiKey.Should().NotBeNullOrEmpty();

            // Check for Base64 validity
            byte[] apiKeyBytes = null;
            Action action = () => apiKeyBytes = Convert.FromBase64String(apiKey);
            action.Should().NotThrow("because the API key should be a valid Base64 string");

            // Check byte length
            apiKeyBytes.Should().NotBeNull();
            apiKeyBytes.Length.Should().Be(32, "because the API key should be 32 bytes long when decoded");
        }
    }
}
