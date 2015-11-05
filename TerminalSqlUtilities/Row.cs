using System.Collections.Generic;

namespace TerminalSqlUtilities
{
    public class Row
    {
        public Row(IEnumerable<FieldValue> values)
        {
            _values = values;
        }

        public IEnumerable<FieldValue> Values { get { return _values; } }

        
        private readonly IEnumerable<FieldValue> _values;
    }
}
