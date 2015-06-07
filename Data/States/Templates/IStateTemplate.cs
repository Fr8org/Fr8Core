using System;

namespace Data.States.Templates
{
    public interface IStateTemplate
    {
        int Id { get; set; }
        String Name { get; set; }
        string ToString();
    }

    public interface IStateTemplate<T> : IStateTemplate
    {
    }
}
