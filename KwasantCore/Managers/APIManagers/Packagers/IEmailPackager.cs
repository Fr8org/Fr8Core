using Data.Entities;

namespace KwasantCore.Managers.APIManagers.Packagers
{
    public interface IEmailPackager
    {
        void Send(EnvelopeDO envelope);
    }
}