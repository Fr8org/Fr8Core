namespace KwasantICS.antlr.antlr.runtime.antlr.debug
{
    public interface ParserController : ParserListener
		{
			ParserEventSupport ParserEventSupport
			{
				set;
			}

			void  checkBreak();
		}
}