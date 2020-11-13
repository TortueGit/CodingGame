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
        MyGame game = new MyGame();

        // game loop
        while (true)
        {
            #region INIT_VALUES
            game.ResetMaxIngredient();

            List<Order> allOrders = new List<Order>();
            int actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
            for (int i = 0; i < actionCount; i++)
            {
                int bonus = 0;
                if (i == 0 && game.IsFirstTurn)
                    bonus = 3;
                if (i == 1 && game.IsFirstTurn)
                    bonus = 2;
                if (i == 2 && game.IsFirstTurn)
                    bonus = 1;

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
                    if (game.MaxIngredient0 < Math.Abs(delta0))
                        game.MaxIngredient0 = Math.Abs(delta0);
                    if (game.MaxIngredient1 < Math.Abs(delta1))
                        game.MaxIngredient1 = Math.Abs(delta1);
                    if (game.MaxIngredient2 < Math.Abs(delta2))
                        game.MaxIngredient2 = Math.Abs(delta2);
                    if (game.MaxIngredient3 < Math.Abs(delta3))
                        game.MaxIngredient3 = Math.Abs(delta3);

                    DebugLogs.WriteMaxIngredientsNeeded(game);
                }

                int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax
                int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell
                bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
                bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell

                Order o = new Order(actionId, actionType, delta0, delta1, delta2, delta3, price + bonus, tomeIndex, taxCount, castable, repeatable);
                
                allOrders.Add(o);
            }
            game.AddOrder(allOrders);

            game.MyWitch.MyInventory = new Inventory();
            inputs = Console.ReadLine().Split(' ');
            game.MyWitch.MyInventory.Ingredient0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory                
            game.MyWitch.MyInventory.Ingredient1 = int.Parse(inputs[1]);
            game.MyWitch.MyInventory.Ingredient2 = int.Parse(inputs[2]);
            game.MyWitch.MyInventory.Ingredient3 = int.Parse(inputs[3]);
            game.MyWitch.MyInventory.Score = int.Parse(inputs[4]); // amount of rupees

            game.OpponentWitch.MyInventory = new Inventory();
            inputs = Console.ReadLine().Split(' ');
            game.OpponentWitch.MyInventory.Ingredient0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory                
            game.OpponentWitch.MyInventory.Ingredient1 = int.Parse(inputs[1]);
            game.OpponentWitch.MyInventory.Ingredient2 = int.Parse(inputs[2]);
            game.OpponentWitch.MyInventory.Ingredient3 = int.Parse(inputs[3]);
            game.OpponentWitch.MyInventory.Score = int.Parse(inputs[4]); // amount of rupees
            #endregion


            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            string action = game.WhatToDo();

            Console.WriteLine(action);

            game.NewTurn();
        }
    }
}

class MyGame
{
    Witch _myWitch;
    Witch _opponentWitch;

    List<Order> _gamePotions;
    List<Order> _gameSpells;

    int _maxIngredient0;
    int _maxIngredient1;
    int _maxIngredient2;
    int _maxIngredient3;
    bool _isFirstTurn;

    public MyGame()
    {
        _myWitch = new Witch();
        _opponentWitch = new Witch();

        _gamePotions = new List<Order>();
        _gameSpells = new List<Order>();

        _isFirstTurn = true;
    }

    internal Witch MyWitch { get => _myWitch; set => _myWitch = value; }
    internal Witch OpponentWitch { get => _opponentWitch; set => _opponentWitch = value; }
    internal List<Order> GamePotions { get => _gamePotions; set => _gamePotions = value; }
    internal List<Order> GameSpells { get => _gameSpells; set => _gameSpells = value; }
    internal int MaxIngredient0 { get => _maxIngredient0; set => _maxIngredient0 = value; }
    internal int MaxIngredient1 { get => _maxIngredient1; set => _maxIngredient1 = value; }
    internal int MaxIngredient2 { get => _maxIngredient2; set => _maxIngredient2 = value; }
    internal int MaxIngredient3 { get => _maxIngredient3; set => _maxIngredient3 = value; }
    internal bool IsFirstTurn => _isFirstTurn;

    internal void NewTurn()
    {
        _myWitch.Reset();
        _opponentWitch.Reset();
        
        _gamePotions = new List<Order>();
        _gameSpells = new List<Order>();

        _isFirstTurn = false;
    }

