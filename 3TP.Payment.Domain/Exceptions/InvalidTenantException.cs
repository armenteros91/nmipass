using System.Runtime.Serialization;

namespace ThreeTP.Payment.Domain.Exceptions
{
    [Serializable]
    public class InvalidTenantException : AppExceptionBase
    {
        public string FieldName { get; }
        public string ErrorMessage { get; }

        public InvalidTenantException(string fieldName, string errorMessage)
            : base($"Validation failed for field '{fieldName}': {errorMessage}")
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        }

        public InvalidTenantException(string fieldName, string errorMessage, Exception? innerException)
            : base($"Validation failed for field '{fieldName}': {errorMessage}", innerException)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        }

        protected InvalidTenantException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FieldName = info.GetString(nameof(FieldName)) ?? string.Empty;
            ErrorMessage = info.GetString(nameof(ErrorMessage)) ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);
            info.AddValue(nameof(FieldName), FieldName);
            info.AddValue(nameof(ErrorMessage), ErrorMessage);
            base.GetObjectData(info, context);
        }
    }
}