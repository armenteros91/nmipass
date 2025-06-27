using ThreeTP.Payment.Application.Interfaces.Maskhelpers;

namespace ThreeTP.Payment.Application.Helpers.Mask;

public class MaskAllStrategy:IMaskStrategy
{
    public string Mask(string input)
    {
        return string.IsNullOrEmpty(input) ? string.Empty : new string('*', input.Length);
    }
}