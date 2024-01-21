using System;
using System.Collections.Generic;
using CodingGame.Contest.FallChallenge.src.game.spell;
using Action = CodingGame.Contest.FallChallenge.src.game.action.Action;

namespace CodingGame.Contest.FallChallenge.src.game
{
    public class PlayerWitch
    {
        public const int GET_EXPECTED_OUTPUT_LINES = 1;
        private Recipe _inventory = new Recipe();
        private List<PlayerSpell> _spells = new List<PlayerSpell>();
        private Action _action;
        private int _deliveriesCount = 0;
        private String _message;
        private int _index;
        private int _score = 0;

        public PlayerWitch()
        {
        }

        public int Index => _index;
        public int DeliveriesCount => _deliveriesCount;
        public int Score => _score;
        public Recipe Inventory { get => _inventory; set => _inventory = value; }
        public List<PlayerSpell> Spells { get => _spells; set => _spells = value; }
        public Action Action { get => _action; set => _action = value; }
        public String Message { get => _message; set => _message = value; }

        public void InitSpells()
        {
            _spells.Add(new PlayerSpell(new Recipe(2, 0, 0, 0), this));
            _spells.Add(new PlayerSpell(new Recipe(-1, 1, 0, 0), this));
            _spells.Add(new PlayerSpell(new Recipe(0, -1, 1, 0), this));
            _spells.Add(new PlayerSpell(new Recipe(0, 0, -1, 1), this));
        }
        

        public void Reset()
        {
            _action = Action.NO_ACTION;
            _message = null;
        }


        public bool CanAfford(Recipe recipe, int repeats)
        {
            for (int i = 0; i < Game.INGREDIENT_TYPE_COUNT; ++i)
            {
                if (_inventory.Delta[i] + recipe.Delta[i] * repeats < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool EnoughSpace(Recipe recipe, int repeats)
        {
            return recipe.GetTotal() * repeats + _inventory.GetTotal() <= Game.MAX_SPACE;
        }

        public bool CanDeliver(Recipe need)
        {
            for (int i = 0; i < Game.INGREDIENT_TYPE_COUNT; ++i)
            {
                if (_inventory.Delta[i] + need.Delta[i] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddDelivery()
        {
            _deliveriesCount++;
        }

        public int[] GetDelta()
        {
            return _inventory.Delta;
        }

        public void AddScore(int score)
        {
            _score += score;
        }
    }
}
