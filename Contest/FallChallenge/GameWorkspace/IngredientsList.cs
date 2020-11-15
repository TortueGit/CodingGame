using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    abstract class IngredientsList
    {
        Dictionary<Ingredient, int> _ingredients;

        internal IngredientsList()
        {
            _ingredients.Add(new Ingredient0(), 0);
            _ingredients.Add(new Ingredient1(), 0);
            _ingredients.Add(new Ingredient2(), 0);
            _ingredients.Add(new Ingredient3(), 0);
        }

        internal IngredientsList(int delta0, int delta1, int delta2, int delta3)
        {
            _ingredients.Add(new Ingredient0(), delta0);
            _ingredients.Add(new Ingredient1(), delta1);
            _ingredients.Add(new Ingredient2(), delta2);
            _ingredients.Add(new Ingredient3(), delta3);
        }

        internal Dictionary<Ingredient, int> Ingredients { get => _ingredients; set => _ingredients = value; }
        internal KeyValuePair<Ingredient, int> GetIngredient(Type type) => _ingredients.Where(x => x.Key.GetType() == type).First();
        internal KeyValuePair<Ingredient, int> GetIngredient0 => GetIngredient(typeof(Ingredient0));
        internal KeyValuePair<Ingredient, int> GetIngredient1 => GetIngredient(typeof(Ingredient1));
        internal KeyValuePair<Ingredient, int> GetIngredient2 => GetIngredient(typeof(Ingredient2));
        internal KeyValuePair<Ingredient, int> GetIngredient3 => GetIngredient(typeof(Ingredient3));

        public override bool Equals(object obj)
        {
            IngredientsList recipe = (IngredientsList)obj;

            foreach(KeyValuePair<Ingredient, int> ingredient in Ingredients.Where(x => x.Value < 0))
            {
                if (ingredient.Value > recipe.Ingredients[ingredient.Key])
                    return false;
            }
            
            return true;
        }
    }
}
