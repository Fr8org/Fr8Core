using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using NUnit.Framework;
using Utilities.Logging;
using UtilitiesTesting;

namespace HubTests.Utilities
{
    [TestFixture]
    class Fr8Log4NetLayoutTests : BaseTest
    {
        public Fr8Log4NetLayout GetLayout()
        {
            var appender =
                LogManager.GetRepository()
                    .GetAppenders()
                    .Single(apndr => apndr.Name.Equals("fr8CustomLayoutFileAppender")) as FileAppender;

            var layout = appender.Layout as Fr8Log4NetLayout;
            return layout;
        }


        [Test]
        public void Should_contatin_and_read_colors_from_xml_config_file()
        {
            var layout = GetLayout();

            Assert.IsNotNull(layout);
            Assert.IsTrue(typeof (Fr8Log4NetLayout).Equals(layout.GetType()));

            Assert.IsNotNullOrEmpty(layout.InfoColor);
            Assert.IsNotNullOrEmpty(layout.WarnColor);
            Assert.IsNotNullOrEmpty(layout.ErrorColor);
        }

        [Test]
        public void Should_add_loglevel_and_bonded_to_it_colorization_to_messages()
        {
            var layout = GetLayout();

            var path = AppDomain.CurrentDomain.BaseDirectory;

            var random = new System.Random().Next(1000);

            LogManager.GetLogger("test").Info(random);
            LogManager.GetLogger("test").Warn(random);
            LogManager.GetLogger("test").Error(random, new Exception("Test exception"));

            var logfileContent = System.IO.File.ReadAllText(path + @"\log-file-log4net.txt");

            Assert.IsTrue(logfileContent.Contains(random.ToString()));
            Assert.IsTrue(logfileContent.Contains(layout.InfoColor));
            Assert.IsTrue(logfileContent.Contains(layout.WarnColor));
            Assert.IsTrue(logfileContent.Contains(layout.ErrorColor));
        }
    }
}
