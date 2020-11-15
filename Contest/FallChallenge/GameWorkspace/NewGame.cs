using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    class NewGame
    {
        static void Main(string[] args)
        {
            string[] inputs;
            GameInfos myGame = new GameInfos();

            // game loop
            while (true)
            {
                #region INIT_VALUES
                myGame.ResetMaxIngredient();

                List<Order> allOrders = new List<Order>();
                int actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
                for (int i = 0; i < actionCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int actionId = int.Parse(inputs[0]); // the unique ID of this spell or recipe
                    string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
                    
                    int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change                
                    int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
                    int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
                    int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change

                    int price = int.Parse(inputs[6]); // the price in rupees if this is a potion

                    if (actionType.Equals(Global.BREW))
                    {
                        if (myGame.MaxIngredient0 < Math.Abs(delta0))
                            myGame.MaxIngredient0 = Math.Abs(delta0);
                        if (myGame.MaxIngredient1 < Math.Abs(delta1))
                            myGame.MaxIngredient1 = Math.Abs(delta1);
                        if (myGame.MaxIngredient2 < Math.Abs(delta2))
                            myGame.MaxIngredient2 = Math.Abs(delta2);
                        if (myGame.MaxIngredient3 < Math.Abs(delta3))
                            myGame.MaxIngredient3 = Math.Abs(delta3);

                        DebugLogs.WriteMaxIngredientsNeeded(myGame);
                    }

                    int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax
                    int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell
                    bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
                    bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell

                    Order o = null;
                    switch (actionType)
                    {
                        case Global.BREW:
                        o = new Potion(actionId, actionType, delta0, delta1, delta2, delta3, price);
                        break;

                        case Global.OPPONENT_CAST:
                        case Global.SPELL:
                        case Global.LEARN:
                        o = new Spell(actionId, actionType, delta0, delta1, delta2, delta3, tomeIndex, taxCount, castable, repeatable);
                        break;
                    }
                    if (o != null)
                        allOrders.Add(o);
                }
                myGame.AddOrder(allOrders);

                int[] deltas = Array.ConvertAll<string, int>(Console.ReadLine().Split(' '), int.Parse);
                myGame.MyWitch.MyInventory = new Inventory(deltas);

                deltas = Array.ConvertAll<string, int>(Console.ReadLine().Split(' '), int.Parse);
                myGame.OpponentWitch.MyInventory = new Inventory(deltas);
                #endregion

                string action = Play(myGame);

                Console.WriteLine(action);
            }
        }

        static string Play(GameInfos game)
        {
            if (!game.MyWitch.CanBrewPotion)
            {
                if (game.MyWitch.PotionToBrew == null)
                {
                    game.MyWitch.PotionToBrew = game.GetMaxPricePotion;
                    if (game.MyWitch.CanBrewPotion)
                        return $"{Global.BREW} {game.MyWitch.PotionToBrew.Id}";
                }

                // Can I do Spell to get the needed ingredients?
                Dictionary<Ingredient, int> missingIngredient = game.MyWitch.MyInventory.GetMissingIngredients(game.MyWitch.PotionToBrew.Recipe);

                Spell spellToCast = game.MyWitch.MySpells.Where(x => x.CanGiveAllIngredients(missingIngredient)).FirstOrDefault();
                if (spellToCast != null)
                {
                    if (spellToCast.Castable)
                        return $"{Global.SPELL} {spellToCast.Id}";
                    else
                        return Global.REST;
                }

                // Can I learn a spell which give all the ingredients?
                spellToCast = game.BookSpells.Where(x => x.CanGiveAllIngredients(missingIngredient)).FirstOrDefault();
                if (spellToCast != null)
                {
                    if (game.IsFirstTurn)
                        return $"{Global.LEARN} {spellToCast.Id}";

                    if (spellToCast.TomeIndex <= game.MyWitch.MyInventory.GetIngredient0.Value)
                    {
                        return $"{Global.LEARN} {spellToCast.Id}";
                    }
                }
            }
            else
            {
                if (game.MyWitch.PotionToBrew != null)
                    return $"{Global.BREW} {game.MyWitch.PotionToBrew.Id}";
            }

            return Global.WAIT;
        }
    }
}
