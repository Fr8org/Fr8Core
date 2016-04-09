using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;

namespace UtilitiesTesting.Integration
{
    [TestFixture]
    [Category("Logger")]
    class LoggerTest
    {
        [Test]
        // actually test is fragile because depends on file name in HubWeb\Config\log4net.tests.config 
        // file name in FileAppender
        public void Must_Read_External_Configuration_And_Log_events()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var fileName = "";
            var section = ConfigurationManager.GetSection("log4net");
            
            var conf = section as XmlElement;
            var elm = conf.SelectNodes("appender/file");
            if (elm.Count > 0)
            {
                //take first fileAppender
                fileName = elm[0].Attributes["value"].Value;
            } 
            
            var number = new System.Random().Next(0, 100);
            
            // log event
            Utilities.Logging.Logger.GetLogger().Info(number);

            // assert the file with logs created and last message contatins number
            // i suppose that file for UtilitiesTesting project will be in relative path
            // look at log4net FileAppender section
            var text = System.IO.File.ReadAllText(path + "\\"+fileName);
            Assert.IsTrue(text.Contains(number.ToString()));

        }

        [Test, Ignore]
        public void Must_Log_To_PaperTrail()
        {
            //how can i check it?
        }


       
    }
}
