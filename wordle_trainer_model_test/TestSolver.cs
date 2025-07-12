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
    public class TestSolver
    {
        [TestMethod]
        public void TestTrimmingOfSpace()
        {
            TestTrimmingInternal("plate", "22222", ["plate", "horse"], ["plate"]);
            TestTrimmingInternal("horse", "22222", ["plate", "horse"], ["horse"]);
            TestTrimmingInternal("plath", "22220", ["plats", "plate", "horse"], ["plate", "plats"]);
            TestTrimmingInternal("plate", "00100", ["xxaxx", "xxaax", "xxxaa", "xxxxa", "xxxax"], ["xxxaa", "xxxxa", "xxxax"]);
        }

        [TestMethod]
        public void TestGetEntropy()
        {
            TestGetEntropyInternal("plate", ["xxxxx", "yyyyy", "latep", "atepl"], 1.0f);
        }

        [TestMethod]
        public void TestDivideSolutionSpace()
        {
            TestDivideSolutionSpaceInternal("plate", ["xxxxx", "yyyyy", "plate", "petal", "latep"], [1, 1, 1, 2]);
            TestDivideSolutionSpaceInternal("plate", ["plate", "plate", "plate", "plate", "plate"], [5]);
        }

        private void TestGetEntropyInternal(string guess, string[] starting_solution_space, double expected_entropy)
        {
            double entropy = Solver.ComputeEntropy(guess, new List<string>(starting_solution_space));
            Assert.AreEqual(expected_entropy, entropy);
        }

        private void TestDivideSolutionSpaceInternal(string guess, string[] starting_solution_space, int[] expected_counts)
        {
            List<string> starting_solution_space_list = starting_solution_space.ToList();
            List<List<string>> actual_counts = Solver.DivideSolutionSpace(guess, starting_solution_space_list).OrderBy(x=>x.Count).ToList();
            Assert.AreEqual(expected_counts.Length, actual_counts.Count);
            for (int i = 0; i < actual_counts.Count; i++)
            {
                Assert.AreEqual(expected_counts[i], actual_counts[i].Count);
            }
        }

        private void TestTrimmingInternal(string guess, string feedback, string[] starting_solution_space, string[] trimmed_items)
        {
            List<string> starting_solution_space_list = starting_solution_space.ToList();
            WordFeedback wf = new WordFeedback(guess, feedback);
            List<string> new_space = Solver.TrimSolutionSpaceForAnswer(wf, starting_solution_space_list);
            Assert.AreEqual(trimmed_items.Length, new_space.Count);
            foreach (string item in trimmed_items)
            {
                Assert.IsTrue(new_space.Contains(item));
            }

        }

    }
}
