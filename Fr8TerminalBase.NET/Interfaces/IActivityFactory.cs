using StructureMap;

namespace Fr8.TerminalBase.Interfaces
{
    public interface IActivityFactory
    {
        IActivity Create(IContainer container);
    }
}
