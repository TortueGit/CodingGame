namespace CodingGame.Contest.FallChallenge.src.view
{
    public class BonusData 
    {
        int _value;
        int _amount;

        public BonusData(int amount, int value) {
            _value = value;
            _amount = amount;
        }

        internal int Value { get => _value; set => _value = value; }
        internal int Amount { get => _amount; set => _amount = value; }
    }
}
