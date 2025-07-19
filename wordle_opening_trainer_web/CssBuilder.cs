namespace wordle_opening_trainer_web
{
    public class CssBuilder
    {
        private readonly HashSet<string> _classes = new(StringComparer.OrdinalIgnoreCase);

        public CssBuilder Add(string @class, bool when = true)
        {
            if (when && !string.IsNullOrWhiteSpace(@class))
            {
                _classes.Add(@class);
            }

            return this;
        }

        public CssBuilder Remove(string @class)
        {
            if (!string.IsNullOrWhiteSpace(@class))
            {
                _classes.Remove(@class);
            }

            return this;
        }

        public CssBuilder Clear()
        {
            _classes.Clear();
            return this;
        }

        public override string ToString() => string.Join(" ", _classes);
    }
}
