using System.Diagnostics;
using wordle_trainer_model;

namespace opening_builder
{
    internal class Program
    {
        private static Dictionary<WordFeedback, string> _optimal_guesses=new Dictionary<WordFeedback, string>();
        private const string _first_move = "plate";

        static void Main(string[] args)
        {
            //Test();
            ExecuteFullProgram();
        }
        private static void Test()
        {
            PossibleSolutions initial_possible_solutions = new PossibleSolutions(["xxxxx", "yyggg", "yyccc", "yybbb", "yyddd", "plate"]);
            WordFeedback first_move_result = new WordFeedback("plate", "00000");
            
            List<string> possible_solutions = Solver.TrimSolutionSpaceForAnswer(first_move_result, initial_possible_solutions.Solutions);
            
            System.Diagnostics.Trace.WriteLine("Trimmed starting solutions: "+ string.Concat(possible_solutions.Select(x=>x + " ")));
            string optimal_response = Solver.GetOptimalResponse(first_move_result, possible_solutions);
            Console.WriteLine(optimal_response);
            Console.ReadLine();
        }

        private static void ExecuteFullProgram()
        {
            PossibleSolutions initial_possible_solutions = new PossibleSolutions();
            //write results to file
            foreach (string initial_feedback_string in GenerateAllPossibleSolutions())
            {
                WordFeedback first_move_result = new WordFeedback(_first_move, initial_feedback_string);
                List<string> possible_solutions = Solver.TrimSolutionSpaceForAnswer(first_move_result, initial_possible_solutions.Solutions);
                
                if (possible_solutions.Count  == 0)
                {
                    Console.WriteLine($"{initial_feedback_string}=>EMPTY");
                } else {
                    string optimal_response = Solver.GetOptimalResponse(first_move_result, possible_solutions);
                    _optimal_guesses.Add(first_move_result, optimal_response);
                    Console.WriteLine($"{initial_feedback_string}=>{optimal_response}");
                }
            }

            //write to file
            using (System.IO.StreamWriter w = new StreamWriter("wordle_second_moves.txt"))
            {
                foreach (WordFeedback f in _optimal_guesses.Keys)
                {
                    w.WriteLine(f.FeedbackString + "\t" + _optimal_guesses[f]);
                }
            }
            Console.ReadLine();
        }

        private static IEnumerable<string> GenerateAllPossibleSolutions(int length = 5)
        {
            var chars = new[] { '0', '1', '2' };
            var total = (int)Math.Pow(chars.Length, length);

            for (int i = 0; i < total; i++)
            {
                yield return NumberToTernaryString(i, chars, length);
            }
        }

        private static string NumberToTernaryString(int value, char[] chars, int length)
        {
            var result = new char[length];
            int baseValue = chars.Length;

            for (int i = length - 1; i >= 0; i--)
            {
                result[i] = chars[value % baseValue];
                value /= baseValue;
            }

            return new string(result);
        }

    }
}
