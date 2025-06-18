namespace ThreeTP.Payment.Domain.Commons
{
    public abstract class BaseEntity
    {
        //public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public byte[] TimeStamp { get; private set; } = Array.Empty<byte>();
    }
}