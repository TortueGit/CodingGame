using System.Linq;

namespace CodingGame.Contest.FallChallenge.src.game.spell
{
    public class TomeSpell : Spell
    {
        public int _stock;
        private bool _repeatable;

        public TomeSpell(Recipe recipe)
        {
            _stock = 0;
            this.Recipe = recipe;

            _repeatable = recipe.Delta.Any(x => x < 0);
        }

        public override int GetStock()
        {
            return _stock;
        }

        public override bool IsRepeatable()
        {
            return _repeatable;
        }
    }
}
