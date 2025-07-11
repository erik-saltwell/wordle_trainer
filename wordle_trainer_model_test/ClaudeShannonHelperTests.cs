using wordle_trainer_model;

namespace wordle_trainer_model_test
{
    [TestClass]
    public class ClaudeShannonHelperTests
    {
        [TestMethod]
        public void TestUncertainty()
        {
            Assert.AreEqual(1.0, ClaudeShannonHelper.ComputeUncertainty(TwoStr));
            Assert.AreEqual(2.0, ClaudeShannonHelper.ComputeUncertainty(FourStr));
            double x = ClaudeShannonHelper.ComputeUncertainty(SixStr);
            Assert.IsTrue(x > 2.0);
            Assert.IsTrue(x < 3.0);
        }

        [TestMethod]
        public void TestInformationGained()
        {
            Assert.AreEqual(1, ClaudeShannonHelper.ComputyGainedInformation(FourStr, TwoStr));
            Assert.AreEqual(2, ClaudeShannonHelper.ComputyGainedInformation(EightStr, TwoStr));
        }

        [TestMethod]
        public void TestLikelihood()
        {
            Assert.AreEqual(0.5, ClaudeShannonHelper.ComputeLikelihood(SixStr, ThreeStr));
            Assert.AreEqual(0.125, ClaudeShannonHelper.ComputeLikelihood(EightStr, OneStr));
        }


        public List<string> OneStr => RepeatString("string", 1);
        public List<string> TwoStr => RepeatString("string", 2);
        public List<string> ThreeStr => RepeatString("string", 3);
        public List<string> FourStr => RepeatString("string", 4);
        public List<string> FiveStr => RepeatString("string", 5);
        public List<string> SixStr => RepeatString("string", 6);
        public List<string> SevenStr => RepeatString("string", 7);
        public List<string> EightStr => RepeatString("string", 8);
        public List<string> NineStr => RepeatString("string", 9);

        public static List<string> RepeatString(string value, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be non-negative.");

            return Enumerable.Repeat(value, count).ToList();
        }
    }
}