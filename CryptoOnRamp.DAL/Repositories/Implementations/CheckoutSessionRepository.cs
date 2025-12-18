using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

public class CheckoutSessionRepository : EntityFrameworkRepository<CheckoutSessionDb, ApplicationContext>, ICheckoutSessionRepository
{
    public CheckoutSessionRepository(ApplicationContext context)
       : base(context)
    {
    }
}
