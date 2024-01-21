namespace CodingGame.Contest.FallChallenge.GameWorkspace
{
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
}
