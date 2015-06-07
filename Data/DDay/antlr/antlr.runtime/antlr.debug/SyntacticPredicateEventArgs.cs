namespace Data.DDay.antlr.antlr.runtime.antlr.debug
{
    public class SyntacticPredicateEventArgs : GuessingEventArgs
	{
		
		
		public SyntacticPredicateEventArgs()
		{
		}
		public SyntacticPredicateEventArgs(int type) : base(type)
		{
		}

		public override string ToString()
		{
			return "SyntacticPredicateEvent [" + Guessing + "]";
		}
	}
}