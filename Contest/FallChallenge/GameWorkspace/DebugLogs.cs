using System;
using System.Text;

namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
    static class DebugLogs
    {
        internal static void WriteOrderIngredients(Order o)
        {
            StringBuilder sb = new StringBuilder($"Order id[{o.Id}] actionType[{o.ActionType}] \t");
            sb.Append($"Ingredient0[{o.Recipe.GetIngredient0.Value}] \t");
            sb.Append($"Ingredient1[{o.Recipe.GetIngredient1.Value}] \t");
            sb.Append($"Ingredient2[{o.Recipe.GetIngredient2.Value}] \t");
            sb.Append($"Ingredient3[{o.Recipe.GetIngredient3.Value}]");

            Console.Error.WriteLine(sb);
        }

        internal static void WriteInventoryIngredients(Inventory inventory)
        {
            StringBuilder sb = new StringBuilder($"Inventory ingredients : \t");
            sb.Append($"Ingredient0[{inventory.GetIngredient0.Value}] \t");
            sb.Append($"Ingredient1[{inventory.GetIngredient1.Value}] \t");
            sb.Append($"Ingredient2[{inventory.GetIngredient2.Value}] \t");
            sb.Append($"Ingredient3[{inventory.GetIngredient3.Value}]");

            Console.Error.WriteLine(sb);
        }

        internal static void WriteMaxIngredientsNeeded(GameInfos game)
        {
            StringBuilder sb = new StringBuilder($"Max ingredients needed : \t");
            sb.Append($"Ingredient0[{game.MaxIngredient0}] \t");
            sb.Append($"Ingredient2[{game.MaxIngredient1}] \t");
            sb.Append($"Ingredient2[{game.MaxIngredient2}] \t");
            sb.Append($"Ingredient3[{game.MaxIngredient3}] \t");

            Console.Error.WriteLine(sb);
        }
    }
}
