namespace TerminalSqlUtilities
{
    public interface ISqlQueryBuilder
    {
        SqlQuery BuildSelectQuery(SelectQuery query);
    }
}
