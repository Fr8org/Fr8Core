using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class FileUtils
    {
        public static IList<string> LoadFileHostList()
        {
            var path = Server.ServerPhysicalPath + "fr8terminals.txt";

            IList<string> urls = null;
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    if (sr.Peek() < 0)
                    {
                        throw new ApplicationException("fr8terminals.txt is empty.");
                    }

                    urls = new List<string>();
                    
                    while (sr.Peek() >= 0)
                    {
                        var line = sr.ReadLine();
                        
                        var uri = line.StartsWith("http") ? line : "http://" + line;
                        
                        urls.Add(uri + "/terminals/discover");
                    }
                }
            }
            catch (Exception)
            {
                //TODO: Replace with incedent
                //Logger.GetLogger().ErrorFormat("Error register plugins actions: '{0}'", ex.Message);
            }
            return urls;
        }
    }
}
