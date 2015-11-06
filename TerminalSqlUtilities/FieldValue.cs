namespace TerminalSqlUtilities
{
    public class FieldValue
    {
        public FieldValue(string field, object value)
        {
            _field = field;
            _value = value;
        }

        public string Field { get { return _field; } }

        public object Value { get { return _value; } }


        private readonly string _field;
        private readonly object _value;
    }
}