    public void ResetMaxIngredient()
    {
        _maxIngredient0 = 0;
        _maxIngredient1 = 0;
        _maxIngredient2 = 0;
        _maxIngredient3 = 0;
    }

    internal void AddOrder(List<Order> allOrders)
    {
        _gamePotions.RemoveAll(x => !allOrders.Select(x => x.Id).Contains(x.Id));
        _gameSpells.RemoveAll(x => !allOrders.Select(x => x.Id).Contains(x.Id));

        foreach(Order order in allOrders)
        {
            switch (order.ActionType)
            {
                case Global.BREW:
                    if (!_gamePotions.Any(x => x.Id == order.Id))
                        _gamePotions.Add(order);
                    break;

                case Global.LEARN:
                    if (!_gameSpells.Any(x => x.Id == order.Id))
                        _gameSpells.Add(order);
                    break;
                
                case Global.OPPONENT_CAST:
                    _opponentWitch.MySpells.Add(order);
                    break;
                case Global.SPELL:
                    _myWitch.MySpells.Add(order);
                    break;
            }
        }
    }

    internal string WhatToDo()
    {
        int potionToBrewId = ChooseBestPotionsToBrew(_gamePotions);
        if (potionToBrewId != -1)
        {
            return $"{Global.BREW} {potionToBrewId}";
        }

        int spellToLearnId = IsItSpellToLearn(_gameSpells);
        if (spellToLearnId != -1 && _myWitch.MySpells.Count < 5)
        {
            return $"{Global.LEARN} {spellToLearnId}";
        }

        int spellId = WhatSpellToDo(_myWitch.MySpells.Where(x => x.Castable).ToList());
        if (spellId != -1)
        {
            return $"{Global.SPELL} {spellId}";
        }
        
        spellToLearnId = IsItSpellToLearn(_gameSpells);
        if (spellToLearnId != -1 && _myWitch.MySpells.Where(x => !x.Castable).Count() < 5)
        {
            return $"{Global.LEARN} {spellToLearnId}";
        }

        if (_myWitch.MySpells.Any(x => !x.Castable))
            return "REST";
        
        // We don't have anything to do ? Maybe there is still an action better than wait.
        if (_myWitch.MyInventory.Ingredient0 <= 0)
            spellId = DoFreeSpell(_myWitch.MySpells.Where(x => x.Castable && x.NbIngredientsCost == 0).ToList());
            if (spellId != -1)
            {
                return $"{Global.SPELL} {spellId}";
            }
        return "WAIT";
    }

    private int IsItSpellToLearn(List<Order> spellBooks)
    {
        Order spellToLearn = null;
        foreach(Order spell in spellBooks)
        {
            if (spell.NbIngredientsCost > spell.NbIngredientsAdd)
                continue;

            if (spellToLearn == null ||
                spellToLearn.NbIngredientsCost >= spell.NbIngredientsCost &&
                (spellToLearn.NbIngredientTypesAdd < spell.NbIngredientTypesAdd ||
                (spellToLearn.NbIngredientTypesAdd == spell.NbIngredientTypesAdd && spellToLearn.NbIngredientsAdd < spell.NbIngredientsAdd))
            )
            {
                if (spell.TomeIndex <= _myWitch.MyInventory.Ingredient0)
                    spellToLearn = spell;
            }
        }

        if (spellToLearn != null)
        {
            return spellToLearn.Id;
        }

        return -1;
    }

