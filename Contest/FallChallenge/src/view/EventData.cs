using System.Collections.Generic;

namespace CodingGame.Contest.FallChallenge.src.view
{
    public class EventData
    {
        public const int LEARN = 0;
        public const int LEARN_PAY = 6;
        public const int NEW_DELIVERIES = 1;
        public const int NEW_TOME_SPELLs = 2;
        public const int PLAYER_SPELL = 3;
        public const int DELIVERY = 4;
        public const int RESET = 5;

        int? _type, _playerIdx, _spellId, _resultId, _tomeIdx, _acquired, _lost, _repeats;
        List<SpellData> _spells;
        List<AnimationData> _animData;

        public int? Type { get => _type; set => _type = value; }
        public int? PlayerIdx { get => _playerIdx; set => _playerIdx = value; }
        public int? SpellId { get => _spellId; set => _spellId = value; }
        public int? ResultId { get => _resultId; set => _resultId = value; }
        public int? TomeIdx { get => _tomeIdx; set => _tomeIdx = value; }
        public int? Acquired { get => _acquired; set => _acquired = value; }
        public int? Lost { get => _lost; set => _lost = value; }
        public int? Repeats { get => _repeats; set => _repeats = value; }
        public List<SpellData> Spells { get => _spells; set => _spells = value; }
        public List<AnimationData> AnimData { get => _animData; set => _animData = value; }
    }
}
