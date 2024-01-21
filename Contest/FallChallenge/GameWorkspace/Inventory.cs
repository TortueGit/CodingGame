using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    class Inventory : IngredientsList
    {
        internal Inventory()
            : base()
        {
        }

        internal Inventory(int[] deltas)
            : base(deltas[0], deltas[1], deltas[2], deltas[3])
        {
        }

        internal Dictionary<Ingredient, int> GetMissingIngredients(IngredientsList ingredientsNeeded)
        {
            Dictionary<Ingredient, int> missingIngredients = new Dictionary<Ingredient, int>();

            foreach (KeyValuePair<Ingredient, int> neededIngredient in ingredientsNeeded.Ingredients.Where(x => x.Value < 0))
            {
                if (GetIngredient(neededIngredient.Key.GetType()).Value < Math.Abs(neededIngredient.Value))
                {
                    missingIngredients.Add(neededIngredient.Key, Math.Abs(neededIngredient.Value) - GetIngredient(neededIngredient.Key.GetType()).Value);
                }
            }

            return missingIngredients;
        }
    }
}
