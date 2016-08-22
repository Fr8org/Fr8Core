namespace Data.States
{
    // If you add, change or remove items, please do the same in OperationalState.ts 
    // in order to keep front-end in sync with back-end.
    public class OperationalState
    {
        public const int Undiscovered = 0;
        public const int Active = 1;
        public const int Inactive = 2;
    }
}
