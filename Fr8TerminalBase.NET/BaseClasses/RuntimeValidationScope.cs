using System;
using fr8.Infrastructure.Data.Crates;
using Fr8.TerminalBase.Infrastructure;

namespace Fr8.TerminalBase.BaseClasses
{
    partial class BaseTerminalActivity
    {
        // Do not use anywere except classes directly dervied from BaseTerminalActivity
        // Subject to be removed after going opensoruce 
        public class RuntimeValidationScope : IDisposable
        {
            private readonly Fr8.TerminalBase.BaseClasses.BaseTerminalActivity _that;

            public readonly ValidationManager ValidationManager;

            public bool HasErrors => ValidationManager.HasErrors;

            public RuntimeValidationScope(Fr8.TerminalBase.BaseClasses.BaseTerminalActivity that, ICrateStorage payloadStorage)
            {
                _that = that;
                ValidationManager = new ValidationManager(payloadStorage);
            }

            public void Dispose()
            {
                if (ValidationManager.HasErrors)
                {
                    _that.RaiseError("Activity validation fails: " + ValidationManager.ValidationResults);
                }
            }
        }
    }

}
