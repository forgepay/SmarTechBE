using System.Text.Json.Serialization;

namespace CryptoOnRamp.BLL.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OnramperTransactionStatus
{
    /// <summary>
    /// The transaction has been successfully completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The payment has been made but the transaction is not yet completed.
    /// </summary>
    Paid,

    /// <summary>
    /// The transaction is currently in progress and awaiting further action.
    /// </summary>
    Pending,

    /// <summary>
    /// A new transaction has been created but no payment has been made yet.
    /// </summary>
    New,

    /// <summary>
    /// The transaction has failed due to an error or user action.
    /// </summary>
    Failed,

    /// <summary>
    /// The transaction has been canceled by the user or the system.
    /// </summary>
    Canceled
}
