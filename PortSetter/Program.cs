using slnStartupProjectLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PortSetter
{
    class Program
    {
        static string solutionFolderName = "fr8company";
        static string solutionPath = "";
        static string solutionFileName = "Dockyard.sln";
        static string configFilePath = @"\config\applicationhost.config";
        static bool isVS2015 = false;
        static bool configureMultiStartUp = false;
        static XDocument configIIS = null;
        static void Main(string[] args)
        {
            Console.WriteLine("Would you like to configure multiple startup? y/n");
            configureMultiStartUp = Console.ReadKey(true).Key == ConsoleKey.Y;
            ConfigurePorts();
            ConfigureStartup();
            Console.ReadKey();
            return;
        }

        private static void ConfigureStartup()
        {
            throw new NotImplementedException();
        }

        static void ConfigurePorts()
        {
            //get solution folder
            solutionPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var folderPath = Directory.GetParent(solutionPath);
            while (folderPath.Name != solutionFolderName)
                folderPath = folderPath.Parent;
            solutionPath = folderPath.FullName;
            //find config file is in the solution folder or in Documents
            try
            {
                isVS2015 = File.Exists(folderPath.FullName + @"\.vs" + configFilePath);
                if (isVS2015)
                    configFilePath = folderPath.FullName + @"\.vs" + configFilePath;
                else
                    configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString() + "\\Documents\\IISExpress" + configFilePath;
                configIIS = XDocument.Load(configFilePath);
            }
            catch
            {
                Console.WriteLine("Couldn't find IIS config file");
                return;
            }

            //get terminals
            var all_terminals = folderPath.GetDirectories("terminal*");
            foreach (var term in all_terminals)
            {
                string userFile = term.FullName + @"\" + term.Name + ".csproj.user";
                string webConfig = term.FullName + "\\Web.config";
                if (File.Exists(userFile) && File.Exists(webConfig))
                {
                    try
                    {
                        string result = ChangePort(term.Name, userFile, webConfig);
                        Console.WriteLine(term.Name.PadRight(40) + result);
                    }
                    catch { Console.WriteLine("Couldn't configure " + term.Name); }
                }
                else Console.WriteLine("skipped " + term.Name);
            }

            configIIS.Save(configFilePath);
        }

        static string ChangePort(string terminalname, string userFile, string webConfig)
        {
            XDocument xUser = XDocument.Load(userFile);
            XDocument xWeb = XDocument.Load(webConfig);

            //get correct port from Web.config / appSettings / TerminalEndpoint
            var terminalEndpoint = xWeb.Descendants("appSettings").FirstOrDefault().Descendants().Where(a => a.Attribute("key").Value == "TerminalEndpoint");
            var port_value = terminalEndpoint.Attributes("value").FirstOrDefault().Value;
            string port = port_value.Substring(port_value.IndexOf(":") + 1);


            //set port in IIS virtual directory
            var sites = configIIS.Descendants("sites").Descendants("site");
            var iis_node = sites.Where(a => a.Attribute("name").Value == terminalname).FirstOrDefault();
            var binding_node = iis_node.Descendants("bindings").FirstOrDefault().Descendants().FirstOrDefault();
            binding_node.Attribute("bindingInformation").Value = String.Format(@"*:{0}:localhost", port);

            //set port in .csproj.user
            var node = xUser.Descendants(@"{http://schemas.microsoft.com/developer/msbuild/2003}IISUrl").FirstOrDefault();
            string old_port = node.Value.Substring(node.Value.LastIndexOf(':') + 1).Trim('/');
            node.Value = string.Format(@"http://localhost:{0}/", port);

            xUser.Save(userFile);

            return String.Format("{0} => {1}", old_port, port);
        }
    }
}
