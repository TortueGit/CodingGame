using System.Collections.Generic;
using System.Linq;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    class Spell : Order
    {
        int _tomeIndex;
        int _taxCount;
        bool _castable;
        bool _repeatable;

        internal Spell(int id, string actionType, int delta0, int delta1, int delta2, int delta3, int tomeIndex, int taxCount, bool castable, bool repeatable)
            : base(id, actionType, delta0, delta1, delta2, delta3)
        {
            _tomeIndex = tomeIndex;
            _taxCount = taxCount;

            _castable = castable;
            _repeatable = repeatable;
        }

        internal int TomeIndex => _tomeIndex;
        internal int TaxCount => _taxCount;
        internal bool Castable => _castable;
        internal bool Reapeatable => _repeatable;
        internal bool CanCast(Inventory inventory) 
        {
            if (Recipe.NbIngredientsAdd - Recipe.NbIngredientsRequired + inventory.Ingredients.Values.Sum() <= 10)
                return Recipe.CanCast(inventory);
            else
                return false;
        }

        internal bool CanGiveAllIngredients(Dictionary<Ingredient, int> ingredientsNeeded)
        {
            // DebugLogs.WriteOrderIngredients(this);

            foreach (KeyValuePair<Ingredient, int> ingredientNeeded in ingredientsNeeded)
            {
                // Console.Error.WriteLine($"Ingredient needed is contains in Recipe : {Recipe.IngredientsTypeAdd.Contains(ingredientNeeded.Key.Type)}");

                // Console.Error.WriteLine($"Ingredient type [{ingredientNeeded.Key.Type}]\n nb needed [{ingredientNeeded.Value}]");
                if (!Recipe.IngredientsTypeAdd.Contains(ingredientNeeded.Key.Type) ||
                    ingredientNeeded.Value > this.Recipe.GetIngredient(ingredientNeeded.Key.GetType()).Value)
                {
                    return false;
                }
            }

            return true;
        }

        internal bool CanGiveAllIngredient(Dictionary<Ingredient, int> ingredientsNeeded)
        {
            foreach (KeyValuePair<Ingredient, int> ingredientNeeded in ingredientsNeeded)
            {
                // Console.Error.WriteLine($"Ingredient needed is contains in Recipe : {Recipe.IngredientsTypeAdd.Contains(ingredientNeeded.Key.Type)}");

                // Console.Error.WriteLine($"Ingredient type [{ingredientNeeded.Key.Type}]\n nb needed [{ingredientNeeded.Value}]");
                if (!Recipe.IngredientsTypeAdd.Contains(ingredientNeeded.Key.Type) ||
                    ingredientNeeded.Value == this.Recipe.GetIngredient(ingredientNeeded.Key.GetType()).Value)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool CanGiveAllIngredientsType(Dictionary<Ingredient, int> ingredientsNeeded)
        {
            return ingredientsNeeded.Keys.All(x => Recipe.IngredientsTypeAdd.Contains(x.Type));
        }

        internal int CostToLearn(Inventory inventory)
        {
            return TomeIndex - inventory.GetIngredient0.Value;
        }
    }
}
