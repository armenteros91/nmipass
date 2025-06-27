using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ThreeTP.Payment.Application.DTOs.Requests.Pasarela;
using ThreeTP.Payment.Application.DTOs.Responses.Pasarela;
using ThreeTP.Payment.Application.Helpers.Mask;
using ThreeTP.Payment.Application.Interfaces.Maskhelpers;

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
    /// Sanitizes a <see cref="SaleTransactionRequestDto"/> object by masking sensitive fields to make it safe for logging.
    ///
    /// This method performs the following:
    /// - Masks the CreditCardNumber to show only the first 6 and last 4 digits.
    /// - Replaces the CVV value with '***'.
    /// - Masks the CreditCardExpiration date as '**/**'.
    ///
    /// This ensures that logs do not contain sensitive cardholder data,
    /// complying with PCI DSS Requirements 3.3 and 3.4.
    /// </summary>
    /// <param name="request">The original transaction request containing sensitive information.</param>
    /// <returns>A sanitized deep copy of the transaction request, safe for logging or auditing purposes.</returns>
    public static SaleTransactionRequestDto SanitizeRequestForLogging(SaleTransactionRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Deep copy using Newtonsoft.Json
        var clone = JsonConvert.DeserializeObject<SaleTransactionRequestDto>(
                        JsonConvert.SerializeObject(request))
                    ?? new SaleTransactionRequestDto();

        // Mask the credit card number
        if (!string.IsNullOrWhiteSpace(clone.CreditCardNumber))
            clone.CreditCardNumber = MaskCardNumber(clone.CreditCardNumber);

        // Mask CVV
        clone.Cvv = "***";

        // Mask expiration date
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
        /// <param name="xml">The XML string representing a payment query Response.</param>
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
        /// Serializes a QueryResponseDto object into a JSON string using Newtonsoft.Json.
        /// </summary>
        /// <param name="response">The <see cref="QueryResponseDto"/> object to serialize.</param>
        /// <returns>A JSON string representing the serialized payment query Response.</returns>
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
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                return JsonConvert.SerializeObject(response, settings);
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
        /// Convierte una cadena con formato <c>application/x-www-form-urlencoded</c> (por ejemplo, de un gateway de pagos)
        /// en una instancia del tipo <typeparamref name="T"/>. Las claves se mapean usando el atributo <see cref="JsonPropertyNameAttribute"/> 
        /// si está presente; en su defecto, se usa el nombre de la propiedad en minúsculas.
        /// </summary>
        /// <typeparam name="T">Tipo al cual se desea mapear la respuesta (debe tener constructor por defecto).</typeparam>
        /// <param name="raw">Cadena con datos en formato <c>key=value&amp;key2=value2</c>.</param>
        /// <returns>Instancia del tipo <typeparamref name="T"/> con las propiedades asignadas según los pares clave-valor de la cadena.</returns>
        /// <example>
        /// DTO de ejemplo:
        /// <code>
        /// public class NmiResponseDto
        /// {
        ///     [JsonPropertyName("response")]
        ///     public int Response { get; set; }
        ///
        ///     [JsonPropertyName("responsetext")]
        ///     public string ResponseText { get; set; }
        ///
        ///     [JsonPropertyName("response_code")]
        ///     public string ResponseCode { get; set; }
        /// }
        /// </code>
        ///
        /// Uso:
        /// <code>
        /// string raw = "response=2&amp;responsetext=DECLINE&amp;response_code=200";
        /// var dto = ParseResponse&lt;NmiResponseDto&gt;(raw);
        /// // dto.Response == 2
        /// // dto.ResponseText == "DECLINE"
        /// // dto.ResponseCode == "200"
        /// </code>
        /// </example>
        public static T ParseResponse<T>(string raw) where T : new()
        {
            var pairs = raw.Split('&')
                .Select(part => part.Split('='))
                .Where(split => split.Length == 2)
                .ToDictionary(split => split[0], split => Uri.UnescapeDataString(split[1]));

            var dto = new T();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Obtener nombre desde JsonPropertyName si existe, o usar el nombre normal en minúsculas
                var jsonAttr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                var key = jsonAttr?.Name ?? prop.Name.ToLower();

                if (pairs.TryGetValue(key, out var val))
                {
                    try
                    {
                        object? convertedValue = Convert.ChangeType(val,
                            Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                        prop.SetValue(dto, convertedValue);
                    }
                    catch
                    {
                        // Puedes loggear el fallo de conversión aquí si es necesario
                        continue;
                    }
                }
            }

            return dto;
        }


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
        /// </remarks>
        public static Dictionary<string, string> ToKeyValuePairsForNotLogginSensitiveData<T>(T dto) where T : class
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


        /// <summary>
        /// Convierte un objeto de tipo <typeparamref name="T"/> en un diccionario de pares clave-valor (`Dictionary&lt;string, string&gt;`), 
        /// utilizando como claves los nombres de las propiedades públicas o sus atributos <see cref="JsonPropertyNameAttribute"/> si están presentes.
        /// </summary>
        /// <typeparam name="T">El tipo del objeto que se desea convertir.</typeparam>
        /// <param name="obj">Instancia del objeto a convertir.</param>
        /// <param name="ignoreNulls">
        /// Si es <c>true</c>, las propiedades con valores nulos o cadenas vacías serán ignoradas. 
        /// Si es <c>false</c>, se incluirán en el resultado.
        /// </param>
        /// <returns>
        /// Un diccionario con los nombres de las propiedades como claves (respetando el atributo <see cref="JsonPropertyNameAttribute"/> si existe) 
        /// y sus valores convertidos a cadena como valores.
        /// </returns>
        /// <remarks>
        /// Este método es útil para serializar objetos de forma personalizada, por ejemplo, para enviar datos en una solicitud `application/x-www-form-urlencoded`.
        /// </remarks>
        public static Dictionary<string, string> ToKeyValuePairsProperty<T>(T obj, bool ignoreNulls = true)
        {
            var dict = new Dictionary<string, string>();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // 👇 Evitar propiedades que requieren parámetros (por ejemplo: indexers)
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                var value = prop.GetValue(obj);
                if (value == null && ignoreNulls) continue;

                // Obtener el nombre definido por JsonPropertyName, o el nombre original
                var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                var key = attr?.Name ?? prop.Name;

                // Convertir el valor a string
                var stringValue = value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(stringValue) && ignoreNulls) continue;

                dict[key] = stringValue!;
            }

            return dict;
        }


        /// <summary>
        /// Enmascara los datos sensibles en una cadena de respuesta tipo query string (clave=valor&...).
        /// </summary>
        /// <param name="input">Cadena de respuesta con formato de pares clave-valor.</param>
        /// <param name="sensitiveKeys">Lista de claves sensibles que deben ser enmascaradas.</param>
        /// <param name="maskChar">Carácter utilizado para enmascarar los valores sensibles.</param>
        /// <returns>Cadena con los valores sensibles enmascarados, manteniendo la estructura original.</returns>
        public static string MaskSensitiveData(string input, List<string>? sensitiveKeys = null, char maskChar = '*')
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            sensitiveKeys ??= new List<string> { "authcode", "transactionid", "orderid", "cvvresponse" };

            var result = input.Split('&')
                .Select(pair =>
                {
                    var kvp = pair.Split('=', 2);
                    if (kvp.Length != 2) return pair;

                    var key = kvp[0];
                    var value = kvp[1];

                    if (sensitiveKeys.Contains(key.ToLower()))
                    {
                        var masked = string.IsNullOrEmpty(value)
                            ? ""
                            : new string(maskChar, Math.Min(4, value.Length)) +
                              value.Substring(Math.Min(4, value.Length));
                        return $"{key}={masked}";
                    }

                    return $"{key}={value}";
                });

            return string.Join("&", result);
        }

        /// <summary>
        /// Convierte un objeto de tipo <typeparamref name="T"/> en un diccionario de pares clave-valor 
        /// (<see cref="Dictionary{String, String}"/>), donde los valores de las propiedades marcadas con 
        /// <see cref="SensitiveDataAttribute"/> se enmascaran automáticamente.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a convertir.</typeparam>
        /// <param name="obj">Instancia del objeto desde el cual se generarán los pares clave-valor.</param>
        /// <returns>
        /// Un diccionario con los nombres de las propiedades como claves (utilizando el atributo 
        /// <see cref="JsonPropertyNameAttribute"/> si está presente) y sus valores como cadenas. 
        /// Las propiedades sensibles estarán enmascaradas.
        /// </returns>
        /// <remarks>
        /// Este método es útil para registrar información que será enviada a servicios externos (como gateways de pago),
        /// permitiendo ocultar información sensible automáticamente sin necesidad de listas manuales de campos.
        /// El enmascaramiento por defecto oculta todos los caracteres excepto los dos últimos.
        /// </remarks>
        public static Dictionary<string, string> ToMaskedKeyValuePairs<T>(T obj)
        {
            var dict = new Dictionary<string, string>();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;

                // Obtener nombre del atributo JsonPropertyName o el nombre de la propiedad
                var attrName = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                var key = attrName?.Name ?? prop.Name;

                var stringValue = value.ToString()?.Trim();
                if (string.IsNullOrEmpty(stringValue)) continue;

                // Verificar si la propiedad es sensible
                var isSensitive = prop.GetCustomAttribute<SensitiveDataAttribute>() != null;

                if (isSensitive)
                {
                    // Enmascarar valor (mostrar últimos 2 caracteres como referencia, opcional)
                    var masked = new string('*', Math.Max(0, stringValue.Length - 2)) + stringValue[^2..];
                    dict[key] = masked;
                }
                else
                {
                    dict[key] = stringValue;
                }
            }

            return dict;
        }

        /// <summary>
        /// Convierte un objeto de tipo <typeparamref name="T"/> en un diccionario clave-valor, enmascarando las propiedades
        /// marcadas con <see cref="SensitiveDataAttribute"/> usando una estrategia de enmascaramiento personalizada.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a convertir.</typeparam>
        /// <param name="obj">Instancia del objeto a convertir.</param>
        /// <param name="maskStrategy">
        /// Función que define cómo enmascarar los valores sensibles. Si no se proporciona, se aplica el patrón por defecto:
        /// ocultar todos los caracteres excepto los dos últimos.
        /// </param>
        /// <returns>
        /// Un diccionario con los nombres de las propiedades como claves (usando <see cref="JsonPropertyNameAttribute"/> si está presente),
        /// y sus valores como cadenas. Las propiedades sensibles estarán enmascaradas según la estrategia proporcionada.
        /// </returns>
        /// <example>
        /// <code>
        /// // DTO de ejemplo con campos sensibles
        /// public class PaymentRequestDto
        /// {
        ///     [JsonPropertyName("card_number")]
        ///     [SensitiveData]
        ///     public string CardNumber { get; set; } = "4111111111111111";
        ///
        ///     [JsonPropertyName("amount")]
        ///     public string Amount { get; set; } = "100.00";
        /// }
        ///
        /// var dto = new PaymentRequestDto();
        ///
        /// // Uso con estrategia por defecto (oculta todo menos los últimos 2 caracteres)
        /// var maskedDefault = ToMaskedKeyValuePairs(dto);
        /// // Resultado: { ["card_number"] = "**************11", ["amount"] = "100.00" }
        ///
        /// // Uso con estrategia personalizada: ocultar todo el valor
        /// var maskedAll = ToMaskedKeyValuePairs(dto, val => new string('*', val.Length));
        /// // Resultado: { ["card_number"] = "****************", ["amount"] = "100.00" }
        ///
        /// // Uso con estrategia personalizada: mostrar solo los primeros 4 caracteres
        /// var maskedFirst4 = ToMaskedKeyValuePairs(dto, val =>
        ///     val.Length <= 4 ? new string('*', val.Length) : val[..4] + new string('*', val.Length - 4));
        /// // Resultado: { ["card_number"] = "4111************", ["amount"] = "100.00" }
        /// </code>
        /// </example>
        public static Dictionary<string, string> ToMaskedKeyValuePairs<T>(
            T obj,
            Func<string, string>? maskStrategy = null)
        {
            var dict = new Dictionary<string, string>();

            // Estrategia por defecto: ocultar todos menos los dos últimos caracteres
            maskStrategy ??= (val) =>
            {
                if (string.IsNullOrEmpty(val)) return string.Empty;
                return new string('*', Math.Max(0, val.Length - 2)) + val[^2..];
            };

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;

                var attrName = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                var key = attrName?.Name ?? prop.Name;

                var stringValue = value.ToString()?.Trim();
                if (string.IsNullOrEmpty(stringValue)) continue;

                var isSensitive = prop.GetCustomAttribute<SensitiveDataAttribute>() != null;

                dict[key] = isSensitive ? maskStrategy(stringValue) : stringValue;
            }

            return dict;
        }


        /// <summary>
        /// Convierte un objeto en una cadena `application/x-www-form-urlencoded`, enmascarando automáticamente
        /// los valores sensibles definidos con <see cref="SensitiveDataAttribute"/>.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a procesar.</typeparam>
        /// <param name="obj">Instancia del objeto a serializar.</param>
        /// <param name="maskStrategy">Función para aplicar el patrón de enmascaramiento.</param>
        /// <returns>Cadena con formato `key=value&key2=value2`, lista para log o auditoría.</returns>
        public static string ToMaskedFormUrlEncoded<T>(
            T obj,
            Func<string, string>? maskStrategy = null)
        {
            var dict = ToMaskedKeyValuePairs(obj, maskStrategy);

            return string.Join("&", dict.Select(kvp =>
                $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        }
    }

    public static class SensitiveDataLogger
    {
        public static Dictionary<string, string> ToMaskedKeyValuePairs<T>(T obj)
        {
            var dict = new Dictionary<string, string>();

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = prop.GetValue(obj);
                if (value == null) continue;

                var attrName = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                var key = attrName?.Name ?? prop.Name;

                var stringValue = value.ToString()?.Trim();
                if (string.IsNullOrEmpty(stringValue)) continue;

                var isSensitive = prop.GetCustomAttribute<SensitiveDataAttribute>() != null;

                if (isSensitive)
                {
                    var maskAttr = prop.GetCustomAttribute<MaskStrategyAttribute>();
                    string masked;

                    if (maskAttr != null && typeof(IMaskStrategy).IsAssignableFrom(maskAttr.StrategyType))
                    {
                        var strategy = (IMaskStrategy)Activator.CreateInstance(maskAttr.StrategyType)!;
                        masked = strategy.Mask(stringValue);
                    }
                    else
                    {
                        // Estrategia por defecto: mostrar últimos 2 caracteres
                        masked = new string('*', Math.Max(0, stringValue.Length - 2)) + stringValue[^2..];
                    }

                    dict[key] = masked;
                }
                else
                {
                    dict[key] = stringValue;
                }
            }

            return dict;
        }

        public static string ToMaskedFormUrlEncoded<T>(T obj)
        {
            var dict = ToMaskedKeyValuePairs(obj);

            return string.Join("&", dict.Select(kvp =>
                $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        }
    }


    /// <summary>
    /// Utilidad para  convertir una cadena en hash 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ComputeSha256(string input)
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