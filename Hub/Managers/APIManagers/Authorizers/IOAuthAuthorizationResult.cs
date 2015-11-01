namespace Hub.Managers.APIManagers.Authorizers
{
    public interface IOAuthAuthorizationResult
    {
        bool IsAuthorized { get; }
        string RedirectUri { get; }
    }
}