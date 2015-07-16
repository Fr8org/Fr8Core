using Data.Entities;
using Data.Interfaces;
using KwasantCore.Security;

namespace KwasantTest.Security
{
    public class MockedSecurityServices : ISecurityServices
    {
        private readonly object _locker = new object();
        private UserDO _currentLoggedInUser;
        public void Login(IUnitOfWork uow, UserDO userDO)
        {
            lock (_locker)
                _currentLoggedInUser = userDO;
        }

        public UserDO GetCurrentUser()
        {
            lock (_locker)
                return _currentLoggedInUser;
        }
    }
}
