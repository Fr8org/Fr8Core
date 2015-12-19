using Data.Entities;

namespace Data.Repositories
{
    internal class AuthorizationTokenChangeTracker
    {
        private string _originalData;
        private readonly AuthorizationTokenDO _tokenInstance;

        public bool HasChanges
        {
            get { return _originalData != _tokenInstance.Token; }
        }

        public AuthorizationTokenDO ActualValue
        {
            get
            {
                return _tokenInstance;
            }
        }

        public AuthorizationTokenChangeTracker(string original, AuthorizationTokenDO tokenInstance)
        {
            _originalData = original;
            _tokenInstance = tokenInstance;
        }

        public void ResetChanges()
        {
            _originalData = ActualValue.Token;
        }
    }
}