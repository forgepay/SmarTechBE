using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;

namespace CryptoOnRamp.DAL.Repositories.Implementations;

public class SessionRepository : EntityFrameworkRepository<SessionDb, ApplicationContext>, ISessionRepository
{
    public SessionRepository(ApplicationContext context)
       : base(context)
    {
    }
}