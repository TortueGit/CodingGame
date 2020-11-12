using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

static class Global
{
    public const string SPELL = "CAST";
    public const string OPPONENT_CAST = "OPPONENT_CAST";
    public const string LEARN = "LEARN";
    public const string BREW = "BREW";

    public static int MaxIngredient0;
    public static int MaxIngredient1;
    public static int MaxIngredient2;
    public static int MaxIngredient3;

    public static void ResetMaxIngredient()
    {
        MaxIngredient0 = 0;
        MaxIngredient1 = 0;
        MaxIngredient2 = 0;
        MaxIngredient3 = 0;
    }
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

        // game loop
        while (true)
        {
            MyGame game = new MyGame();
            int actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
            for (int i = 0; i < actionCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int actionId = int.Parse(inputs[0]); // the unique ID of this spell or recipe
                string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
                
                int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change                
                if (Global.MaxIngredient0 < delta0)
                    Global.MaxIngredient0 = delta0;
                int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
                if (Global.MaxIngredient1 < delta1)
                    Global.MaxIngredient1 = delta1;
                int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
                if (Global.MaxIngredient2 < delta2)
                    Global.MaxIngredient2 = delta2;
                int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
                if (Global.MaxIngredient3 < delta3)
                    Global.MaxIngredient3 = delta3;

                int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
                int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax
                int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell
                bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
                bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell

                Order o = new Order(actionId, actionType, delta0, delta1, delta2, delta3, price, tomeIndex, taxCount, castable, repeatable);
                game.GameOrders.Add(o);
            }

            game.MyInventory = new Inventory();
            inputs = Console.ReadLine().Split(' ');
            game.MyInventory.Ingredient0 += int.Parse(inputs[0]); // tier-0 ingredients in inventory                
            game.MyInventory.Ingredient1 += int.Parse(inputs[1]);
            game.MyInventory.Ingredient2 += int.Parse(inputs[2]);
            game.MyInventory.Ingredient3 += int.Parse(inputs[3]);
            game.MyInventory.Score += int.Parse(inputs[4]); // amount of rupees

            game.OpponentInventory = new Inventory();
            inputs = Console.ReadLine().Split(' ');
            game.OpponentInventory.Ingredient0 += int.Parse(inputs[0]); // tier-0 ingredients in inventory                
            game.OpponentInventory.Ingredient1 += int.Parse(inputs[1]);
            game.OpponentInventory.Ingredient2 += int.Parse(inputs[2]);
            game.OpponentInventory.Ingredient3 += int.Parse(inputs[3]);
            game.OpponentInventory.Score += int.Parse(inputs[4]); // amount of rupees

            Console.Error.WriteLine($"inv0 = {game.MyInventory.Ingredient0} && inv1 = {game.MyInventory.Ingredient1} && inv2 = {game.MyInventory.Ingredient2} && inv3 = {game.MyInventory.Ingredient3}");

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            string action = game.WhatToDo();

            Console.WriteLine(action);
        }
    }
}

class MyGame
{
    Inventory _myInventory;
    Inventory _opponentInventory;
    List<Order> _gameOrders;

    Inventory _simulInventory;
    List<int> _bestPotionsToBrew;
    List<int> _simulPotionsToBrew;
    int _simulScore;

    public MyGame()
    {
        Global.ResetMaxIngredient();
        _myInventory = new Inventory();
        _gameOrders = new List<Order>();
        
        _simulInventory = new Inventory();
        _simulScore = 0;
    }

    internal Inventory MyInventory 
    { 
        get => _myInventory; 
        set 
        {
            _myInventory = value;
            _simulInventory = value;
        }
    }
    internal Inventory OpponentInventory { get => _opponentInventory; set => _opponentInventory = value; }
    internal List<Order> GameOrders { get => _gameOrders; set => _gameOrders = value; }
    internal List<int> BestPotionsToBrew => _bestPotionsToBrew;

    internal string WhatToDo()
    {
        ChooseBestPotionsToBrew(_gameOrders.Where(x => x.ActionType.Equals(Global.BREW)).ToList());
        if (_bestPotionsToBrew != null)
        {
            Console.Error.WriteLine($"_bestPotionsToBrew.Count {_bestPotionsToBrew.Count}");
            return $"{Global.BREW} {_bestPotionsToBrew.First()}";
        }

        int spellToLearnId = IsItSpellToLearn(_gameOrders.Where(x => x.ActionType.Equals(Global.LEARN)).ToList());
        if (spellToLearnId != -1 && _gameOrders.Where(x => x.ActionType.Equals(Global.SPELL)).Count() < 5)
        {
            return $"{Global.LEARN} {spellToLearnId}";
        }

        int spellId = WhatSpellToDo(_gameOrders.Where(x => x.ActionType.Equals(Global.SPELL) && x.Castable == true).ToList());
        if (spellId != -1)
        {
            return $"{Global.SPELL} {spellId}";
        }
        
        spellToLearnId = IsItSpellToLearn(_gameOrders.Where(x => x.ActionType.Equals(Global.LEARN)).ToList());
        if (spellToLearnId != -1)
        {
            return $"{Global.LEARN} {spellToLearnId}";
        }

        return "REST";
    }

