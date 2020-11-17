using System;
using System.Collections.Generic;
using System.Linq;
using CodingGame.Contest.FallChallenge.src.game.spell;
using CodingGame.Contest.FallChallenge.src.view;
using Action = CodingGame.Contest.FallChallenge.src.game.action.Action;

namespace CodingGame.Contest.FallChallenge.src.game
{
    //@Singleton
    public class Game
    {
        public const int READ_AHEAD_COST = 1;
        public const int INGREDIENT_TYPE_COUNT = 4;
        public const int MAX_SPACE = 10;

        public static bool ACTIVE_SPELLS;
        public static bool ACTIVE_TOME;
        public static bool INVENTORY_BONUS;

        List<PlayerWitch> _players = new List<PlayerWitch>();

        private List<TomeSpell> _tome;
        private List<DeliverySpell> _deliveries;
        private List<DeliverySpell> _newDeliveries;
        private List<TomeSpell> _newTomeSpells;
        HashSet<TomeSpell> _learntSpells = new HashSet<TomeSpell>();
        HashSet<DeliveryCompletion> _delivered = new HashSet<DeliveryCompletion>();
        private List<EventData> _viewerEvents;

        private int[] _bonus;
        private int[] _bonusValue = new int[] { 3, 1 };
        private int[] _tomeStockGain;

        private void SlideBonus()
        {
            if (_bonus[0] <= 0)
            {
                _bonus[0] = _bonus[1];
                _bonus[1] = 0;
                _bonusValue[0] = 1;
                _bonusValue[1] = 0;
            }
        }

        private int GetScoreOf(DeliverySpell delivery)
        {
            int index = _deliveries.IndexOf(delivery);
            int bonusScore = 0;
            if (index < 2)
            {
                if (_bonus[index] > 0)
                {
                    bonusScore = _bonusValue[index];
                }
            }

            return delivery.GetScore() + bonusScore;
        }

        public int GetBonusValue(DeliverySpell spell)
        {
            if (GetBonusAmount(spell) == 0)
            {
                return 0;
            }

            int index = _deliveries.IndexOf(spell);

            if (index < 0 || index > 1)
            {
                return 0;
            }

            return _bonusValue[index];
        }

        public int GetBonusAmount(DeliverySpell spell)
        {
            int index = _deliveries.IndexOf(spell);

            if (index < 0 || index > 1)
            {
                return 0;
            }

            return _bonus[index];
        }

        private int GetScoreOf(Spell spell)
        {
            if (spell.GetType() == typeof(DeliverySpell))
            {
                return GetScoreOf((DeliverySpell)spell);
            }
            return spell.GetScore();
        }

        private SpellType GetSpellType(Spell spell, PlayerWitch player)
        {
            if (spell.GetType() == typeof(TomeSpell))
            {
                return SpellType.LEARN;
            }
            else if (spell.GetType() == typeof(DeliverySpell))
            {
                return SpellType.BREW;
            }
            else
            {
                if (spell.IsOwner(player))
                {
                    return SpellType.CAST;
                }
                else
                {
                    return SpellType.OPPONENT_CAST;

                }
            }
        }

        public void CheckSpellActionType(Action action, SpellType type)
        {
            String expectedStr = null;

            switch (type)
            {
                case SpellType.LEARN:
                    expectedStr = "LEARN";
                    break;
                case SpellType.CAST:
                    expectedStr = "CAST";
                    break;
                case SpellType.BREW:
                    expectedStr = "BREW";
                    break;
                case SpellType.OPPONENT_CAST:
                    throw new GameException("Tried to cast an opponent's spell...");
            }

            if (!action.Str.Equals(expectedStr))
            {
                throw new GameException(String.Format("Command does not match action, expected '%s' but got '%s'", expectedStr, action.Str));
            }
        }

        private Spell GetSpellById(int id)
        {
            return new List<Spell>().Concat(_deliveries)
                .Concat(_tome)
                .Concat(_delivered.Select(x => x.Delivery))
                .Concat(_players.SelectMany(x => x.Spells))
                .Single(x => x.Id == id);
        }

        private void DoReset(PlayerWitch p)
        {
            if (p.Spells.All(spell => spell.IsActive()))
            {
                throw new GameException("All spells are already castable");
            }
            p.Spells.ForEach(x => x.Activate());

            EventData e = new EventData();
            e.Type = EventData.RESET;
            e.PlayerIdx = p.Index;
            _viewerEvents.Add(e);
        }

