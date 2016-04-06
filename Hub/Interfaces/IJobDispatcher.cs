using System;
using System.Linq.Expressions;

namespace Hub.Interfaces
{
    public interface IJobDispatcher
    {
        void Enqueue(Expression<Action> job);
    }
}