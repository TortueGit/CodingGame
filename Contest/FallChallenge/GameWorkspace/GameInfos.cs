using System.Collections.Generic;
using System.Linq;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    internal class GameInfos
    {        
        Witch _myWitch;
        Witch _opponentWitch;

        List<Potion> _potions;
        List<Spell> _bookSpells;

        int _maxIngredient0;
        int _maxIngredient1;
        int _maxIngredient2;
        int _maxIngredient3;

        bool _isFirstTurn;

        internal GameInfos()
        {
            _myWitch = new Witch();
            _opponentWitch = new Witch();

            _potions = new List<Potion>();
            _bookSpells = new List<Spell>();

            _isFirstTurn = true;
        }

        internal Witch MyWitch { get => _myWitch; set => _myWitch = value; }
        internal Witch OpponentWitch { get => _opponentWitch; set => _opponentWitch = value; }
        internal int MaxIngredient0 { get => _maxIngredient0; set => _maxIngredient0 = value; }
        internal int MaxIngredient1 { get => _maxIngredient1; set => _maxIngredient1 = value; }
        internal int MaxIngredient2 { get => _maxIngredient2; set => _maxIngredient2 = value; }
        internal int MaxIngredient3 { get => _maxIngredient3; set => _maxIngredient3 = value; }
        internal bool IsFirstTurn { get => _isFirstTurn; set => _isFirstTurn = value; }
        internal List<Potion> Potions { get => _potions; set => _potions = value; }
        internal List<Spell> BookSpells { get => _bookSpells; set => _bookSpells = value; }
        internal Potion GetMaxPricePotion => _potions.OrderByDescending(x => x.Price).OrderBy(x => x.NbIngredientsRequired).FirstOrDefault();
        
        internal void AddOrder(List<Order> allOrders)
        {
            _potions.RemoveAll(x => !allOrders.Select(x => x.Id).Contains(x.Id));
            _bookSpells.RemoveAll(x => !allOrders.Select(x => x.Id).Contains(x.Id));

            if (MyWitch.PotionToBrew != null &&
                !_potions.Contains(MyWitch.PotionToBrew))
            {
                MyWitch.PotionToBrew = null;
            }

            foreach(Order order in allOrders)
            {
                switch (order.ActionType)
                {
                    case Global.BREW:
                        if (!_potions.Any(x => x.Id == order.Id))
                            _potions.Add((Potion)order);
                        break;

                    case Global.LEARN:
                        if (!_bookSpells.Any(x => x.Id == order.Id))
                            _bookSpells.Add((Spell)order);
                        break;

                    case Global.OPPONENT_CAST:
                        _opponentWitch.MySpells.Add((Spell)order);
                        break;
                    case Global.SPELL:
                        _myWitch.MySpells.Add((Spell)order);
                        break;
                }
            }
        }

        internal void ResetMaxIngredient()
        {
            _maxIngredient0 = 0;
            _maxIngredient1 = 0;
            _maxIngredient2 = 0;
            _maxIngredient3 = 0;
        }
    }
}
