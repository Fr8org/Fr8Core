
using System;
using System.IO;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static string TestRealPdfFile1()
        {
            return Path.Combine(Environment.CurrentDirectory, "Tools\\FileTools\\TestFiles", "small_pdf_file.pdf");
        }
    }
}
