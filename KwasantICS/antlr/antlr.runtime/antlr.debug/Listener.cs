namespace KwasantICS.antlr.antlr.runtime.antlr.debug
{
    public interface Listener
	{
		void  doneParsing	(object source, TraceEventArgs e);
		void  refresh		();
	}
}