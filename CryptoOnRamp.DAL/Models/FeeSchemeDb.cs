namespace CryptoOnRamp.DAL.Models;

public class FeeSchemeDb
{
    public int Id { get; set; }
    public FeeType Type { get; set; }
    public int? TargetUserId { get; set; } // null for Processor; userId SA/Agent for rest
    public decimal Percent { get; set; }
    public int UpdatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
