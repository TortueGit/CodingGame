using System;
using System.Collections.Generic;

namespace CodingGame.Contest.FallChallenge.src.view
{
    public class FrameViewData 
    {
        List<EventData> _events;
        List<int?> _scores;
        List<List<int?>> _playerSpells;
        List<int?> _tomeSpells, _deliveries;
        List<int[]> _inventories;
        List<int?> _active;
        Dictionary<int?, int?> _stock;
        Dictionary<int?, BonusData> _bonus;
        Dictionary<int?, String> _messages;

        public List<EventData> Events { get => _events; set => _events = value; }
        public List<int?> Scores { get => _scores; set => _scores = value; }
        public List<List<int?>> PlayerSpells { get => _playerSpells; set => _playerSpells = value; }
        public List<int?> TomeSpells { get => _tomeSpells; set => _tomeSpells = value; }
        public List<int?> Deliveries { get => _deliveries; set => _deliveries = value; }
        public List<int[]> Inventories { get => _inventories; set => _inventories = value; }
        public List<int?> Active { get => _active; set => _active = value; }
        public Dictionary<int?, int?> Stock { get => _stock; set => _stock = value; }
        public Dictionary<int?, BonusData> Bonus { get => _bonus; set => _bonus = value; }
        public Dictionary<int?, string> Messages { get => _messages; set => _messages = value; }
    }
}
