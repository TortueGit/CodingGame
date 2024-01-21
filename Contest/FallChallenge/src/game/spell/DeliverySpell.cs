namespace CodingGame.Contest.FallChallenge.src.game.spell
{
    public class DeliverySpell : Spell
    {
        private int _score;

        public DeliverySpell(Recipe need, int score)
        {
            this.Recipe = new Recipe(-need.Delta[0], -need.Delta[1], -need.Delta[2], -need.Delta[3]);
            _score = score;
        }

        public override int GetScore()
        {
            return _score;
        }

        public override bool IsRepeatable()
        {
            return false;
        }
    }
}
