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
        public void TestGuessString()
        {
            WordFeedback word = Create("abcde", "22222");
            Assert.AreEqual("abcde", word.GuessString);

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
            List<LetterFeedback> letters = new List<LetterFeedback>();
            letters.Add(new LetterFeedback(2, 'c', Feedback.RIGHT));
            letters.Add(new LetterFeedback(0, 'a', Feedback.WRONG_SPACE));
            letters.Add(new LetterFeedback(1, 'b', Feedback.WRONG_SPACE));
            letters.Add(new LetterFeedback(3, 'd', Feedback.WRONG_SPACE));
            letters.Add(new LetterFeedback(4, 'e', Feedback.WRONG_SPACE));
            WordFeedback f = new WordFeedback(letters);
            string feedback_string = f.FeedbackString;
            Assert.AreEqual("11211", feedback_string);
        }

        [TestMethod]
        public void TestOtherDisallowedCharacters()
        {
            WordFeedback f = Create("abcde", "02210");
            Assert.IsTrue(f.GuessCompliesWithFeedback("xbckd"));
            Assert.IsFalse(f.GuessCompliesWithFeedback("xbckd", "px"));
        }

        [TestMethod]
        public void TestCreateFeedbackFromGuessAndSolution()
        {
            TestCreateFeedbackInternal("place", "place", "22222");
            TestCreateFeedbackInternal("place", "zzzzz", "00000");
            TestCreateFeedbackInternal("place", "ecalp", "11211");
            TestCreateFeedbackInternal("plate", "spitp", "01020");

            TestCreateFeedbackInternal("xyyza", "lllyy", "00011");
            TestCreateFeedbackInternal("xyyza", "yllyy", "10010");
            TestCreateFeedbackInternal("xyyza", "lllyz", "00011");
            TestCreateFeedbackInternal("xyyza", "ylyly", "10200");
            TestCreateFeedbackInternal("xyyza", "lllly", "00001");
            TestCreateFeedbackInternal("xyyzz", "lllyy", "00011");
        }

        [TestMethod]
        public void TestCreateSolution()
        {
            TestSolutionInternal("abcde");
            TestSolutionInternal("aaaaa");
        }

        [TestMethod]
        public void TestCreateFromGuessAndFeedback()
        {
            TestCreateFromGessAndFeedbackInternal("abcde", "00000");
            TestCreateFromGessAndFeedbackInternal("ecabd", "01210");
        }

        private void TestCreateFromGessAndFeedbackInternal( string guess, string feedback)
        {
            WordFeedback expected = Create(guess, feedback);
            WordFeedback actual = new WordFeedback(guess, feedback);
            Assert.AreEqual(expected.Letters.Count, actual.Letters.Count);
            for (int i = 0; i < expected.Letters.Count; i++)
            {
                Assert.AreEqual(expected.Letters[i].position, actual.Letters[i].position);
                Assert.AreEqual(expected.Letters[i].character, actual.Letters[i].character);
                Assert.AreEqual(expected.Letters[i].feedback, actual.Letters[i].feedback);
            }
        }

        private void TestSolutionInternal(string solution)
        {
            WordFeedback f = WordFeedback.CreateFromSolution(solution);
            Assert.AreEqual(solution.Length, f.Letters.Count);
            for (int i = 0; i < f.Letters.Count; i++)
            {
                Assert.AreEqual(f.Letters[i].character, solution[i]);
                Assert.AreEqual(Feedback.RIGHT, f.Letters[i].feedback);
                Assert.AreEqual(i, f.Letters[i].position);
            }
        }

        private void TestCreateFeedbackInternal(string solution_text , string guess_text, string expected_feedback_string)
        {
            WordFeedback solution = WordFeedback.CreateFromSolution(solution_text);
            WordFeedback temp = WordFeedback.CreateFromGuessAndSolution(guess_text, solution);
            string feedback_string = temp.FeedbackString;
            Assert.AreEqual(expected_feedback_string, feedback_string);
        }

        private void TestFeedbackStringInternal(string feedback)
        {
            List<Feedback> list = new List<Feedback>();
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
