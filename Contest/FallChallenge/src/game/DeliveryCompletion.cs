using CodingGame.Contest.FallChallenge.src.game.spell;

namespace CodingGame.Contest.FallChallenge.src.game
{
    public class DeliveryCompletion
    {
        private DeliverySpell delivery;
        private int index, earned;

        public DeliveryCompletion(DeliverySpell delivery, int index, int earned)
        {
            this.delivery = delivery;
            this.index = index;
            this.earned = earned;
        }

        public DeliverySpell getDelivery()
        {
            return delivery;
        }

        public int getIndex()
        {
            return index;
        }

        public int getEarned()
        {
            return earned;
        }
    }
}
