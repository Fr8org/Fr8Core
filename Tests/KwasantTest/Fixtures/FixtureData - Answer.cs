using Data.Entities;

namespace KwasantTest.Fixtures
{
    public partial class FixtureData
    {
        public AnswerDO TestAnswer1()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 1,
                AnswerStatus=2,
                QuestionID = 1,
                EventID = 1,
                Event = TestEvent5(),
                Text = "Starbucks on Valencia",
            };
            return curAnswerDO;
        }

        public AnswerDO TestAnswer2()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 2,
                AnswerStatus = 2,
                QuestionID=2,
                EventID = 1,
                Event = TestEvent5(),
                Text = "Starbucks on Valencia 2",
            };
            return curAnswerDO;
        }

        public AnswerDO TestAnswer3()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 0,
                AnswerStatus = 2,
                QuestionID = 1,
                EventID = 1,
                Event = TestEvent5(),
                Text = "Starbucks on Valencia 2",
            };
            return curAnswerDO;
        }

        public AnswerDO TestAnswer4()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 1,
                AnswerStatus = 2,
                QuestionID = 1,
                EventID = 1,
                Event = TestEvent5(),
                Question=TestQuestion7(),
                Text = "Starbucks on Valencia",
            };
            return curAnswerDO;
        }

        public AnswerDO TestAnswer5()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 1,
                AnswerStatus = 2,
                QuestionID = 1,
                EventID=1,
                Event = TestEvent5(),
                Question = TestQuestion7(),
                Text = "Starbucks on Valencia 2",
            };
            return curAnswerDO;
        }
                
        public AnswerDO TestAnswer6()
        {
            var curAnswerDO = new AnswerDO

            {
                Id = 0,
                AnswerStatus = 2,
                QuestionID = 1,
                Text = "Starbucks on Valencia",
            };
            return curAnswerDO;
        }
                
    }
}
