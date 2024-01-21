using System.Collections.Generic;
using CodingGame.Contest.FallChallenge.src.game.spell;

namespace CodingGame.Contest.FallChallenge.src.game
{
    public class Deck
    {
        private LinkedList<TomeSpell> _tome = new LinkedList<TomeSpell>();
        private LinkedList<DeliverySpell> _deliveries = new LinkedList<DeliverySpell>();

        public Deck()
        {
            _tome.AddLast(new TomeSpell(new Recipe(-3, 0, 0, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(3, -1, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(1, 1, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 0, 1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(3, 0, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(2, 3, -2, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(2, 1, -2, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(3, 0, 1, -1)));
            _tome.AddLast(new TomeSpell(new Recipe(3, -2, 1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(2, -3, 2, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(2, 2, 0, -1)));
            _tome.AddLast(new TomeSpell(new Recipe(-4, 0, 2, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(2, 1, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(4, 0, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 0, 0, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 2, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(1, 0, 1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(-2, 0, 1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(-1, 0, -1, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 2, -1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(2, -2, 0, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(-3, 1, 1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 2, -2, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(1, -3, 1, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 3, 0, -1)));
            _tome.AddLast(new TomeSpell(new Recipe(0, -3, 0, 2)));
            _tome.AddLast(new TomeSpell(new Recipe(1, 1, 1, -1)));
            _tome.AddLast(new TomeSpell(new Recipe(1, 2, -1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(4, 1, -1, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(-5, 0, 0, 2)));
            _tome.AddLast(new TomeSpell(new Recipe(-4, 0, 1, 1)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 3, 2, -2)));
            _tome.AddLast(new TomeSpell(new Recipe(1, 1, 3, -2)));
            _tome.AddLast(new TomeSpell(new Recipe(-5, 0, 3, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(-2, 0, -1, 2)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 0, -3, 3)));
            _tome.AddLast(new TomeSpell(new Recipe(0, -3, 3, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(-3, 3, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(-2, 2, 0, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 0, -2, 2)));
            _tome.AddLast(new TomeSpell(new Recipe(0, -2, 2, 0)));
            _tome.AddLast(new TomeSpell(new Recipe(0, 0, 2, -1)));

            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 2, 0, 0), 6));
            _deliveries.AddLast(new DeliverySpell(new Recipe(3, 2, 0, 0), 7));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 4, 0, 0), 8));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 0, 2, 0), 8));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 3, 0, 0), 8));
            _deliveries.AddLast(new DeliverySpell(new Recipe(3, 0, 2, 0), 9));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 2, 2, 0), 10));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 5, 0, 0), 10));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 0, 0, 2), 10));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 0, 3, 0), 11));
            _deliveries.AddLast(new DeliverySpell(new Recipe(3, 0, 0, 2), 11));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 4, 0), 12));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 2, 0, 2), 12));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 3, 2, 0), 12));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 2, 3, 0), 13));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 2, 2), 14));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 3, 0, 2), 14));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 0, 0, 3), 14));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 5, 0), 15));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 0, 4), 16));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 2, 0, 3), 16));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 3, 2), 17));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 2, 3), 18));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 0, 0, 5), 20));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 1, 0, 1), 9));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 2, 1, 1), 12));
            _deliveries.AddLast(new DeliverySpell(new Recipe(1, 0, 2, 1), 12));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 2, 2, 0), 13));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 2, 0, 2), 15));
            _deliveries.AddLast(new DeliverySpell(new Recipe(2, 0, 2, 2), 17));
            _deliveries.AddLast(new DeliverySpell(new Recipe(0, 2, 2, 2), 19));
            _deliveries.AddLast(new DeliverySpell(new Recipe(1, 1, 1, 1), 12));
            _deliveries.AddLast(new DeliverySpell(new Recipe(3, 1, 1, 1), 14));
            _deliveries.AddLast(new DeliverySpell(new Recipe(1, 3, 1, 1), 16));
            _deliveries.AddLast(new DeliverySpell(new Recipe(1, 1, 3, 1), 18));
            _deliveries.AddLast(new DeliverySpell(new Recipe(1, 1, 1, 3), 20));
        }

        public LinkedList<TomeSpell> Tome { get => _tome; set => _tome = value; }
        public LinkedList<DeliverySpell> Deliveries { get => _deliveries; set => _deliveries = value; }
    }
}
