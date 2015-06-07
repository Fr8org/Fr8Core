namespace Data.DDay.antlr.antlr.runtime.antlr.debug
{
    public interface SemanticPredicateListener : Listener
	{
		void  semanticPredicateEvaluated(object source, SemanticPredicateEventArgs e);
	}
}