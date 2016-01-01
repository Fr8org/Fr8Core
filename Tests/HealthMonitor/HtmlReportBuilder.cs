using System.Linq;
using System.Text;

namespace HealthMonitor
{
    public class HtmlReportBuilder
    {
        private const string WrapperHtmlTemplate =
            @"<html>
                <head>
                    <style type=""text/css"">
                        table {{
                            border-collapse: collapse;
                        }}

                        table th, table td {{
                            border: 1px solid #727272;
                            padding: 3px;
                        }}

                        table thead tr {{
                            background-color: white;
                        }}

                        table tbody tr.odd {{
                            background-color: white;
                        }}

                        table tbody tr.even {{
                            background-color: #cacaca;
                        }}
                    </style>
                </head>
                <body>
                    <div style=""margin: 10px 0 10px 0"">
                        Tests passed: {0} / {1}, Application: {2}
                    </div>
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
                            {3}
                        </tbody>
                    </table>
                </body>
            </html>";

        private const string ItemHtmlTemplate =
            @"<tr class=""{0}"">
                <td>{1}</td>
                <td>{2}sec</td>
                <td>{3}</td>
                <td>{4}</td>
                <td>{5}</td>
            </tr>";


        public string CreateWrapper(string appName, int success, int total, string content)
        {
            return string.Format(WrapperHtmlTemplate, success, total, appName, content);
        }

        public string CreateTestReportItemPart(TestReportItem item, int index)
        {
            return string.Format(
                ItemHtmlTemplate,
                index % 2 == 0 ? "even" : "odd",
                item.Name,
                item.Time,
                item.Success ? "Yes" : "No",
                item.Message,
                item.StackTrace
            );
        }

        public string BuildReport(string appName, TestReport report)
        {
            var sb = new StringBuilder();

            var n = 0;
            foreach (var test in report.Tests)
            {
                sb.Append(CreateTestReportItemPart(test, n));
                ++n;
            }

            var fullContent = CreateWrapper(
                appName,
                report.Tests.Count(x => x.Success),
                report.Tests.Count(),
                sb.ToString()
            );

            return fullContent;
        }
    }
}
