using System.Collections.Generic;
using System.Linq;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    class Recipe : IngredientsList
    {
        internal Recipe(int delta0, int delta1, int delta2, int delta3)
            : base(delta0, delta1, delta2, delta3)
        {
        }

        internal int NbIngredientsRequired => Ingredients.Where(x => x.Value < 0).Sum(x => x.Value);
        internal int NbIngredientsAdd => Ingredients.Where(x => x.Value > 0).Sum(x => x.Value);
        internal List<int> IngredientsTypeRequired => Ingredients.Where(x => x.Value < 0).Select(x => x.Key.Type).ToList();
        internal List<int> IngredientsTypeAdd => Ingredients.Where(x => x.Value > 0).Select(x => x.Key.Type).ToList();
    }
}
