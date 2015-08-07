namespace Data.Entities
{
    public enum CriteriaExecutionMode
    {
        /// <summary>
        /// Execute criteria when conditions are met.
        /// </summary>
        WithConditions = 1,

        /// <summary>
        /// Execute regardless of conditions.
        /// </summary>
        WithoutConditions = 2
    }
}
