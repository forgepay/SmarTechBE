namespace CryptoOnRamp.DAL.Models;

public enum FeeType
{
    Unknown = 0,
    Company = 1,
    SuperAgent = 3,
    Agent  = 4,
}

public enum PayoutType
{
    Unknown = 0,
    Company = 1,
    Admin = 2,
    SuperAgent = 3,
    Agent = 4,
}
