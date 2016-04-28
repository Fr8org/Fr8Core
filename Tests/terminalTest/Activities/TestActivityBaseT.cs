using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public abstract class TestActivityBase<T> : EnhancedTerminalActivity<T>
        where T: StandardConfigurationControlsCM
    {
        protected TestActivityBase() 
            : base(false)
        {
        }

        protected void Log(string message)
        {
            // use any logging logic you want
            //File.AppendAllText(@"C:\Work\fr8_research\log.txt", message + "\n");
        }
    }
}