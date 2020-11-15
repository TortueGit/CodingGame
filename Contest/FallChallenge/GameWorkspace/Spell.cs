using System.Collections.Generic;

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

        internal bool CanGiveAllIngredients(Dictionary<Ingredient, int> ingredientsNeeded)
        {
            foreach (KeyValuePair<Ingredient, int> ingredientNeeded in ingredientsNeeded)
            {                
                if (Recipe.IngredientsTypeAdd.Contains(ingredientNeeded.Key.Type) && 
                    ingredientNeeded.Value > this.Recipe.Ingredients[ingredientNeeded.Key])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
