using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using System.Diagnostics;

namespace wordle_trainer_model
{
    public enum Feedback
    {
        WRONG_LETTER = 0,
        WRONG_SPACE = 1,
        RIGHT = 2,
        UNKNOWN = 3
    }

    public record LetterFeedback(int position, char character, Feedback feedback);

    [DebuggerDisplay("{DebugString}")]
    public class WordFeedback
    {
        public WordFeedback(string guess)
        {
            string guess_text = guess.ToLowerInvariant();
            this.Letters = new List<LetterFeedback>(5);
            for (int i = 0; i < guess.Length; i++)
            {
                this.Letters.Add(new LetterFeedback(i, guess_text[i], Feedback.UNKNOWN));
            }
        }

        public WordFeedback(List<LetterFeedback> letters)
        {
            this.Letters = new();
            this.Letters.AddRange(letters);  
        }

        public WordFeedback(WordFeedback other)
        {
            this.Letters = new List<LetterFeedback>(other.Letters);
        }

        public string DebugString
        {
            get
            {
                return string.Concat(Letters.Select(x => x.character)) + " | "+FeedbackString;
            }
        }


        public IEnumerable<LetterFeedback> Find(char character)
        {
            foreach (LetterFeedback feedback in this.Letters)
            {
                if (feedback.character == character) yield return feedback;
            }
        }

        public LetterFeedback GetByPosition(int index)
        {
            return Letters.Where(x => x.position == index).First();
        }

        public void RemoveByPosition(int index)
        {
            Letters.RemoveAll(x => x.position == index);
        }

        public bool HasPosition(int index)
        {
            return Letters.Any(x => x.position == index);
        }

        public List<LetterFeedback> Letters { get; internal set; }  
        public string FeedbackString => string.Concat(Letters.OrderBy(x=>x.position).Select(x => Convert.ToInt32(x.feedback).ToString()));

        public IEnumerable<LetterFeedback> GetLettersByFeedback(Feedback feedback) => Letters.Where(x => x.feedback == feedback);

        public static WordFeedback CreateFromSolution(string solution)
        {
            List<LetterFeedback> letters = new List<LetterFeedback>();
            for (int i = 0; i < solution.Length; ++i)
            {
                LetterFeedback letter = new LetterFeedback(i, solution[i], Feedback.RIGHT);
                letters.Add(letter);
            }
            WordFeedback retVal = new WordFeedback(solution);
            retVal.Letters = letters;
            return retVal;
        }

        public static WordFeedback CreateFromGuessAndSolution(string guess, WordFeedback solution)
        {
            if (guess.Length != solution.Letters.Count) throw new InvalidOperationException();
            WordFeedback working_solution = new WordFeedback(solution);
            WordFeedback working_guess = new WordFeedback(guess);
            List<LetterFeedback> final_feedback = new List<LetterFeedback>();
            
            //build list of positions that are left in our guess
            List<int> positions = working_guess.Letters.Select(x => x.position).ToList();
            //find all perfect matches and remove them from both working objects
            foreach (int position in positions)
            {
                LetterFeedback guess_letter = working_guess.Letters.First(x => x.position == position);
                LetterFeedback solution_letter = working_solution.Letters.First(x=>x.position == position);
                if(solution_letter.character == guess_letter.character)
                {
                    //its a match, so add feedback and remove from both lists as they were matched
                    final_feedback.Add(new LetterFeedback(position, solution_letter.character, Feedback.RIGHT));
                    working_solution.RemoveByPosition(position);
                    working_guess.RemoveByPosition(position);  
                }
            }
            //build list of positions that are left in our guess
            positions = working_guess.Letters.Select(x => x.position).ToList();
            //find all characters in the guess that aren't in the solution at all, and add feedback that its the wrong letter, then remove ONLY from the guess (because its not in the solution)
            foreach (int position in positions)
            {
                LetterFeedback guess_letter = working_guess.Letters.First(x => x.position == position);
                if(!working_solution.Letters.Any(x=>x.character == guess_letter.character)){
                    final_feedback.Add(new LetterFeedback(position, guess_letter.character, Feedback.WRONG_LETTER));
                    working_guess.RemoveByPosition(position);
                }
            }

            //rebuild the list of positions that are left
            positions = working_guess.Letters.Select(x => x.position).ToList();
            //foreach position left in the guess, walk through the solution, and if you find the char anywhere, then add feedback of WRONG_SPACE, then remove from both lists.
            foreach(int position in positions)
            {
                LetterFeedback guess_letter = working_guess.Letters.First(y => y.position == position);
                LetterFeedback? solution_letter = working_solution.Letters.FirstOrDefault(x => x.character == guess_letter.character, null)!;
                if(solution_letter is not null)
                {
                    //we have a match, so set feedback to wrong_space and delete from guess and solution
                    final_feedback.Add(new LetterFeedback(position, guess_letter.character, Feedback.WRONG_SPACE));
                    working_guess.RemoveByPosition(guess_letter.position);
                    working_solution.RemoveByPosition(solution_letter.position);
                } else
                {
                    //no match, so set feedback to WRONG_LETTER and remove from guess.
                    final_feedback.Add(new LetterFeedback(position, guess_letter.character, Feedback.WRONG_LETTER));
                    working_guess.RemoveByPosition(position);
                }
            }
            System.Diagnostics.Debug.Assert(final_feedback.Count == guess.Length);
            return new WordFeedback(final_feedback);
        }

