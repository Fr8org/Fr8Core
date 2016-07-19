using StructureMap;

namespace Fr8.TerminalBase.Interfaces
{
    /// <summary>
    /// Factory that is generating activity instances for the certain activity template.
    /// See https://github.com/Fr8org/Fr8Core/blob/dev/Docs/ForDevelopers/SDK/.NET/Reference/IActivityFactory.md
    /// </summary>
    public interface IActivityFactory
    {
        IActivity Create(IContainer container);
    }
}
