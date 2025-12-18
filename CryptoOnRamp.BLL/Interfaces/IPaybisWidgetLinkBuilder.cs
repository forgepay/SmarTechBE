using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IPaybisWidgetLinkBuilder
{
    Task<Uri> GetWidgetUrlAsync(PaybisWidgetPetition petition);
}
