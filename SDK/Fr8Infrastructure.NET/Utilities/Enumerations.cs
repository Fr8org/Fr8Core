using System;
using System.Collections.Generic;

namespace Fr8.Infrastructure.Utilities
{

    //creates a base class for a string-based enumerated list. The subclasses instantiate the list with the allowable values, and this class makes sure that only those values can be used. 
    //basically just creates an enumeration that doesn't have an underlying integer definition. 
    public abstract class EnumeratedList : List<string>
    {
        string _value;
        public List<string> AllowedValues;

        public EnumeratedList()
        {
            SetAllowable(AllowedValues);
        }
        
        //configures the list of allowed strings. 
        public void SetAllowable(List<string> initial_vals)
        {
            AllowedValues = initial_vals;
        }

        internal void Set(string value)
        {
            if (!this.AllowedValues.Contains(value))
                throw new ArgumentException("You tried to set a value that is not defined for this enumerated type. Allowable values:" + AllowedValues);
            _value = value;
        }

        internal string Get()
        {
            return _value;
        }

       
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public class xCallRequestStatus : EnumeratedList
    {
        public xCallRequestStatus()
        {
            List<string> init_list = new List<string> {
                "Active",
                "Fulfilled",
                "Deactivated"};
            SetAllowable(init_list);
        }
    }

    /// <summary>
    /// /////////////////////////////////////////////////////////////////
    /// </summary>
    public class CallState : EnumeratedList
    {
        public CallState()
        {
            List<string> init_list = new List<string> {
                "Unstarted",
                "Waiting To Start",
                "Active",
                "Completed",
                "Interrupted"};
            SetAllowable(init_list);
        }
    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////
    /// </summary>
    public class UserCallState : EnumeratedList
    {
        public UserCallState()
        {
            List<string> init_list = new List<string> {
                "Participating",
                "Post-Call",
                "Finished",
                };
            SetAllowable(init_list);
        }
    }

    /////////////////////////////////////////////////////////////////////

    public class xParticipantType : EnumeratedList
    {
        public xParticipantType()
        {
            List<string> init_list = new List<string> {
                "Producer",
                "Consumer"
                };
            SetAllowable(init_list);
        }
    }

    /// <summary>
    /// HTTP method to use when making requests
    /// </summary>
    public enum Method
    {
        GET,
        POST,
        PUT,
    }

    public enum RegistrationStatus
    {
        Successful,
        Pending,
        UserMustLogIn,
    }

    public enum LoginStatus
    {
        Successful,
        InvalidCredential,
        ImplicitUser,
        UnregisteredUser,
        Pending
    }
}
  





              