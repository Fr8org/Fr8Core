using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Crates;
using Data.Interfaces.Manifests;

namespace terminalFr8Core.Services
{
    public interface ICrateConversion
    {
        Crate Convert(Crate input);
    }
}