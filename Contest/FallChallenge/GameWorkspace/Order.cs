namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    abstract class Order
    {
        int _id;
        string _actionType;

        Recipe _recipe;

        int _nbIngredientsAdd;
        int _nbIngredientsCost;
        int _nbIngredientTypesAdd;
        int _nbIngredientTypesCost;
        int _nbIngredientsRequired;

        internal Order(int id, string actionType, int delta0, int delta1, int delta2, int delta3)
        {
            _id = id;
            _actionType = actionType;

            _recipe = new Recipe(delta0, delta1, delta2, delta3);
        }

        internal int Id => _id;
        internal string ActionType => _actionType;
        internal Recipe Recipe => _recipe;
        internal int NbIngredientsAdd => _recipe.NbIngredientsAdd;
        internal int NbIngredientsRequired => _recipe.NbIngredientsRequired;
        internal int NbIngredientTypesAdd => _recipe.IngredientsTypeAdd.Count;
        internal int NbIngredientTypesRequired => _recipe.IngredientsTypeRequired.Count;
    }
}
