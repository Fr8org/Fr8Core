using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace terminalUtilities
{
    public class TabularUtilities
    {
        public const string ExtractedFieldsCrateLabel = "ExtractedFields";

        /// <summary>
        /// If table contains only one row, extract it into a list of FieldDTO to save user 
        /// from adding the Extract Table Field action for such a common case. The function 
        /// assumes that fields in headersArray and rows follow in the same order. If they 
        /// don't, the function will not produce correct results. 
        /// </summary>
        /// <returns>Returns null if the input table contains 0 or more than 1 rows and a Crate 
        /// with the extracted fields if the table contains exactly one row.</returns>
        /// <param name="isFirstRowAsColumnNames">Whether the first row in rows contains headers.</param>
        /// <param name="isRunTime">If true, StandardPayloadDataCM rather than FieldDescriptionsCM (if false).</param>
        /// <param name="headersArray">An array with table headers.</param>
        /// <param name="rows">Table rows as a list of TableRowDTO.</param>
        public static Crate PrepareFieldsForOneRowTable(bool isFirstRowAsColumnNames, List<TableRowDTO> rows, IList<string> headersArray = null)
        {
            if (!isFirstRowAsColumnNames && (headersArray == null || headersArray.Count == 0))
            {
                throw new ArgumentException("headerArray parameter needs to be supplied if isFirstRowAsColumnNames is false.");
            }

            // Create individual fields if only one row
            if (rows.Count == (isFirstRowAsColumnNames ? 2 : 1))
            {
                string headerName;
                TableCellDTO cell;
                var row = rows[(isFirstRowAsColumnNames ? 1 : 0)];
                List<KeyValueDTO> fields = new List<KeyValueDTO>();
                for (var i = 0; i < row.Row.Count; i++)
                {
                    cell = row.Row[i];
                    headerName = isFirstRowAsColumnNames ? rows[0].Row[i].Cell.Value : headersArray[i];
                    if (!string.IsNullOrEmpty(cell.Cell.Value))
                    {
                        fields.Add(new KeyValueDTO("Value immediately below of " + headerName, cell.Cell.Value));
                    }
                }
                return Crate.FromContent(ExtractedFieldsCrateLabel, new StandardPayloadDataCM()
                {
                    PayloadObjects = new List<PayloadObjectDTO>()
                                {
                                    new PayloadObjectDTO(fields)
                                }
                });
            }

            return null;
        }
    }
}
