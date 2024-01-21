using System.Collections.Generic;

namespace CodingGame.Contest.FallChallenge.src.view
{
    public class GlobalViewData 
    {
        List<List<SpellData>> _playerSpells;
        List<SpellData> _tomeSpells;
        List<SpellData> _deliveries;

        public List<List<SpellData>> PlayerSpells { get => _playerSpells; set => _playerSpells = value; }
        public List<SpellData> TomeSpells { get => _tomeSpells; set => _tomeSpells = value; }
        public List<SpellData> Deliveries { get => _deliveries; set => _deliveries = value; }
    }
}
