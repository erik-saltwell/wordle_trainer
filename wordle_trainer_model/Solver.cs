using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wordle_trainer_model
{
    public static class Solver
    {
        public static List<string> TrimSolutionSpaceForAnswer(WordFeedback previous_guess, List<string> previous_solution_space)
        {
            List<string> result = new List<string>();   
            foreach(string guess in previous_solution_space)
            {
                if(previous_guess.GuessCompliesWithFeedback(guess)) 
                    result.Add(guess);
            }

            return result;
        }

        public static string GetOptimalResponse(WordFeedback feedback, List<string> solution_space)
        {
            string result = string.Empty;
            double best_entropy = 0.0f;
            foreach (string guess in solution_space)
            {
                double entropy = ComputeEntropy(guess, solution_space);
                if (entropy > best_entropy)
                {
                    best_entropy = entropy;
                    result = guess;
                }
            }
            return result;
        }

        internal static double ComputeEntropy(string guess, List<string> solution_space) {
            List<List<string>> divided_solution_space = DivideSolutionSpace(guess, solution_space);
            double entropy = ClaudeShannonHelper.ComputeShannonEntropy(solution_space, divided_solution_space);
            return entropy;
        }

        internal static List<List<string>> DivideSolutionSpace(string solution, List<string> old_solution_space)
        {
            WordFeedback solution_feedback = WordFeedback.CreateFromSolution(solution);
            Dictionary<string, List<string>> divided_solution_space = new();
            foreach (string guess in old_solution_space)
            {
                WordFeedback wf = WordFeedback.CreateFromGuessAndSolution(guess, solution_feedback);
                string feedback_string = wf.FeedbackString;
                if (!divided_solution_space.ContainsKey(feedback_string))
                {
                    divided_solution_space.Add(feedback_string, new List<string>());
                }
                divided_solution_space[feedback_string].Add(guess);
            }
            List<List<string>> result = new();
            result.AddRange(divided_solution_space.Values);
            return result;
        }
    }
}
