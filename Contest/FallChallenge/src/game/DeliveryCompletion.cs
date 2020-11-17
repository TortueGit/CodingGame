using CodingGame.Contest.FallChallenge.src.game.spell;

namespace CodingGame.Contest.FallChallenge.src.game
{    
    public class DeliveryCompletion
    {
        private DeliverySpell _delivery;
        private int _index, _earned;

        public DeliveryCompletion(DeliverySpell delivery, int index, int earned)
        {
            _delivery = delivery;
            _index = index;
            _earned = earned;
        }

        public DeliverySpell Delivery => _delivery;
        public int Index => _index;
        public int Earned => _earned;
    }
}
