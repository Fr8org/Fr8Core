namespace KwasantICS.DDay.iCal.Compatibility
{

#if NETCF
    [AttributeUsageAttribute(AttributeTargets.All)]
    public sealed class BrowsableAttribute : Attribute
    {
        public BrowsableAttribute(bool value)
        {
        }
    }
#endif

}