        public bool GuessCompliesWithFeedback(string guess, IEnumerable<char>? other_disallowed_chars = null)
        {
            if (Letters.Any(x => x.feedback == Feedback.UNKNOWN))
                throw new NotImplementedException();

            WordFeedback working_guess = new WordFeedback(guess);
            Dictionary<char, int> wrong_space_characters = new();
            HashSet<char> wrong_characters = new();

            // Process correct spots, removing them from characters that can be used in future steps
            foreach (LetterFeedback letter in this.GetLettersByFeedback(Feedback.RIGHT))
            {
                if (!working_guess.HasPosition(letter.position)) return false;
                if (working_guess.GetByPosition(letter.position).character != letter.character) return false;
                working_guess.RemoveByPosition(letter.position);
            }

            //make sure there are no chars in positions that say wrong_space for that char at that position
            // then count how many of that char is in the feedback as WRONG_SPACE, so we can make sure the answer has at least that many in the wrong position
            foreach (LetterFeedback letter in this.GetLettersByFeedback(Feedback.WRONG_SPACE))
            {
                if (working_guess.HasPosition(letter.position) && working_guess.GetByPosition(letter.position).character == letter.character) return false;
                wrong_space_characters[letter.character] = this.Letters.Where(x => x.character == letter.character && x.feedback == Feedback.WRONG_SPACE).Count();
            }

            //check that we have at least as many wrong_space chars in guess as we have in the map, removing each char as we find it.
            foreach (char character in wrong_space_characters.Keys)
            {
                for (int i = 0; i < wrong_space_characters[character]; ++i)
                {
                    LetterFeedback letter_to_remove = working_guess.Letters.FirstOrDefault(x => x.character == character)!;
                    if (letter_to_remove == null) return false;
                    working_guess.Letters.Remove(letter_to_remove);
                }
            }

            foreach(LetterFeedback letter in this.GetLettersByFeedback(Feedback.WRONG_LETTER)){
                if(working_guess.Letters.Any(x=>x.character == letter.character)) return false;
            }

            if(other_disallowed_chars is not null)
            {
                foreach(char character in other_disallowed_chars)
                {
                    if(working_guess.Letters.Any(x=>x.character == character)) return false;
                }
            }

            return true;
        }
    }

    public class State
    {
        private LetterFeedback[] _lastGuess = new LetterFeedback[5];
        private HashSet<char> _wrongLetters = new HashSet<char>();

        private State()
        { Reset(); }

        public void Reset()
        {
            _wrongLetters.Clear();
            for (int i = 0; i < 5; i++)
            {
                _lastGuess[i] = new LetterFeedback(i, ' ', Feedback.WRONG_LETTER);
            }
        }
    }
}
;