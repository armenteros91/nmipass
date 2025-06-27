namespace ThreeTP.Payment.Domain.Commons
{
    public abstract class BaseEntity
    {
       public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }
        public byte[] TimeStamp { get; private set; } = Array.Empty<byte>();
    }
}