    private int ChooseBestPotionsToBrew(List<Order> potions)
    {
        Order potionToBrew = null;
        foreach (Order potion in potions)
        {
            if (CanIBrewPotions(potion.Id))
            {
                if ((potionToBrew == null || potionToBrew.Price < potion.Price) && potion.NbIngredientsCost <= potion.Price)
                    potionToBrew = potion;
            }
        }

        if (potionToBrew != null)
            return potionToBrew.Id;

        return -1;
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

    private bool CanIBrewPotions(int potionId)
    {
        Order potion = _gamePotions.Where(x => x.Id == potionId).First();

        if (_myWitch.MyInventory.Ingredient0 + potion.Ingredient0 >= 0 &&
            _myWitch.MyInventory.Ingredient1 + potion.Ingredient1 >= 0 &&
            _myWitch.MyInventory.Ingredient2 + potion.Ingredient2 >= 0 &&
            _myWitch.MyInventory.Ingredient3 + potion.Ingredient3 >= 0)
        {
            return true;
        }

        return false;
    }

    private bool CanICastSpell(int spellId)
    {
        Order spell = _myWitch.MySpells.Where(x => x.Id == spellId).First();
        if (_myWitch.MyInventory.Ingredient0 + spell.Ingredient0 >= 0 &&
            _myWitch.MyInventory.Ingredient1 + spell.Ingredient1 >= 0 &&
            _myWitch.MyInventory.Ingredient2 + spell.Ingredient2 >= 0 &&
            _myWitch.MyInventory.Ingredient3 + spell.Ingredient3 >= 0)
        {
            Console.Error.WriteLine($"!!!!!!! HELLO !!!!!!!!");
            // I can cast this spell, but does I need it ?
            if ((spell.NbIngredientsAdd - spell.NbIngredientsRequired) <= (10 - _myWitch.MyInventory.NbIngredients) &&
                (spell.Ingredient0 > 0 && (_maxIngredient0 - _myWitch.MyInventory.Ingredient0 > 0) ||
                spell.Ingredient1 > 0 && (_maxIngredient1 - _myWitch.MyInventory.Ingredient1 > 0) ||
                spell.Ingredient2 > 0 && (_maxIngredient2 - _myWitch.MyInventory.Ingredient2 > 0) ||
                spell.Ingredient3 > 0 && (_maxIngredient3 - _myWitch.MyInventory.Ingredient3 > 0))
            )
                return true;
        }

        DebugLogs.WriteOrderIngredients(spell);
        DebugLogs.WriteInventoryIngredients(_myWitch.MyInventory);
        DebugLogs.WriteMaxIngredientsNeeded(this);
        return false;
    }

    private int DoFreeSpell(List<Order> freeSpells)
    {
        if (freeSpells.Count > 0)
        {
            return freeSpells.OrderByDescending(x => x.NbIngredientTypesAdd).First().Id;
        }

        return -1;
    }
}

class Witch
{
    Witch _previousState;
    Inventory _myInventory;
    List<Order> _mySpells;
    int _score;    

    public Witch()
    {
        _myInventory = new Inventory();
        _mySpells = new List<Order>();
        _score = 0;
        _previousState = null;
    }

    internal Witch PreviousState { get => _previousState; set => _previousState = value; }
    internal Inventory MyInventory { get => _myInventory; set => _myInventory = value; }
    internal List<Order> MySpells { get => _mySpells; set => _mySpells = value; }
    public int Score { get => _score; set => _score = value; }

    internal void Reset()
    {
        _myInventory = new Inventory();
        _mySpells = new List<Order>();
        _score = 0;
        _previousState = this;
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
    int _nbIngredientsAdd;
    int _nbIngredientsCost;
    int _nbIngredientTypesAdd;
    int _nbIngredientTypesCost;
    int _nbIngredientsRequired;
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
    public int NbIngredientsAdd => _nbIngredientsAdd;
    public int NbIngredientsCost => _nbIngredientsCost;
    public int NbIngredientTypesAdd => _nbIngredientTypesAdd;
    public int NbIngredientTypesCost => _nbIngredientTypesCost;
    public int NbIngredientsRequired => _nbIngredientsRequired;
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
            _nbIngredientsRequired += Math.Abs(_ingredient0);
        }

        if (_ingredient1 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient1;
        }
        else if (_ingredient1 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient1) * 2;
            _nbIngredientsRequired += Math.Abs(_ingredient1);
        }

        if (_ingredient2 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient2;
        }
        else if (_ingredient2 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient2) * 3;
            _nbIngredientsRequired += Math.Abs(_ingredient2);
        }

        if (_ingredient3 > 0)
        {
            _nbIngredientTypesAdd++;
            _nbIngredientsAdd += _ingredient3;
        }
        else if (_ingredient3 < 0)
        {
            _nbIngredientTypesCost++;
            _nbIngredientsCost += Math.Abs(_ingredient3) * 4;
            _nbIngredientsRequired += Math.Abs(_ingredient3);
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

    internal static void WriteMaxIngredientsNeeded(MyGame game)
    {
        StringBuilder sb = new StringBuilder($"Max ingredients needed : \t");
        sb.Append($"Ingredient0[{game.MaxIngredient0}] \t");
        sb.Append($"Ingredient2[{game.MaxIngredient1}] \t");
        sb.Append($"Ingredient2[{game.MaxIngredient2}] \t");
        sb.Append($"Ingredient3[{game.MaxIngredient3}] \t");

        Console.Error.WriteLine(sb);
    }
}
