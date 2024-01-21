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

                myGame.NewTurn();
            }
        }


        /**
        *
        *   Pense bête:
        *   - Calculer nb de tour pour obtenir ingredients manquant.
        *   - Regarder si autre potion dans la liste peut être brew maintenant.
        *   - Vérifier que brew une potion moins couteuse est avantageux par rapport au nombre de tour restant pour avoir les ingredients manquants.
        *
        **/
        static string Play(GameInfos game)
        {
            Dictionary<Ingredient, int> missingIngredients = new Dictionary<Ingredient, int>();
            if (game.MyWitch.CanBrewPotion || game.MyWitch.PotionToBrew == null)
            {
                if (game.MyWitch.PotionToBrew == null)
                {
                    game.MyWitch.PotionToBrew = game.GetMaxPricePotion;
                    if (game.MyWitch.CanBrewPotion)
                        return $"{Global.BREW} {game.MyWitch.PotionToBrew.Id}";
                    else
                        missingIngredients = game.MyWitch.MyInventory.GetMissingIngredients(game.MyWitch.PotionToBrew.Recipe);
                }
                else
                    return $"{Global.BREW} {game.MyWitch.PotionToBrew.Id}";
            }

            // No potion to BREW... What should I do?

            // Is it free spell available in the book spell ?
            var freeSpellsToLearn = game.BookSpells.Where(x => x.Recipe.NbIngredientsRequired == 0);
            if (freeSpellsToLearn.Count() > 0)
            {
                Spell spellToLearn = GetBestFreeSpellsToLearn(freeSpellsToLearn);
                if (spellToLearn != null)
                {
                    var tmpFreeSpells = new List<Spell>(freeSpellsToLearn);

                    int costToLearn = spellToLearn.CostToLearn(game.MyWitch.MyInventory);
                    while (freeSpellsToLearn.Count() > 0 && costToLearn > 0 && spellToLearn != null)
                    {
                        tmpFreeSpells.Remove(spellToLearn);
                        spellToLearn = tmpFreeSpells.FirstOrDefault();
                        costToLearn = spellToLearn.CostToLearn(game.MyWitch.MyInventory);
                    }

                    if (costToLearn <= 0 && spellToLearn != null)
                        return $"{Global.LEARN} {spellToLearn.Id}";
                    else
                    {
                        spellToLearn = GetBestFreeSpellsToLearn(freeSpellsToLearn);
                        costToLearn = spellToLearn.CostToLearn(game.MyWitch.MyInventory);
                        if (missingIngredients.ContainsKey(game.MyWitch.MyInventory.GetIngredient0.Key))
                            missingIngredients[game.MyWitch.MyInventory.GetIngredient0.Key] += costToLearn;
                        else
                            missingIngredients.Add(game.MyWitch.MyInventory.GetIngredient0.Key, costToLearn);
                    }
                }
            }

            // Do I need some ingredient0?
            if (missingIngredients.Where(x => x.Key.Type == 0).Count() > 0)
            {
                Spell spell = null;
                if (game.MyWitch.MySpells.Where(x => x.Recipe.GetIngredient0.Value > 0)
                                            .Count(x => x.Castable && x.CanCast(game.MyWitch.MyInventory)) > 0)
                {
                    spell = game.MyWitch.MySpells.Where(x => x.Recipe.GetIngredient0.Value > 0 && x.Castable && x.CanCast(game.MyWitch.MyInventory))
                                                    .FirstOrDefault();
                    if (spell != null)
                        return $"{Global.SPELL} {spell.Id}";
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (missingIngredients.Any(x => x.Key.Type == i))
                {
                    var spellsToCast = game.MyWitch.MySpells.Where(x => x.Recipe.Ingredients.Any(x => x.Key.Type == i && x.Value > 0))
                                            .OrderByDescending(x => x.Recipe.Ingredients.Single(x => x.Key.Type == i).Value);
                    var spellsToLearn = game.BookSpells.Where(x => x.Recipe.Ingredients.Any(x => x.Key.Type == i && x.Value > 0))
                                            .OrderByDescending(x => x.Recipe.Ingredients.Single(x => x.Key.Type == i).Value);

                    if (spellsToCast.Count() > 0 || spellsToLearn.Count() > 0)
                    {
                        if (spellsToCast.Where(x => x.CanCast(game.MyWitch.MyInventory) && x.Castable).Count() > 0)
                        {
                            Spell spell = spellsToCast.Where(x => x.CanCast(game.MyWitch.MyInventory) && x.Castable).FirstOrDefault();
                            if (spell != null)
                                return $"{Global.SPELL} {spell.Id}";
                        }
                        else 
                        {
                            Spell spell = spellsToLearn.Where(x => x.CostToLearn(game.MyWitch.MyInventory) <= 0).FirstOrDefault();
                            if (spell != null)
                                return $"{Global.LEARN} {spell.Id}";
                        }
                    }
                }
            }

            var stl = game.BookSpells.Where(x => x.CostToLearn(game.MyWitch.MyInventory) <= 0);
            if (stl.Count() > 0)
                return $"{Global.SPELL} {stl.First().Id}";

            if (game.MyWitch.MySpells.Any(x => !x.Castable))
                return $"{Global.REST}";
            
            return Global.WAIT;
        }

        static Spell GetBestFreeSpellsToLearn(IEnumerable<Spell> spells)
        {
            return spells.OrderByDescending(x => x.NbIngredientTypesAdd)
                        .OrderByDescending(x => x.NbIngredientsAdd)
                        .OrderBy(x => x.NbIngredientsRequired)
                        .OrderBy(x => x.TomeIndex).FirstOrDefault();            
        }
    }
}
