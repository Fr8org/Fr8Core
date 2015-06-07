using Data.Entities;
using Data.States;
using StructureMap;
using Data.Interfaces;namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public NegotiationDO TestNegotiation1()
        {
            var curBookingRequestDO = TestBookingRequest1();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            var question = TestQuestion1();
            question.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question);
            return curNegotiationDO;
        }

        public NegotiationDO TestNegotiation2()
        {
            var curBookingRequestDO = TestBookingRequest2();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 2"
            };
            var question = TestQuestion1();
            question.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question);
            return curNegotiationDO;

        }

        public NegotiationDO TestNegotiation3()
        {
            var curBookingRequestDO = TestBookingRequest1();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            var question1 = TestQuestion1();
            var question2 = TestQuestion2();
            question1.Negotiation = curNegotiationDO;
            question2.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question1);
            curNegotiationDO.Questions.Add(question2);

            return curNegotiationDO;
        }

        public NegotiationDO TestNegotiation4()
        {
            var curBookingRequestDO = TestBookingRequest1();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            var question1 = TestQuestion1();
            var question2 = TestQuestion3();
            question1.Negotiation = curNegotiationDO;
            question2.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question1);
            curNegotiationDO.Questions.Add(question2);

            return curNegotiationDO;
        }

        public NegotiationDO TestNegotiation5()
        {
            var curBookingRequestDO = TestBookingRequest1();
            var curNegotiationDO = new NegotiationDO
            {
                Id = 1,
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
          
            return curNegotiationDO;
        }

        public NegotiationDO TestNegotiation6()
        {
            var curBookingRequestDO = TestBookingRequest1();
            var curNegotiationDO = new NegotiationDO
            {
                BookingRequest = curBookingRequestDO,
                BookingRequestID = curBookingRequestDO.Id,
                NegotiationState = NegotiationState.InProcess,
                Name = "Negotiation 1"
            };
            var question = TestQuestion3();
            question.Negotiation = curNegotiationDO;
            curNegotiationDO.Questions.Add(question);
            return curNegotiationDO;
        }
       
    }
}
