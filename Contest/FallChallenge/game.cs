using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

static class Global
{
    internal const string SPELL = "CAST";
    internal const string OPPONENT_CAST = "OPPONENT_CAST";
    internal const string LEARN = "LEARN";
    internal const string BREW = "BREW";
    internal const string WAIT = "WAIT";
    internal const string REST = "REST";
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
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
                    if (spellToLearn != null)
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

        if (game.MyWitch.MySpells.Any(x => !x.Castable))
            return $"{Global.REST}";

        var stl = game.BookSpells.Where(x => x.CostToLearn(game.MyWitch.MyInventory) <= 0);
        if (stl.Count() > 0)
            return $"{Global.SPELL} {stl.First().Id}";

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

internal class GameInfos
{
    Witch _myWitch;
    Witch _opponentWitch;

    List<Potion> _potions;
    List<Spell> _bookSpells;

    int _maxIngredient0;
    int _maxIngredient1;
    int _maxIngredient2;
    int _maxIngredient3;

    bool _isFirstTurn;

    internal GameInfos()
    {
        _myWitch = new Witch();
        _opponentWitch = new Witch();

        _potions = new List<Potion>();
        _bookSpells = new List<Spell>();

        _isFirstTurn = true;
    }

    internal Witch MyWitch { get => _myWitch; set => _myWitch = value; }
    internal Witch OpponentWitch { get => _opponentWitch; set => _opponentWitch = value; }
    internal int MaxIngredient0 { get => _maxIngredient0; set => _maxIngredient0 = value; }
    internal int MaxIngredient1 { get => _maxIngredient1; set => _maxIngredient1 = value; }
    internal int MaxIngredient2 { get => _maxIngredient2; set => _maxIngredient2 = value; }
    internal int MaxIngredient3 { get => _maxIngredient3; set => _maxIngredient3 = value; }
    internal bool IsFirstTurn { get => _isFirstTurn; set => _isFirstTurn = value; }
    internal List<Potion> Potions { get => _potions; set => _potions = value; }
    internal List<Spell> BookSpells { get => _bookSpells; set => _bookSpells = value; }
    internal Potion GetMaxPricePotion => _potions.OrderByDescending(x => x.Price).OrderBy(x => x.NbIngredientsRequired).FirstOrDefault();

    internal void AddOrder(List<Order> allOrders)
    {
        _potions.RemoveAll(x => !allOrders.Select(x => x.Id).Contains(x.Id));
        _bookSpells.RemoveAll(x => !allOrders.Select(x => x.Id).Contains(x.Id));

        if (MyWitch.PotionToBrew != null &&
            !_potions.Contains(MyWitch.PotionToBrew))
        {
            MyWitch.PotionToBrew = null;
        }

        foreach (Order order in allOrders)
        {
            switch (order.ActionType)
            {
                case Global.BREW:
                    if (!_potions.Any(x => x.Id == order.Id))
                        _potions.Add((Potion)order);
                    break;

                case Global.LEARN:
                    if (!_bookSpells.Any(x => x.Id == order.Id))
                        _bookSpells.Add((Spell)order);
                    break;

                case Global.OPPONENT_CAST:
                    _opponentWitch.MySpells.Add((Spell)order);
                    break;
                case Global.SPELL:
                    _myWitch.MySpells.Add((Spell)order);
                    break;
            }
        }
    }

    internal void ResetMaxIngredient()
    {
        _maxIngredient0 = 0;
        _maxIngredient1 = 0;
        _maxIngredient2 = 0;
        _maxIngredient3 = 0;
    }

