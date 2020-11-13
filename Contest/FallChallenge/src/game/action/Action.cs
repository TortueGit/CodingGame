using System;

namespace CodingGame.Contest.FallChallenge.src.game.action
{
    public abstract class Action
    {
        internal int _spellId;
        internal String _str;
        
        public static readonly Action NO_ACTION = new NoAction() {            
        };

        public Action() {
            _str = "NO_ACTION";
        }

        public String Str => _str;
        public virtual bool IsSpell => false;
        public virtual bool IsReset => false;
        public virtual bool IsWait => false;
        public virtual int GetRepeats => 1;
    }

    public class NoAction : Action
    {

    }
}
