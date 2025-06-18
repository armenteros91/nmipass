using System.Runtime.Serialization;

namespace ThreeTP.Payment.Domain.Exceptions
{
    [Serializable]
    public abstract class AppExceptionBase : Exception
    {
        public string? ErrorCode { get; }

        protected AppExceptionBase(string message)
            : base(message)
        {
        }

        protected AppExceptionBase(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected AppExceptionBase(string message, string? errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        protected AppExceptionBase(string message, string? errorCode, Exception? innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        protected AppExceptionBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ArgumentNullException.ThrowIfNull(info);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            base.GetObjectData(info, context);
        }
    }
}