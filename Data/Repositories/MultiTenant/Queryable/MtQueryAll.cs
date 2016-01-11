using Data.Interfaces.Manifests;

namespace Data.Repositories
{
    public class MtQueryAll<T> : MtQueryable<T>
        where T : Manifest
    {
        public MtQueryAll(IMtQueryExecutor<T> executor)
            : base(null, executor)
        {
        }
    }
}