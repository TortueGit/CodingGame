using System.Collections.Generic;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    internal class Witch
    {
        Witch _previousState;
        Inventory _myInventory;
        List<Spell> _mySpells;
        Potion _potionToBrew;
        bool _canBrewPotion;
        int _score;

        internal Witch()
        {
            _myInventory = new Inventory();
            _mySpells = new List<Spell>();
            _score = 0;
            _previousState = null;
            _potionToBrew = null;
            _canBrewPotion = false;
        }

        internal Witch PreviousState { get => _previousState; set => _previousState = value; }
        internal Inventory MyInventory { get => _myInventory; set => _myInventory = value; }
        internal List<Spell> MySpells { get => _mySpells; set => _mySpells = value; }
        internal Potion PotionToBrew
        {
            get => _potionToBrew;
            set
            {
                _potionToBrew = value;
            }
        }

        public bool CanBrewPotion
        {
            get => _canBrewPotion;
            set
            {
                if (PotionToBrew != null)
                {
                    if (MyInventory.Ingredients.Values.Count >= PotionToBrew.Recipe.NbIngredientsRequired)
                    {
                        _canBrewPotion = MyInventory.Equals(PotionToBrew.Recipe.Ingredients);
                    }
                }
                _canBrewPotion = false;
            }
        }
        internal int Score { get => _score; set => _score = value; }

        internal void Reset()
        {
            _previousState = this;
            _myInventory = new Inventory();
            _mySpells = new List<Spell>();
            _score = 0;
        }
    }
}