    private int IsItSpellToLearn(List<Order> spellBooks)
    {
        Order spellToLearn = null;
        foreach(Order spell in spellBooks)
        {
            if (spellToLearn == null ||
                spellToLearn.Cost >= spell.Cost &&
                (spellToLearn.NbIngredientTypesAdd < spell.NbIngredientTypesAdd ||
                (spellToLearn.NbIngredientTypesAdd == spell.NbIngredientTypesAdd && spellToLearn.NbIngredientsAdd < spell.NbIngredientsAdd))
            )
            {
                if (spell.TomeIndex <= _myInventory.Ingredient0)
                    spellToLearn = spell;
            }
        }

        if (spellToLearn != null)
            return spellToLearn.Id;

        return -1;
    }

    private void ChooseBestPotionsToBrew(List<Order> potions)
    {
        for (int i = 0; i < potions.Count(); i++)
        {
            int turnPrice = 0;
            Order potion = potions[i];

            if (CanIBrewPotions(potion.Id))
            {
                SimulBrewery(potion);
                
                int indexNextPotionToBrew = i + 1;
                while (indexNextPotionToBrew <= potions.Count)
                {
                    _simulPotionsToBrew = new List<int>();
                    _simulPotionsToBrew.Add(potion.Id);
                    Console.Error.WriteLine($"!!!!!!!!!!!!! _simulPotionsToBrew.Count {_simulPotionsToBrew.Count} !!!!!!!!!!!");
                    turnPrice = potion.Price;
                    // I can brew this potion, but can I brew others with it ?
                    for (int j = indexNextPotionToBrew; j < potions.Count; j++)
                    {
                        Order pot = potions[j];
                        if (CanIBrewPotions(pot.Id))
                        {
                            SimulBrewery(pot);
                            turnPrice += pot.Price;
                            _simulPotionsToBrew.Add(pot.Id);
                        }
                    }

                    Console.Error.WriteLine($"simulScore = {_simulScore} |  turnPrice = {turnPrice}");
                    if (_simulScore < turnPrice)
                    {
                        _simulScore = turnPrice;
                        Console.Error.WriteLine($"_simulPotionsToBrew.Count {_simulPotionsToBrew.Count}");
                        _bestPotionsToBrew = new List<int>(_simulPotionsToBrew);
                        Console.Error.WriteLine($"_bestPotionsToBrew.Count {_bestPotionsToBrew.Count}");
                    }

                    indexNextPotionToBrew++;
                }
            }
        }
    }

    private int WhatSpellToDo(List<Order> spells)
    {
        foreach(Order spell in spells)
        {
            if (CanICastSpell(spell.Id))
            {
                return spell.Id;
            }
        }

        return -1;
    }

    private void SimulBrewery(Order potion)
    {
        _simulInventory.Ingredient0 += potion.Ingredient0;
        _simulInventory.Ingredient1 += potion.Ingredient1;
        _simulInventory.Ingredient2 += potion.Ingredient2;
        _simulInventory.Ingredient3 += potion.Ingredient3;
    }

    private bool CanIBrewPotions(int potionId)
    {
        Order potion = _gameOrders.Where(x => x.Id == potionId).First();

        if (_simulInventory.Ingredient0 + potion.Ingredient0 >= 0 &&
            _simulInventory.Ingredient1 + potion.Ingredient1 >= 0 &&
            _simulInventory.Ingredient2 + potion.Ingredient2 >= 0 &&
            _simulInventory.Ingredient3 + potion.Ingredient3 >= 0)
        {
            DebugLogs.WriteInventoryIngredients(_simulInventory);
            DebugLogs.WriteOrderIngredients(potion);
            return true;
        }

        return false;
    }

