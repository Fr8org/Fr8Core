using System.Text;

namespace HealthMonitor
{
    public class HtmlReportBuilder
    {
        private const string WrapperHtmlTemplate =
            @"<html>
                <body>
                    <table style=""width:100%"">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Time</th>
                                <th>Success?</th>
                                <th>Exception</th>
                                <th>Stack trace</th>
                            </tr>
                        </thead>
                        <tbody>
                            {0}
                        </tbody>
                    </table>
                </body>
            </html>";

        private const string ItemHtmlTemplate =
            @"<tr>
                <td>{0}</td>
                <td>{1}sec</td>
                <td>{2}</td>
                <td>{3}</td>
                <td>{4}</td>
            </tr>";


        public string CreateWrapper(string content)
        {
            return string.Format(WrapperHtmlTemplate, content);
        }

        public string CreateTestReportItemPart(TestReportItem item)
        {
            return string.Format(
                ItemHtmlTemplate,
                item.Name,
                item.Time,
                item.Success ? "Yes" : "No",
                item.Message,
                item.StackTrace
            );
        }

        public string BuildReport(TestReport report)
        {
            var sb = new StringBuilder();

            foreach (var test in report.Tests)
            {
                sb.Append(CreateTestReportItemPart(test));
            }

            var fullContent = CreateWrapper(sb.ToString());

            return fullContent;
        }
    }
}
