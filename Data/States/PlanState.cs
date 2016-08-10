namespace Data.States
{
    public class PlanState
    {
        public const int Inactive = 1;
        public const int Executing = 2;
        public const int Deleted = 3;
        public const int Active = 4;
        
        public static int StringToInt(string str)
        {
            int planState = 1;

            switch (str)
            {
                case "Inactive":
                    planState = Inactive;
                    break;

                case "Executing":
                    planState = Executing;
                    break;

                case "Active":
                    planState = Active;
                    break;

                case "Deleted":
                    planState = Deleted;
                    break;
            }

            return planState;
        }

        public static string IntToString(int planState)
        {
            string state = "Inactive";

            switch (planState)
            {
                case Inactive:
                    state = "Inactive";
                    break;

                case Active:
                    state = "Active";
                    break;

                case Deleted:
                    state = "Deleted";
                    break;

                case Executing:
                    state = "Executing";
                    break;
            }

            return state;
        }
    }
}