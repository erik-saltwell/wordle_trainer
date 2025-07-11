using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wordle_trainer_model;

namespace wordle_trainer_model_test
{
    [TestClass]
    public class FeebackTests
    {

        [TestMethod]
        public  void TestAllRight()
        {
            TestGuess("abcde", "22222", "abcde", true);
            TestGuess("abcde", "22222", "abcdf", false);
            TestGuess("abcde", "22222", "abcda", false);
        }

        [TestMethod]
        public void TestWrongLetter()
        {
            TestGuess("abcde", "22220", "abcdf", true);
            TestGuess("abcde", "22220", "abcde", false);
        }

        [TestMethod]
        public void TestWrongSpace()
        {
            TestGuess("abcde", "22211", "abcde", false);
            TestGuess("abcde", "22211", "abcae", false);
            TestGuess("abcde", "22211", "abcad", false);
            TestGuess("abcde", "22211", "abced", true);
        }

        [TestMethod]
        public void TestMultipleWrongSpace()
        {
            TestGuess("xyyza", "01100", "lllyy", true);
            TestGuess("xyyza", "01100", "yllyy", true);
            TestGuess("xyyza", "01100", "lllyz", false);
            TestGuess("xyyza", "01100", "ylyly", false);
            TestGuess("xyyza", "01100", "lllly", false);
            TestGuess("xyyzz", "01000", "lllyy", false);
        }

        [TestMethod]
        public void TestFeedbackString()
        {
            TestFeedbackStringInternal("00000");
            TestFeedbackStringInternal("11111");
            TestFeedbackStringInternal("22222");
            TestFeedbackStringInternal("01201");
            TestFeedbackStringInternal("00110");
            TestFeedbackStringInternal("00210");
            TestFeedbackStringInternal("21212");
            TestFeedbackStringInternal("01202");
        }

        [TestMethod]
        public void TestOtherDisallowedCharacters()
        {
            WordFeedback f = Create("abcde", "02210");
            Assert.IsTrue(f.GuessCompliesWithFeedback("xbckd"));
            Assert.IsFalse(f.GuessCompliesWithFeedback("xbckd", "px"));
        }

        private void TestFeedbackStringInternal(string feedback)
        {
            WordFeedback f = Create("abcde", feedback);
            string output = f.FeedbackString;
            Assert.AreEqual(feedback, output);
        }

        private void TestGuess(string old_guess, string feedback, string guess, bool complies)
        {
            WordFeedback f = Create(old_guess, feedback);
            Assert.AreEqual(complies, f.GuessCompliesWithFeedback(guess));
        }

        private static WordFeedback Create(string guess, string feedback)
        {
            if (guess.Length != feedback.Length)
                throw new InvalidOperationException();
            WordFeedback retVal = new WordFeedback(guess);
            for (int i = 0; i < feedback.Length; i++)
            {
                Feedback f = Parse(feedback[i]);
                retVal.Letters[i] = new LetterFeedback(i, guess[i], f);
            }
            return retVal;
        }

        private static Feedback Parse(char character)
        {
            switch (character)
            {
                case '0':
                    return Feedback.WRONG_LETTER;
                case '1':
                    return Feedback.WRONG_SPACE;
                case '2':
                    return Feedback.RIGHT;
                case '3':
                    return Feedback.UNKNOWN;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
