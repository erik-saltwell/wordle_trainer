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

        public WordFeedback(WordFeedback other)
        {
            this.Letters = new List<LetterFeedback>(other.Letters);
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
        public string FeedbackString => string.Concat(Letters.Select(x => Convert.ToInt32(x.feedback).ToString()));

        public IEnumerable<LetterFeedback> GetLettersByFeedback(Feedback feedback) => Letters.Where(x => x.feedback == feedback);

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