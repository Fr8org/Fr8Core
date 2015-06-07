using System.ServiceProcess;

namespace MTAService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
                {
                    new MTA()
                };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
