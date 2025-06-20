using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;

namespace ThreeTP.Payment.Application.Helpers;

/// <summary>
/// Utility class containing helpers for sanitizing sensitive transaction request data
/// and masking card numbers (PAN) according to PCI DSS standards.
/// 
/// These utilities ensure that sensitive data is never logged or exposed in logs or databases.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Sanitizes a transaction request object by masking sensitive fields to make it safe for logging.
    /// 
    /// - Masks the CreditCardNumber to show only the first 6 and last 4 digits.
    /// - Replaces the CVV value with '***'.
    /// - Masks the CreditCardExpiration date with '**/**'.
    /// 
    /// This ensures that logs do not contain sensitive cardholder data,
    /// complying with PCI DSS Requirements 3.3 and 3.4.
    /// </summary>
    /// <param name="request">The original transaction request containing sensitive information.</param>
    /// <returns>A sanitized deep copy of the transaction request, safe for logging or auditing purposes.</returns>
    public static BaseTransactionRequestDto SanitizeRequestForLogging(BaseTransactionRequestDto request)
    {
        // Create a deep copy of the original request
        var clone = JsonSerializer.Deserialize<BaseTransactionRequestDto>(JsonSerializer.Serialize(request));

        // Mask the credit card number if it is not null or empty
        if (!string.IsNullOrEmpty(clone.CreditCardNumber))
            clone.CreditCardNumber = MaskCardNumber(clone.CreditCardNumber);

        // Never log the CVV; replace it with asterisks
        clone.Cvv = "***";

        // Mask the credit card expiration date
        clone.CreditCardExpiration = "**/**";

        return clone;
    }


    /// <summary>
    /// Masks a card number (PAN) according to PCI DSS standards by displaying
    /// only the first 6 and last 4 digits and replacing the rest with asterisks (*).
    /// 
    /// Example:
    /// Input: 4111111111111111
    /// Output: 411111******1111
    /// 
    /// If the input is invalid or too short (< 10 digits), returns '****'.
    /// </summary>
    /// <param name="cardNumber">The full credit card number to mask.</param>
    /// <returns>A masked card number, or '****' if input is invalid.</returns>
    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 10)
            return "****";

        int visibleStart = 6;
        int visibleEnd = 4;
        int maskLength = cardNumber.Length - (visibleStart + visibleEnd);

        string start = cardNumber.Substring(0, visibleStart);
        string end = cardNumber.Substring(cardNumber.Length - visibleEnd);

        string masked = start + new string('*', maskLength) + end;
        return masked;
    }


    /// <summary>
    /// Provides methods to serialize and deserialize payment query responses between XML and JSON formats.
    /// </summary>
    public static class QueryResponseSerializer
    {
        /// <summary>
        /// Deserializes an XML string into a QueryResponseDto object.
        /// </summary>
        /// <param name="xml">The XML string representing a payment query response.</param>
        /// <returns>A <see cref="QueryResponseDto"/> object containing the deserialized data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="xml"/> parameter is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the XML deserialization fails due to invalid XML or mismatched structure.</exception>
        public static QueryResponseDto DeserializeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException(nameof(xml), "XML string cannot be null or empty.");
            }

            try
            {
                var serializer = new XmlSerializer(typeof(QueryResponseDto));
                using var reader = new StringReader(xml);
                return (QueryResponseDto)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to deserialize XML", ex);
            }
        }

        /// <summary>
        /// Serializes a QueryResponseDto object into a JSON string.
        /// </summary>
        /// <param name="response">The <see cref="QueryResponseDto"/> object to serialize.</param>
        /// <returns>A JSON string representing the serialized payment query response.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="response"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when JSON serialization fails due to invalid data or configuration.</exception>
        public static string ToJson(QueryResponseDto response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response), "Response object cannot be null.");
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                return JsonSerializer.Serialize(response, options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to serialize to JSON", ex);
            }
        }
    }

    /// <summary>
    /// Provides extension methods for converting DTO objects into key-value pair representations.
    /// </summary>
    public static class DtoExtensions
    {
        /// <summary>
        /// Converts a DTO object into a dictionary where:
        /// - Keys are the property names converted to lowercase.
        /// - Values are the string representation of property values.
        /// 
        /// This is particularly useful for:
        /// - Dynamically generating form-urlencoded content for API requests.
        /// - Building generic logging or serialization utilities.
        /// 
        /// Only public, readable, non-null properties are included.
        /// </summary>
        /// <typeparam name="T">The type of the DTO object.</typeparam>
        /// <param name="dto">The DTO object to convert.</param>
        /// <returns>
        /// A dictionary of property names and values representing the DTO.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
        /// <remarks>
        /// ⚠️ Sensitive data should be masked before calling this method if it is intended for logging or external transmission.
        /// 
        /// 📚 Why as an extension method?
        /// - Placing this method in a utility class avoids embedding serialization logic in DTOs, adhering to Clean Architecture principles.
        /// - Ensures reusability across different DTO types without requiring each DTO to implement its own serialization logic.
        /// </remarks>
        public static Dictionary<string, string> ToKeyValuePairs<T>(T dto) where T : class
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "DTO object cannot be null.");
            }

            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetValue(dto) != null &&
                            !p.GetCustomAttributes<SensitiveDataAttribute>().Any())
                .ToDictionary(
                    p => p.Name.ToLower(),
                    p => p.GetValue(dto)!.ToString()!
                );
        }
    }
    
    /// <summary>
    /// Utilidad para  convertir una cadena en hash 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ComputeSHA256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes); // .NET 5+ => uppercase
    }

    /// <summary>
    /// Generates a new API key.
    /// </summary>
    /// <returns>A string representing the new API key.</returns>
    public static string GenerateApiKey()
    {
        byte[] apiKeyBytes = new byte[32];
        RandomNumberGenerator.Fill(apiKeyBytes);
        return Convert.ToBase64String(apiKeyBytes);
    }
}

/// <summary>
/// Attribute to mark DTO properties as sensitive, excluding them from key-value pair serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SensitiveDataAttribute : Attribute
{
}