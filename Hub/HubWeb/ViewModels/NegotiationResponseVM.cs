using System;
using Data.States;

namespace Web.ViewModels
{
	public class NegotiationResponseVM : NegotiationVM
	{
        public CommunicationMode CommunicationMode { get; set; }
        public String OriginatingUser { get; set; }
	}

    public class NegotiationResponseQuestionVM : NegotiationQuestionVM
    {
    }

    public class NegotiationResponseAnswerVM : NegotiationAnswerVM
    {
        public bool UserAnswer { get; set; }
    }
}