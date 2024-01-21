namespace CodingGame.Contest.FallChallenge.src.view
{
    public class AnimationData 
    {
        private const int SHORT = 100;
        private const int QUICK = 300;
        private const int LONG = 500;

        public const int STIR_DURATION = LONG;
        public const int RESET_DURATION = QUICK;
        public const int SHELF_TO_POT_DURATION = LONG;
        public const int POT_TO_SHELF_DURATION = SHELF_TO_POT_DURATION;
        public const int SPLASH_DURATION = LONG;
        public const int SHELF_TO_POT_SEPERATION = SHORT;
        public const int POT_TO_SHELF_SEPERATION = SHELF_TO_POT_SEPERATION;
        public const int SHELF_TO_TOME_DURATION = LONG;
        public const int SHELF_TO_TOME_SEPARATION = SHORT;
        public const int TOME_TO_SHELF_DURATION = LONG;
        public const int TOME_TO_SHELF_SEPARATION = SHORT;
        public const int TOME_TO_LEARNT_DURATION = LONG;
        public const int NEW_SPELL_DURATION = LONG;
        public const int NEW_SPELL_SEPARATION = SHORT;
        public const int POTION_SPAWN_DURATION = LONG;
        public const int POTION_TO_DELIVERY_DURATION = LONG;
        public const int DELIVERY_FADE_DURATION = LONG;

        int _start, _end;
        int? _trigger;
        int? _triggerEnd;

        public AnimationData(int start, int duration) 
        {
            _start = start;
            _end = start + duration;
            _trigger = null;
            _triggerEnd = null;
        }

        public AnimationData(int start, int duration, int? triggerAfter, int? triggerDuration) 
        {
            _start = start;
            _end = start + duration;
            _trigger = triggerAfter == null ? null : start + triggerAfter;
            _triggerEnd = triggerDuration == null ? null : start + triggerAfter + triggerDuration;
        }
        
        internal int Start { get => _start; set => _start = value; }
        internal int End { get => _end; set => _end = value; }
        internal int? Trigger { get => _trigger; set => _trigger = value; }
        internal int? TriggerEnd { get => _triggerEnd; set => _triggerEnd = value; }
    }
}
