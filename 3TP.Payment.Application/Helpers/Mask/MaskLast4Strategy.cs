using ThreeTP.Payment.Application.Interfaces.Maskhelpers;

namespace ThreeTP.Payment.Application.Helpers.Mask;

public class MaskLast4Strategy:IMaskStrategy
{
    public string Mask(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return new string('*', Math.Max(0, input.Length - 4)) + input[^4..];
    }
}