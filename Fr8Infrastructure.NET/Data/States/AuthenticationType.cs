namespace Fr8.Infrastructure.Data.States
{
    public class AuthenticationType
    {
        public const int None = 1;
        public const int Internal = 2;
        public const int External = 3;
        public const int InternalWithDomain = 4;
        public const int PhoneNumberWithCode = 5;
    }
}
