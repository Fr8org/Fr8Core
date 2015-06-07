using Data.Interfaces;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    public interface ICalDAVClientFactory
    {
        ICalDAVClient Create(IRemoteCalendarAuthDataDO authData);
    }
}