    internal void NewTurn()
    {
        _myWitch.Reset();
        _opponentWitch.Reset();

        _potions = new List<Potion>();
        _bookSpells = new List<Spell>();

        _isFirstTurn = false;
    }
}

abstract class IngredientsList
{
    Dictionary<Ingredient, int> _ingredients = new Dictionary<Ingredient, int>();

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
    internal KeyValuePair<Ingredient, int> GetIngredient(Ingredient ingredient) => _ingredients.Where(x => x.Key.Type == ingredient.Type).First();
    internal KeyValuePair<Ingredient, int> GetIngredient0 => GetIngredient(typeof(Ingredient0));
    internal KeyValuePair<Ingredient, int> GetIngredient1 => GetIngredient(typeof(Ingredient1));
    internal KeyValuePair<Ingredient, int> GetIngredient2 => GetIngredient(typeof(Ingredient2));
    internal KeyValuePair<Ingredient, int> GetIngredient3 => GetIngredient(typeof(Ingredient3));

    public bool CanCast(IngredientsList inventory)
    {
        foreach (KeyValuePair<Ingredient, int> ingredient in Ingredients.Where(x => x.Value < 0))
        {
            if (Math.Abs(ingredient.Value) > inventory.GetIngredient(ingredient.Key).Value)
                return false;
        }

        return true;
    }
}

abstract class Ingredient
{
    int _type;
    int _cost;

    internal Ingredient(int type, int cost)
    {
        _type = type;
        _cost = cost;
    }

    internal int Type => _type;
    internal int Cost => _cost;
}

class Ingredient0 : Ingredient
{
    internal Ingredient0()
        : base(0, 1)
    {
    }
}

class Ingredient1 : Ingredient
{
    internal Ingredient1()
        : base(1, 2)
    {
    }
}

class Ingredient2 : Ingredient
{
    internal Ingredient2()
        : base(2, 3)
    {
    }
}

class Ingredient3 : Ingredient
{
    internal Ingredient3()
        : base(3, 4)
    {
    }
}

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

internal class Witch
{
    Witch _previousState;
    Inventory _myInventory;
    List<Spell> _mySpells;
    Potion _potionToBrew;
    bool _canBrewPotion;
    int _score;

    internal Witch()
    {
        _myInventory = new Inventory();
        _mySpells = new List<Spell>();
        _score = 0;
        _previousState = null;
        _potionToBrew = null;
        _canBrewPotion = false;
    }

    internal Witch PreviousState { get => _previousState; set => _previousState = value; }
    internal Inventory MyInventory { get => _myInventory; set => _myInventory = value; }
    internal List<Spell> MySpells { get => _mySpells; set => _mySpells = value; }
    internal Potion PotionToBrew
    {
        get => _potionToBrew;
        set
        {
            _potionToBrew = value;
        }
    }

    public bool CanBrewPotion
    {
        get => _canBrewPotion;
        set
        {
            if (PotionToBrew != null)
            {
                if (MyInventory.Ingredients.Values.Count >= PotionToBrew.Recipe.NbIngredientsRequired)
                {
                    _canBrewPotion = MyInventory.Equals(PotionToBrew.Recipe.Ingredients);
                }
            }
            _canBrewPotion = false;
        }
    }
    internal int Score { get => _score; set => _score = value; }

    internal void Reset()
    {
        _previousState = this;
        _myInventory = new Inventory();
        _mySpells = new List<Spell>();
        _score = 0;
    }
}

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

class Potion : Order
{
    int _price;
    internal Potion(int id, string actionType, int delta0, int delta1, int delta2, int delta3, int price)
        : base(id, actionType, delta0, delta1, delta2, delta3)
    {
        _price = price;
    }
    internal int Price => _price;
}

static class DebugLogs
{
    internal static void WriteOrderIngredients(Order o)
    {
        if (o == null)
        {
            Console.Error.WriteLine("!!! Null Order !!!");
            return;
        }

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

    internal static void WriteMissingIngredients(Dictionary<Ingredient, int> missingIngredients)
    {
        StringBuilder sb = new StringBuilder($"Missing ingredients : \t");

        foreach (KeyValuePair<Ingredient, int> ingredient in missingIngredients)
        {
            sb.Append($"Ingredient{ingredient.Key.Type} {ingredient.Value}\t");
        }

        Console.Error.WriteLine(sb);
    }
}
