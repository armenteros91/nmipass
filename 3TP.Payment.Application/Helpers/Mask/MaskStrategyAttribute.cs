namespace ThreeTP.Payment.Application.Helpers.Mask;

public class MaskStrategyAttribute :Attribute
{
    public Type StrategyType { get; }

    public MaskStrategyAttribute(Type strategyType)
    {
        StrategyType = strategyType;
    }
}