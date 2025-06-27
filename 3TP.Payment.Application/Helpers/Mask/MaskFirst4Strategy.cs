using ThreeTP.Payment.Application.Interfaces.Maskhelpers;

namespace ThreeTP.Payment.Application.Helpers.Mask;

public class MaskFirst4Strategy:IMaskStrategy
{
    public string Mask(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Length <= 4 ? new string('*', input.Length) : input[..4] + new string('*', input.Length - 4);
    }
}