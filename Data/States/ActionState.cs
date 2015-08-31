namespace Data.States
{
    public class ActionState
    {
        public const int Unstarted = 1;
        public const int Inprocess = 2;
        public const int Completed = 3;
        public const int Error = 4;

        public static int MapActionState(string status)
        {
            switch (status)
            {
                case "Unstarted":
                    return Unstarted;
                case "Inprocess":
                    return Inprocess;
                case "Completed":
                    return Completed;
                case "Error":
                    return Error;
                default:
                    return 0;
            }
        }
    }
}
