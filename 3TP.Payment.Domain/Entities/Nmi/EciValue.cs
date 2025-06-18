namespace ThreeTP.Payment.Infrastructure.Persistence.Entities.Nmi;

/// <summary>
/// Verified 	            Attempted 	No          3DS (rarely shown)
/// Mastercard and Maestro 	02 	        01 	         00
/// Other brands 	        05 	        06 	         07
/// </summary>
public class EciValue
{
    /// <summary>
    /// 
    /// </summary>
    public Guid Id { get; set; }

    public string Brand { get; set; }

    /// <summary>
    /// 
    /// </summary>

    public string Label { get; set; } // Verified, Attempted, No 3DS

    /// <summary>
    /// 
    /// </summary>
    public int Value { get; set; }
}