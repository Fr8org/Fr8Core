using Data.Entities;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public QuestionDO TestQuestion1()
        {
            var curQuestionDO = new QuestionDO

            {
                Id = 1,
                NegotiationId=1,
                Text = "Where should the meeting take place?",
                AnswerType = "text"
            };
            var answer = TestAnswer1();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion2()
        {
            var curQuestionDO = new QuestionDO
            {
                Id = 0,
                NegotiationId = 1,
                Text = "Where should we go now?",
                AnswerType = "text"
            };
            var answer = TestAnswer1();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion3()
        {
            var curQuestionDO = new QuestionDO
            {
                Id = 2,
                NegotiationId = 1,
                Text = "Where should we go now?",
                AnswerType = "text"
            };
            var answer = TestAnswer2();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }


        public QuestionDO TestQuestion4()
        {
            var curQuestionDO = new QuestionDO

            {
                Id = 1,
                Text = "Where should the meeting take place?",
                NegotiationId = 1,
                Negotiation = TestNegotiation5(),
                AnswerType = "text"
            };
            var answer = TestAnswer1();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion5()
        {
            var curQuestionDO = new QuestionDO

            {
                Id = 1,
                Text = "Where should we go now?",
                NegotiationId = 1,
                Negotiation = TestNegotiation5(),
                AnswerType = "text"
            };
            var answer = TestAnswer1();
            answer.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion6()
        {
            var curQuestionDO = new QuestionDO

            {
                Id = 1,
                Text = "Where should the meeting take place?",
                NegotiationId = 1,
                Negotiation = TestNegotiation5(),
                AnswerType = "text"
            };
            var answer1 = TestAnswer1();
            var answer2 = TestAnswer3();
            answer1.Question = curQuestionDO;
            answer2.Question = curQuestionDO;
            curQuestionDO.Answers.Add(answer1);
            curQuestionDO.Answers.Add(answer2);
            return curQuestionDO;
        }

        public QuestionDO TestQuestion7()
        {
            var curQuestionDO = new QuestionDO

            {
                Id = 1,
                NegotiationId = 1,
                Negotiation = TestNegotiation5(),
                Text = "Where should the meeting take place?",
                AnswerType = "text"
            };
            return curQuestionDO;
        }
        

    }
}
