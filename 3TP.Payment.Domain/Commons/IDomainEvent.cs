namespace ThreeTP.Payment.Domain.Commons
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}