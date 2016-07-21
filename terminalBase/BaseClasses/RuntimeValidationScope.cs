using System;
using Fr8Data.Crates;
using TerminalBase.Infrastructure;

namespace TerminalBase.BaseClasses
{
    partial class BaseTerminalActivity
    {
        // Do not use anywere except classes directly dervied from BaseTerminalActivity
        // Subject to be removed after going opensoruce 
        public class RuntimeValidationScope : IDisposable
        {
            private readonly BaseTerminalActivity _that;

            public readonly ValidationManager ValidationManager;

            public bool HasErrors => ValidationManager.HasErrors;

            public RuntimeValidationScope(BaseTerminalActivity that, ICrateStorage payloadStorage)
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
