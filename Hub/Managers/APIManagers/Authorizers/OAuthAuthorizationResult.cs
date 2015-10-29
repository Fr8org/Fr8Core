namespace Hub.Managers.APIManagers.Authorizers
{
    class OAuthAuthorizationResult : IOAuthAuthorizationResult
    {
        public OAuthAuthorizationResult(bool isAuthorized, string redirectUri = null)
        {
            IsAuthorized = isAuthorized;
            RedirectUri = redirectUri;
        }

        public bool IsAuthorized { get; private set; }
        public string RedirectUri { get; private set; }
    }
}