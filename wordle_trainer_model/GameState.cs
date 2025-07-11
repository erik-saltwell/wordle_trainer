namespace wordle_trainer_model
{
    internal class GameState
    {
        private LetterFeedback[] _lastGuess = new LetterFeedback[5];
        private HashSet<char> _wrongLetters = new HashSet<char>();
        private HashSet<string> _solution_space = new HashSet<string>();
        private const string _firstMove = "plate";

        public GameState()
        { Reset(); }

        public void Reset()
        {
            _wrongLetters.Clear();
            _solution_space.Clear();
            for (int i = 0; i < 5; i++)
            {
                _lastGuess[i] = new LetterFeedback(i, ' ', Feedback.WRONG_LETTER);
            }
        }
    }
}