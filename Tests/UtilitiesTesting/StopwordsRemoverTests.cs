using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace UtilitiesTesting
{
    [TestFixture]
    public class StopwordsRemoverTests
    {
        private const string TheCocaColaCompanyIsNice = "The Coca-cola company is nice";

        [Test]
        public void RemoveStopwords_WhenEmptyStringIsPassed_ReturnsEmptyString()
        {
            Assert.AreSame(string.Empty, StopwordsRemover.RemoveStopwords(string.Empty), "Empty string passed was not returned back");
        }

        [Test]
        public void RemoveStopwords_WhenNoStopwordsArePassed_ReturnsSameString()
        {
            Assert.AreSame(TheCocaColaCompanyIsNice, StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice), "String passed was not returned back");
        }

        [Test]
        public void RemoveStopwords_WhenStopwordsIsAPartOfTheWord_DoesntRemoveIt()
        {
            Assert.AreEqual(TheCocaColaCompanyIsNice, StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "com"), "Stopword soundn't be removed");
        }

        [Test]
        public void RemoveStopwords_WhenStopwordIsFollowedBySeparator_RemovesItWithSeparator()
        {
            Assert.AreEqual("Coca-cola company is nice", StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "The"), "Stopword should be removed with separator");
        }

        [Test]
        public void RemoveStopwords_WhenStopwordIfPrecededBySeparator_RemovesItWithSeparator()
        {
            Assert.AreEqual("The Coca-cola company is", StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "nice"), "Stopword should be removed with separator");
        }

        [Test]
        public void RemoveStopwords_WhenStopwordIsPrecededAndFollowedBySeparatorsOfTheSameType_RemovesItWithOnlyOneOfThem()
        {
            Assert.AreEqual("The Coca-cola is nice", StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "company"), "Stopword should be removed with only one separator");
        }

        [Test]
        public void RemoveStopwords_WhenStopwordIsPreceededAndFollowedBySeparatorsOfDifferentType_RemovesItWithNonWhitespaceSeparator()
        {
            Assert.AreEqual("The cola company is nice", StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "Coca"), "Stopword should be removed with non-whitespace separator");
        }

        [Test]
        public void RemoveStopwords_Always_IsCaseInsensitive()
        {
            Assert.AreEqual("The Coca-cola is nice", StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "COMPANY"), "Stopwords remover should be case-insensitive");
        }

        [Test]
        public void RemoveStopwords_WhenNoStopwordIsFound_ReturnsTheSameString()
        {
            Assert.AreSame(TheCocaColaCompanyIsNice, StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "NotAWord", "Nothing should be removed"));
        }

        [Test]
        public void RemoveStopwords_Always_RemovesAllStopwords()
        {
            Assert.AreEqual("The cola is nice", StopwordsRemover.RemoveStopwords(TheCocaColaCompanyIsNice, "Coca", "company"), "All stopwords should be removed");
        }
    }
}
