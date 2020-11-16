using System;
using System.Collections.Generic;
using CodingGame.Contest.FallChallenge.src.game.action;
using CodingGame.Contest.FallChallenge.src.game.spell;
using Action = CodingGame.Contest.FallChallenge.src.game.action.Action;

namespace CodingGame.Contest.FallChallenge.src.game
{
    public class PlayerWitch
    {
        private Recipe _inventory = new Recipe();
        private List<PlayerSpell> _spells = new List<PlayerSpell>();
        private Action _action;
        private int _deliveriesCount = 0;
        private String _message;

        public PlayerWitch()
        {
        }

        public int GetExpectedOutputLines()
        {
            return 1;
        }

        public void InitSpells()
        {
            _spells.Add(new PlayerSpell(new Recipe(2, 0, 0, 0), this));
            _spells.Add(new PlayerSpell(new Recipe(-1, 1, 0, 0), this));
            _spells.Add(new PlayerSpell(new Recipe(0, -1, 1, 0), this));
            _spells.Add(new PlayerSpell(new Recipe(0, 0, -1, 1), this));
        }

        public void Reset()
        {
            SetAction(Action.NO_ACTION);
            _message = null;
        }

        public String GetMessage()
        {
            return _message;
        }

        public bool CanAfford(Recipe recipe, int repeats)
        {
            for (int i = 0; i < Game.INGREDIENT_TYPE_COUNT; ++i)
            {
                if (GetInventory().Delta[i] + recipe.Delta[i] * repeats < 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool EnoughSpace(Recipe recipe, int repeats)
        {
            return recipe.GetTotal() * repeats + GetInventory().GetTotal() <= Game.MAX_SPACE;
        }

        public bool CanDeliver(Recipe need)
        {
            for (int i = 0; i < Game.INGREDIENT_TYPE_COUNT; ++i)
            {
                if (GetInventory().Delta[i] + need.Delta[i] < 0)
                {
                    return false;
                }
            }
            return true;
        }

        // public void AddScore(int n)
        // {
        //     SetScore(GetScore() + n);
        // }

        public void AddDelivery()
        {
            _deliveriesCount++;
        }

        public int GetDeliveriesCount()
        {
            return _deliveriesCount;
        }

        public Recipe GetInventory()
        {
            return _inventory;
        }

        public List<PlayerSpell> GetSpells()
        {
            return _spells;
        }

        public Action GetAction()
        {
            return _action;
        }

        public void SetAction(Action action)
        {
            _action = action;
        }

        public int[] GetDelta()
        {
            return _inventory.Delta;
        }

        public void SetMessage(String message)
        {
            _message = message;
        }
    }
}
