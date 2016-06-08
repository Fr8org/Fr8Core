using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;

namespace Fr8.Infrastructure.Data.Crates
{
    public interface ICrateStorage : IEnumerable<Crate>
    {
        int Count { get; }
        void Add(Crate crate);
        void Add(params Crate[] crate);
        void Clear();
        int Remove(Predicate<Crate> predicate);
        int Replace(Predicate<Crate> predicate, Crate crate);
    }
}