namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
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
}
