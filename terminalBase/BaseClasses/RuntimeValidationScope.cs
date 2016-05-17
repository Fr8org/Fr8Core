using System;
using Fr8Data.Crates;
using Fr8Data.Managers;
using Fr8Data.Manifests;
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
            private readonly ICrateStorage _payloadStorage;
            private readonly ValidationResultsCM _currentValidationResults;

            public readonly ValidationManager ValidationManager;

            public bool HasErrors => ValidationManager.HasErrors;

            public RuntimeValidationScope(BaseTerminalActivity that, ICrateStorage payloadStorage)
            {
                _currentValidationResults = new ValidationResultsCM();

                _that = that;
                _payloadStorage = payloadStorage;

                ValidationManager = new ValidationManager(_currentValidationResults, payloadStorage);
            }

            public void Dispose()
            {
                if (ValidationManager.HasErrors)
                {
                    _that.RaiseError("Activity validation fails: " + _currentValidationResults);
                }
            }
        }
    }

}
