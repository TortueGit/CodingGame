namespace CodingGame.Contest.FallChallenge.src.view
{
    public class SpellData 
    {
        int _id;
        int[] _delta;
        bool _repeatable;
        int? _score;

        private SpellData(int id, int[] delta, bool? repeatable, int? score)
        {
            _id = id;
            _delta = delta;
            _repeatable = repeatable.Value;
            _score = score;
        }

        public SpellData(int id, int[] delta, bool repeatable) 
        {
            new SpellData(id, delta, repeatable, null);
        }

        public SpellData(int id, int[] delta, int score) 
        {
            new SpellData(id, delta, null, score);
        }

        public int Id { get => _id; set => _id = value; }
        public int[] Delta { get => _delta; set => _delta = value; }
        public bool Repeatable { get => _repeatable; set => _repeatable = value; }
        public int? Score { get => _score; set => _score = value; }
    }
}
