namespace Data.DDay.antlr.antlr.runtime.antlr.debug
{
    public interface ParserTokenListener : Listener
	{
		void  parserConsume	(object source, TokenEventArgs e);
		void  parserLA		(object source, TokenEventArgs e);
	}
}