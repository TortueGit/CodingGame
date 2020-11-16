namespace CodingGame.Contest.FallChallenge.src.game.spell
{
    public class PlayerSpell : Spell
    {

        private bool _active;
        private bool _repeatable;
        public PlayerWitch _owner;

        private PlayerSpell(Recipe recipe, PlayerWitch owner, bool repeatable)
        {
            this.Recipe = recipe;
            _active = true;
            _owner = owner;
            _repeatable = repeatable;
        }

        public PlayerSpell(Recipe recipe, PlayerWitch owner)
        {
            new PlayerSpell(recipe, owner, false);
        }

        public PlayerSpell(TomeSpell learnt, PlayerWitch owner)
        {
            new PlayerSpell(new Recipe(learnt.Recipe), owner, learnt.IsRepeatable());
        }

        public override bool IsActive()
        {
            return _active;
        }

        public override bool IsOwner(PlayerWitch player)
        {
            return player == _owner;
        }

        public override bool IsRepeatable()
        {
            return _repeatable;
        }

        public void activate()
        {
            _active = true;
        }

        public void deactivate()
        {
            _active = false;
        }

    }
}
