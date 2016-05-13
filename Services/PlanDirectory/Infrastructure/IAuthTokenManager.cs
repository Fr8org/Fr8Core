using System;

namespace PlanDirectory.Infrastructure
{
    public interface IAuthTokenManager
    {
        string CreateToken(Guid fr8AccountId);
        Guid? GetFr8AccountId(string token);
    }
}
