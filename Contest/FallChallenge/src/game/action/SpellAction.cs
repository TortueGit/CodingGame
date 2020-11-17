using System;

namespace CodingGame.Contest.FallChallenge.src.game.action
{
    public class SpellAction : Action
    {
        private int? _param;

        public SpellAction(String str, int spellId, int? param)
        {
            _str = str;
            _spellId = spellId;
            _param = param;
        }

        public override bool IsSpell() => true;

        public override int GetRepeats() => _param ?? 1;
    }
}
