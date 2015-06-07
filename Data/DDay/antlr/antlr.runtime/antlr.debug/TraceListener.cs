namespace Data.DDay.antlr.antlr.runtime.antlr.debug
{
    public interface TraceListener : Listener
	{
		void  enterRule	(object source, TraceEventArgs e);
		void  exitRule	(object source, TraceEventArgs e);
	}
}