        private DeliveryCompletion DoDelivery(PlayerWitch p, DeliverySpell del)
        {
            if (!p.CanDeliver(del.Recipe))
            {
                throw new GameException("Not enough ingredients for order " + del.Id);
            }
            for (int i = 0; i < INGREDIENT_TYPE_COUNT; ++i)
            {
                p.Inventory.Add(i, del.Recipe.Delta[i]);
            }
            DeliveryCompletion delCompletion = new DeliveryCompletion(del, _deliveries.IndexOf(del), GetScoreOf(del));
            _delivered.Add(delCompletion);
            p.AddDelivery();
            p.AddScore(GetScoreOf(del));

            EventData e = new EventData();
            e.Type = EventData.DELIVERY;
            e.PlayerIdx = p.Index;
            e.SpellId = del.Id;
            _viewerEvents.Add(e);
            return delCompletion;
        }

        private void DoLearn(PlayerWitch p, TomeSpell spell)
        {
            int index = _tome.IndexOf(spell);
            if (p.Inventory.Delta[0] < index * READ_AHEAD_COST)
            {
                throw new GameException("Not enough ingredients to learn " + spell.Id);
            }
            for (int i = 0; i < index; ++i)
            {
                _tomeStockGain[i] += READ_AHEAD_COST;
                p.Inventory.Delta[0] -= READ_AHEAD_COST;
            }
            PlayerSpell learnt = new PlayerSpell(spell, p);
            p.Spells.Add(learnt);

            int maxToGet = MAX_SPACE - p.Inventory.GetTotal();
            int ingredientsGot = Math.Min(maxToGet, spell.GetStock());
            p.Inventory.Delta[0] += ingredientsGot;

            _learntSpells.Add(spell);

            EventData e = new EventData();
            e.Type = EventData.LEARN;
            e.PlayerIdx = p.Index;
            e.SpellId = spell.Id;
            e.ResultId = learnt.Id;
            e.TomeIdx = index;
            e.Acquired = ingredientsGot;
            e.Lost = spell.GetStock() - ingredientsGot;
            _viewerEvents.Add(e);
        }

        private void DoPlayerSpell(PlayerWitch p, PlayerSpell spell)
        {
            int repeats = p.Action.GetRepeats();
            if (repeats < 1)
            {
                throw new GameException("Repeat can't be zero (on " + spell.Id + ")");
            }
            if (repeats > 1 && !spell.IsRepeatable())
            {
                throw new GameException("Spell " + spell.Id + " is not repeatable");
            }
            if (!spell.IsActive())
            {
                throw new GameException("Spell " + spell.Id + " is exhausted");
            }
            if (!p.CanAfford(spell.Recipe, repeats))
            {
                throw new GameException("Not enough ingredients for spell " + spell.Id);
            }
            if (!p.EnoughSpace(spell.Recipe, repeats))
            {
                throw new GameException("Not enough space in inventory for spell " + spell.Id);
            }

            //do spell
            for (int k = 0; k < repeats; ++k)
            {
                for (int i = 0; i < INGREDIENT_TYPE_COUNT; ++i)
                {
                    p.Inventory.Add(i, spell.GetDelta()[i]);
                }
            }
            spell.Deactivate();

            EventData e = new EventData();
            e.Type = EventData.PLAYER_SPELL;
            e.PlayerIdx = p.Index;
            e.SpellId = spell.Id;
            e.Repeats = repeats;
            _viewerEvents.Add(e);
        }

        public List<String> GetGlobalInfoFor(PlayerWitch player)
        {
            return new List<String>();
        }

        public void ResetGameTurnData()
        {
            _players.ForEach(x => x.Reset());
            _learntSpells.Clear();
            _delivered.Clear();
            _viewerEvents.Clear();
            _newDeliveries.Clear();
        }

        public List<EventData> GetViewerEvents()
        {
            return _viewerEvents;
        }

        public List<TomeSpell> GetTome()
        {
            return _tome;
        }

        public List<DeliverySpell> GetDeliveries()
        {
            return _deliveries;
        }

        public static String GetExpected()
        {
            if (!Game.ACTIVE_SPELLS)
            {
                return "BREW <id> | WAIT";
            }
            if (!Game.ACTIVE_TOME)
            {
                return "BREW <id> | CAST <id> | REST | WAIT";
            }
            return "BREW <id> | CAST <id> [<repeat>] | LEARN <id> | REST | WAIT";
        }

        public void OnEnd()
        {
            if (INVENTORY_BONUS)
            {
                foreach (PlayerWitch player in _players)
                {
                    for (int i = 1; i < INGREDIENT_TYPE_COUNT; i++)
                    {
                        player.AddScore(player.Inventory.Delta[i]);
                    }
                }
            }
        }

        public Dictionary<int, BonusData> GetBonusData()
        {
            Dictionary<int, BonusData> bonusData = new Dictionary<int, BonusData>();
            for (int i = 0; i < 2; ++i)
            {
                if (_bonus[i] > 0)
                {
                    bonusData.Add(i, new BonusData(_bonus[i], _bonusValue[i]));
                }
            }

            return bonusData;
        }
    }
}
