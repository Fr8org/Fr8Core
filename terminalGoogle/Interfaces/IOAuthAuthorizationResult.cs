namespace terminalGoogle.Interfaces
{
    public interface IOAuthAuthorizationResult
    {
        bool IsAuthorized { get; }
        string RedirectUri { get; }
    }
}