    private bool CanICastSpell(int spellId)
    {
        Order spell = _gameOrders.Where(x => x.Id == spellId).First();
        if (_myInventory.Ingredient0 + spell.Ingredient0 >= 0 &&
            _myInventory.Ingredient1 + spell.Ingredient1 >= 0 &&
            _myInventory.Ingredient2 + spell.Ingredient2 >= 0 &&
            _myInventory.Ingredient3 + spell.Ingredient3 >= 0)
        {
            // I can cast this spell, but does I need it ?
            if (spell.Ingredient0 > 0 && (Global.MaxIngredient0 - _myInventory.Ingredient0 >= 0) && _myInventory.NbIngredients < 8 ||
                spell.Ingredient1 > 0 && (Global.MaxIngredient1 - _myInventory.Ingredient1 >= 0) ||
                spell.Ingredient2 > 0 && (Global.MaxIngredient2 - _myInventory.Ingredient2 >= 0) ||
                spell.Ingredient3 > 0 && (Global.MaxIngredient3 - _myInventory.Ingredient3 >= 0)
            )
                return true;
        }

        return false;
    }
}

class Inventory
{
    int _ingredient0, _ingredient1, _ingredient2, _ingredient3;

    int _nbIngredients;

    int _score;

    public Inventory()
    {
        _ingredient0 = 0;
        _ingredient1 = 0;
        _ingredient2 = 0;
        _ingredient3 = 0;
        _score = 0;
    }

    public int Ingredient0 { get => _ingredient0; set => _ingredient0 = value; }
    public int Ingredient1 { get => _ingredient1; set => _ingredient1 = value; }
    public int Ingredient2 { get => _ingredient2; set => _ingredient2 = value; }
    public int Ingredient3 { get => _ingredient3; set => _ingredient3 = value; }
    public int Score { get => _score; set => _score = value; }

    public int NbIngredients => Ingredient0 + Ingredient1 + Ingredient2 + Ingredient3;
}

class Order
{
    int _id;
    string _actionType;
    int _ingredient0, _ingredient1, _ingredient2, _ingredient3;
    int _price;
    int _tomeIndex;
    int _taxCount;
    int _cost;
    int _nbIngredientsAdd;
    int _nbIngredientsCost;
    int _nbIngredientTypesAdd;
    int _nbIngredientTypesCost;
    bool _castable;
    bool _repeatable;

    public Order(int id, string actionType, int delta0, int delta1, int delta2, int delta3, int price, int tomeIndex, int taxCount, bool castable, bool repeatable)
    {
        _id = id;
        _actionType = actionType;
        _ingredient0 = delta0;
        _ingredient1 = delta1;
        _ingredient2 = delta2;
        _ingredient3 = delta3;
        _price = price;
        _tomeIndex = tomeIndex;
        _taxCount = taxCount;
        _castable = castable;
        _repeatable = repeatable;
        
        Calculate();
    }

    public int Id => _id;
    public string ActionType => _actionType;
    public int Ingredient0 => _ingredient0;
    public int Ingredient1 => _ingredient1;
    public int Ingredient2 => _ingredient2;
    public int Ingredient3 => _ingredient3;
    public int Price => _price;
    public int TomeIndex => _tomeIndex;
    public int TaxCount => _taxCount;
    public int Cost => _cost;
    public int NbIngredientsAdd => _nbIngredientsAdd;
    public int NbIngredientsCost => _nbIngredientsCost;
    public int NbIngredientTypesAdd => _nbIngredientTypesAdd;
    public int NbIngredientTypesCost => _nbIngredientTypesCost;
    public bool Castable => _castable;
    public bool Reapeatable => _repeatable;    

    private void Calculate()
    {
        if (_ingredient0 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient0;
        }
        else if (_ingredient0 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient0);
        }

        if (_ingredient1 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient1;
        }
        else if (_ingredient1 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient1);
        }

        if (_ingredient2 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient2;
        }
        else if (_ingredient2 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient2);
        }

        if (_ingredient3 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient3;
        }
        else if (_ingredient3 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient3);
        }
    }
}

static class DebugLogs
{
    internal static void WriteOrderIngredients(Order o)
    {
        StringBuilder sb = new StringBuilder($"Order id[{o.Id}] actionType[{o.ActionType}] \t");
        sb.Append($"Ingredient0[{o.Ingredient0}] \t");
        sb.Append($"Ingredient1[{o.Ingredient1}] \t");
        sb.Append($"Ingredient2[{o.Ingredient2}] \t");
        sb.Append($"Ingredient3[{o.Ingredient3}]");

        Console.Error.WriteLine(sb);
    }

    internal static void WriteInventoryIngredients(Inventory inventory)
    {
        StringBuilder sb = new StringBuilder($"Inventory ingredients : \t");
        sb.Append($"Ingredient0[{inventory.Ingredient0}] \t");
        sb.Append($"Ingredient1[{inventory.Ingredient1}] \t");
        sb.Append($"Ingredient2[{inventory.Ingredient2}] \t");
        sb.Append($"Ingredient3[{inventory.Ingredient3}]");

        Console.Error.WriteLine(sb);
    }
}
