using System;

namespace CodingGame.Contest.FallChallenge.src.game.spell
{
    public abstract class Spell
    {
        static int INSTANCE_COUNT = 0;

        Recipe _recipe;
        int _id = INSTANCE_COUNT++;

        public int Id => _id;
        public Recipe Recipe { get => _recipe; set => _recipe = value; }

        public virtual int GetScore() => 0;

        public virtual int GetStock() => -1;

        public virtual bool IsActive() => false;

        public virtual bool IsOwner(PlayerWitch player) => false;

        public int[] GetDelta() => _recipe.Delta;


        public abstract bool IsRepeatable();

        public override String ToString()
        {
            return Convert.ToString(_id);
        }
    }
}
