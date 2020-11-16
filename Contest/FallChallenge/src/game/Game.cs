using System;
using System.Collections.Generic;
using System.Linq;
using CodingGame.Contest.FallChallenge.src.game.spell;
using CodingGame.Contest.FallChallenge.src.view;

namespace CodingGame.Contest.FallChallenge.src.game
{
    //@Singleton
    public class Game
    {
        public List<object> toto = new List<object>();

        public const int READ_AHEAD_COST = 1;
        public const int STARTING_SCORE = 0;
        public const int MAX_SPACE = 10;
        public const int INGREDIENT_TYPE_COUNT = 4;
        public const int COUNTER_SIZE = 5;
        public const int TOME_SIZE = 6;

        public static int[] STARTING_INGREDIENT_COUNT;
        public static int DELIVERY_GOAL;
        public static bool ACTIVE_TOME;
        public static bool ACTIVE_SPELLS;
        public static bool ACTIVE_BONUS;
        public static bool INVENTORY_BONUS;

        // @Inject private MultiplayerGameManager<Player> gameManager;
        // @Inject private GameSummaryManager gameSummaryManager;

        private List<PlayerWitch> _players = new List<PlayerWitch>(new PlayerWitch(), new PlayerWitch());

        private List<TomeSpell> _tome;
        private List<DeliverySpell> _deliveries;
        private List<DeliverySpell> _newDeliveries;
        private List<TomeSpell> _newTomeSpells;
        private Random _random;
        private int[] _tomeStockGain;
        private Deck _deck;
        private int[] _bonus;
        private int[] _bonusValue = new int[] { 3, 1 };
        private int _frameDuration;
        HashSet<TomeSpell> _learntSpells = new HashSet<TomeSpell>();
        HashSet<DeliveryCompletion> _delivered = new HashSet<DeliveryCompletion>();
        private List<EventData> _viewerEvents;

        private void SlideBonus()
        {
            if (_bonus[0] <= 0)
            {
                _bonus[0] = _bonus[1];
                _bonus[1] = 0;
                _bonusValue[0] = 1;
                _bonusValue[1] = 0;
            }
        }

        private int GetScoreOf(DeliverySpell delivery)
        {
            int index = _deliveries.IndexOf(delivery);
            int bonusScore = 0;
            if (index < 2)
            {
                if (_bonus[index] > 0)
                {
                    bonusScore = _bonusValue[index];
                }
            }

            return delivery.GetScore() + bonusScore;
        }

        public void Init(long seed)
        {
            _deck = new Deck();

            // switch (gameManager.getLeagueLevel())
            // {
            //     case 1:
            //         // Wood 2
            //         STARTING_INGREDIENT_COUNT = new int[] { 2, 2, 3, 3 };
            //         DELIVERY_GOAL = 2;
            //         ACTIVE_TOME = false;
            //         ACTIVE_SPELLS = false;
            //         ACTIVE_BONUS = false;
            //         INVENTORY_BONUS = false;
            //         deck.deliveries.stream()
            //             .filter(del => {
            //             return IntStream.of(del.getDelta())
            //                 .anyMatch(count->count < -1);
            //         })
            //     .forEach(del=> {
            //             for (int i = 0; i < INGREDIENT_TYPE_COUNT; ++i)
            //             {
            //                 del.getDelta()[i] /= 2;
            //             }
            //         });
            //         Set<Recipe> deltas = new HashSet<>();
            //         deck.deliveries = deck.deliveries.stream()
            //             .filter(del->deltas.add(del.recipe))
            //             .collect(Collectors.toCollection(LinkedList::new));
            //         break;
            //     case 2:
            //         // Wood 1
            //         STARTING_INGREDIENT_COUNT = new int[] { 3, 0, 0, 0 };
            //         DELIVERY_GOAL = 3;
            //         ACTIVE_TOME = false;
            //         ACTIVE_SPELLS = true;
            //         ACTIVE_BONUS = false;
            //         INVENTORY_BONUS = true;
            //         break;
            //     default:
            //         // Bronze+
            //         STARTING_INGREDIENT_COUNT = new int[] { 3, 0, 0, 0 };
            //         DELIVERY_GOAL = 6;
            //         ACTIVE_TOME = true;
            //         ACTIVE_SPELLS = true;
            //         ACTIVE_BONUS = true;
            //         INVENTORY_BONUS = true;
            // }

            // Bronze+
            STARTING_INGREDIENT_COUNT = new int[] { 3, 0, 0, 0 };
            DELIVERY_GOAL = 6;
            ACTIVE_TOME = true;
            ACTIVE_SPELLS = true;
            ACTIVE_BONUS = true;
            INVENTORY_BONUS = true;

            _bonus = ACTIVE_BONUS ? new int[] { 4, 4 } : new int[] { 0, 0 };
            _viewerEvents = new List<EventData>();
            _random = new Random(seed);

            Collections.shuffle(deck.tome, random);
            Collections.shuffle(deck.deliveries, random);

            _tome = new List<TomeSpell>();
            _deliveries = new List<DeliverySpell>();
            _newDeliveries = new List<DeliverySpell>();
            _newTomeSpells = new List<TomeSpell>();
            _tomeStockGain = new int[Game.TOME_SIZE];

            if (ACTIVE_TOME)
            {
                for (int i = 0; i < TOME_SIZE; ++i)
                {
                    _tome.Add(_deck.Tome.First);
                    _deck.Tome.RemoveFirst();
                    _tomeStockGain[i] = 0;
                }
            }
            for (int i = 0; i < COUNTER_SIZE; ++i)
            {
                _deliveries.Add(_deck.Deliveries.First);
                _deck.Deliveries.RemoveFirst();
            }

            foreach (PlayerWitch p in _players)
            {
                for (int i = 0; i < INGREDIENT_TYPE_COUNT; ++i)
                {
                    p.GetInventory().delta[i] = STARTING_INGREDIENT_COUNT[i];
                }
                p.SetScore(STARTING_SCORE);
                if (ACTIVE_SPELLS)
                {
                    p.InitSpells();
                }
            }
        }

        public int GetBonusValue(DeliverySpell spell)
        {
            if (GetBonusAmount(spell) == 0)
            {
                return 0;
            }

            int index = _deliveries.indexOf(spell);

            if (index < 0 || index > 1)
            {
                return 0;
            }

            return _bonusValue[index];
        }

        public int GetBonusAmount(DeliverySpell spell)
        {
            int index = _deliveries.IndexOf(spell);

            if (index < 0 || index > 1)
            {
                return 0;
            }

            return _bonus[index];
        }

        public List<String> GetCurrentFrameInfoFor(PlayerWitch player)
        {
            PlayerWitch foe = gameManager.getPlayer(1 - player.GetIndex());

            List<String> lines = new List<string>();
            lines.Add((_deliveries.Count + _tome.Count + player.GetSpells().Count + foe.GetSpells().Count).ToString());

            _deliveries.ForEach(spell =>
            {
                lines.Add(
                    String.Format(
                        "%d %s %s %d %d %d %d %d",
                        spell.GetId(),
                        SpellType.BREW.Name(),
                        spell.recipe.ToPlayerString(),
                        GetScoreOf(spell),
                        GetBonusValue(spell),
                        GetBonusAmount(spell),
                        spell.IsActive() ? 1 : 0,
                        spell.IsRepeatable() ? 1 : 0
                    )
                );
            });

            var toto = (
                _tome,
                player.GetSpells(),
                foe.GetSpells()
            ).ForEach(spell =>
            {
                lines.Add(
                    String.Format(
                        "%d %s %s %d %d %d %d %d",
                        spell.GetId(),
                        GetSpellType(spell, player).Name(),
                        spell.Recipe.ToPlayerString(),
                        GetScoreOf(spell),
                        _tome.IndexOf(spell),
                        spell.GetStock(),
                        spell.IsActive() ? 1 : 0,
                        spell.IsRepeatable() ? 1 : 0
                    )
                );
            });

            // scores
            var titi = (player, foe).ForEach(p =>
            {
                lines.Add(String.Format("%s %d", p.GetInventory().ToPlayerString(), p.GetScore()));
            });

            return lines;
        }

        private int GetScoreOf(Spell spell)
        {
            if (spell.GetType() == typeof(DeliverySpell))
            {
                return GetScoreOf((DeliverySpell)spell);
            }
            return spell.GetScore();
        }

        private SpellType GetSpellType(Spell spell, PlayerWitch player)
        {
            if (spell.GetType() == typeof(TomeSpell))
            {
                return SpellType.LEARN;
            }
            else if (spell.GetType() == typeof(DeliverySpell))
            {
                return SpellType.BREW;
            }
            else
            {
                if (spell.IsOwner(player))
                {
                    return SpellType.CAST;
                }
                else
                {
                    return SpellType.OPPONENT_CAST;

                }
            }
        }

        public void CheckSpellActionType(Action action, SpellType type)
        {
            String expectedStr = null;

            switch (type)
            {
                case LEARN:
                    expectedStr = "LEARN";
                    break;
                case CAST:
                    expectedStr = "CAST";
                    break;
                case BREW:
                    expectedStr = "BREW";
                    break;
                case OPPONENT_CAST:
                    throw new GameException("Tried to cast an opponent's spell...");
            }

            if (!action.GetStr().Equals(expectedStr))
            {
                throw new GameException(String.Format("Command does not match action, expected '%s' but got '%s'", expectedStr, action.GetStr()));
            }
        }

        public void PerformGameUpdate()
        {
            foreach (PlayerWitch player in _players)
            {
                Optional<Spell> optSpell = Optional.Empty();
                try
                {
                    Action action = player.GetAction();
                    if (action.IsSpell())
                    {
                        int id = action.SpellId;
                        optSpell = GetSpellById(id);
                        if (optSpell.IsPresent())
                        {
                            Spell spell = optSpell.Get();
                            CheckSpellActionType(action, GetSpellType(spell, player));
                            if (spell.GetType() == typeof(TomeSpell))
                            {
                                DoLearn(player, (TomeSpell)spell);
                                gameSummaryManager.AddPlayerSpellLearn(player, spell);
                            }
                            else if (spell.GetType() == typeof(DeliverySpell))
                            {
                                DeliveryCompletion delComplete = DoDelivery(player, (DeliverySpell)spell);
                                gameManager.AddTooltip(
                                    new Tooltip(
                                        player.GetIndex(),
                                        String.Format(
                                            "%s made a delivery!",
                                            player.GetNicknameToken()
                                        )
                                    )
                                );
                                gameSummaryManager.AddPlayerDelivery(player, delComplete);
                            }
                            else if (spell.GetType() == typeof(PlayerSpell))
                            {
                                DoPlayerSpell(player, (PlayerSpell)spell);
                                gameSummaryManager.AddPlayerSpellAction(player, spell);
                            }
                        }
                        else
                        {
                            throw new GameException(String.Format("Spell with id %d does not exist", id));
                        }
                    }
                    else if (action.IsReset())
                    {
                        DoReset(player);
                        gameSummaryManager.AddPlayerResetAction(player);
                    }
                    else if (action.IsWait())
                    {
                        gameSummaryManager.AddPlayerWaitAction(player);
                    }
                }
                catch (GameException gameException)
                {
                    gameSummaryManager.AddPlayerInvalidAction(player, gameException.GetMessage());
                }

            }

            foreach (TomeSpell spell in _learntSpells)
            {
                _tome.Remove(spell);
                if (!_deck.Tome.IsEmpty())
                {
                    TomeSpell newSpell = _deck.Tome.First;
                    _deck.Tome.RemoveFirst();
                    _tome.Add(newSpell);
                    _newTomeSpells.Add(newSpell);
                }
            }

            foreach (DeliveryCompletion delCompletion in _delivered)
            {
                if (delCompletion.GetIndex() < _bonus.length)
                {
                    if (_bonus[delCompletion.GetIndex()] > 0)
                    {
                        _bonus[delCompletion.GetIndex()]--;
                    }
                }
            }

            _delivered.Select(x => x.GetDelivery()).Distinct().ForEach(delivery =>
            {
                _deliveries.Remove(delivery);
                if (!_deck.Deliveries.IsEmpty())
                {
                    DeliverySpell newDelivery = _deck.Deliveries.First;
                    _deck.Deliveries.RemoveFirst();
                    _deliveries.Add(newDelivery);
                    _newDeliveries.Add(newDelivery);
                }
            });

            SlideBonus();

            for (int i = 0; i < _tome.Count; i++)
            {
                _tome[i].Stock += _tomeStockGain[i];
                _tomeStockGain[i] = 0;
            }

            ComputeEvents();

            if (GameOver())
            {
                gameManager.EndGame();
            }

            gameManager.AddToGameSummary(gameSummaryManager.GetSummary());
        }

        private void ChainAnims(
            int count, List<AnimationData> animData, int[] timeArray, int timeIdx, int duration, int separation, Integer triggerAfter, int triggerDuration
        )
        {
            for (int i = 0; i < count; ++i)
            {
                animData.Add(new AnimationData(timeArray[timeIdx], duration, triggerAfter, triggerDuration));

                if (i < count - 1)
                {
                    timeArray[timeIdx] += separation;
                }
            }
            if (count > 0)
            {
                timeArray[timeIdx] += duration;
            }
        }

        private void ChainAnims(int count, List<AnimationData> animData, int[] timeArray, int timeIdx, int duration, int separation)
        {
            for (int i = 0; i < count; ++i)
            {
                animData.add(new AnimationData(timeArray[timeIdx], duration));

                if (i < count - 1)
                {
                    timeArray[timeIdx] += separation;
                }
            }
            if (count > 0)
            {
                timeArray[timeIdx] += duration;
            }
        }

        private void ComputeEvents()
        {
            int[] playerTime = new int[] { 0, 0 };

            int? lastTriggerEnd = null;
            List<EventData> learnEvents = new List<EventData>();

            foreach(EventData event in _viewerEvents)
            {
            if (event.Type == EventData.PLAYER_SPELL) {
            Spell spell = GetSpellById(event.SpellId).Get();
                event.AnimData = new List<AnimationData>();

        int loss = spell.Recipe.GetTotalLoss() * event.Repeats;
        int gain = spell.Recipe.GetTotalGain() * event.Repeats;

        chainAnims(
            loss, event.animData, playerTime, event.playerIdx, AnimationData.SHELF_TO_POT_DURATION, AnimationData.SHELF_TO_POT_SEPERATION,
            AnimationData.SHELF_TO_POT_DURATION, AnimationData.SPLASH_DURATION
        );
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.STIR_DURATION);
        chainAnims(
            gain, event.animData, playerTime, event.playerIdx, AnimationData.POT_TO_SHELF_DURATION, AnimationData.POT_TO_SHELF_SEPERATION
        );

        } else if (event.type == EventData.DELIVERY) {
            Spell spell = getSpellById(event.spellId).get();
                event.animData = new ArrayList<>();

        int loss = spell.recipe.getTotalLoss();
        chainAnims(
            loss, event.animData, playerTime, event.playerIdx, AnimationData.SHELF_TO_POT_DURATION, AnimationData.SHELF_TO_POT_SEPERATION,
            AnimationData.SHELF_TO_POT_DURATION, AnimationData.SPLASH_DURATION
        );
        lastTriggerEnd = playerTime [event.playerIdx] + AnimationData.SPLASH_DURATION;
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.STIR_DURATION);
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.POTION_SPAWN_DURATION);
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.POTION_TO_DELIVERY_DURATION);
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.DELIVERY_FADE_DURATION);

        } else if (event.type == EventData.LEARN) {
                event.animData = new ArrayList<>();
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.TOME_TO_LEARNT_DURATION);
        chainAnims(
                    event.acquired + event.lost, event.animData, playerTime, event.playerIdx, AnimationData.TOME_TO_SHELF_DURATION,
            AnimationData.TOME_TO_SHELF_DURATION
        );
        learnEvents.add(event);
        } else if (event.type == EventData.RESET) {
                event.animData = new ArrayList<>();
        waitForAnim(event.animData, playerTime, event.playerIdx, AnimationData.RESET_DURATION);
        }
    }

if (learnEvents.size() >= 2)
{
    int currentTime = IntStream.of(playerTime).max().getAsInt();
    Arrays.fill(playerTime, currentTime);
}

if (!learnEvents.isEmpty())
{
    EventData newTomeSpellsEvent = new EventData();
    newTomeSpellsEvent.type = EventData.NEW_TOME_SPELLs;
    newTomeSpellsEvent.animData = new ArrayList<>();

    newTomeSpellsEvent.spells = newTomeSpells.stream()
        .map(newTomeSpell-> new SpellData(newTomeSpell.getId(), newTomeSpell.getDelta(), newTomeSpell.isRepeatable()))
        .collect(Collectors.toList());

    viewerEvents.add(newTomeSpellsEvent);

    learnEvents.stream().forEach(learnEvent-> {
        chainAnims(
            1, newTomeSpellsEvent.animData, playerTime, learnEvent.playerIdx, AnimationData.NEW_SPELL_DURATION,
            AnimationData.NEW_SPELL_SEPARATION
        );
    });
}

learnEvents.forEach(learnEvent-> {
    EventData newLearnPayEvent = new EventData();

    newLearnPayEvent.type = EventData.LEARN_PAY;
    newLearnPayEvent.animData = new ArrayList<>();
    newLearnPayEvent.playerIdx = learnEvent.playerIdx;
    newLearnPayEvent.tomeIdx = learnEvent.tomeIdx;

    viewerEvents.add(newLearnPayEvent);

    chainAnims(
        learnEvent.tomeIdx, newLearnPayEvent.animData, playerTime, learnEvent.playerIdx, AnimationData.SHELF_TO_TOME_DURATION,
        AnimationData.SHELF_TO_TOME_SEPARATION
    );
});

int currentTime = IntStream.of(playerTime).max().getAsInt();

if (lastTriggerEnd != null && lastTriggerEnd > currentTime)
{
    currentTime = lastTriggerEnd;
}

int[] postTurnEventsTime = new int[] { currentTime, currentTime };
int newDeliveriesTimeIdx = 0;

if (!newDeliveries.isEmpty())
{
    EventData newDeliveriesEvent = new EventData();
    newDeliveriesEvent.type = EventData.NEW_DELIVERIES;
    newDeliveriesEvent.animData = new ArrayList<>();
    newDeliveriesEvent.spells = newDeliveries.stream()
        .map(newDelivery-> new SpellData(newDelivery.getId(), newDelivery.getDelta(), newDelivery.getScore()))
        .collect(Collectors.toList());

    viewerEvents.add(newDeliveriesEvent);

    chainAnims(
        newDeliveries.size(), newDeliveriesEvent.animData, postTurnEventsTime, newDeliveriesTimeIdx, AnimationData.NEW_SPELL_DURATION,
        AnimationData.NEW_SPELL_SEPARATION
    );

    currentTime = IntStream.of(postTurnEventsTime).max().getAsInt();
}

int minTime = 1000;

frameDuration = Math.max(
    currentTime,
    minTime
);
gameManager.setFrameDuration(frameDuration);

    }

    private void waitForAnim(List<AnimationData> eventAnimData, int[] playerTime, Integer playerIdx, int duration)
{
    eventAnimData.add(new AnimationData(playerTime[playerIdx], duration));
    playerTime[playerIdx] += duration;
}

private boolean gameOver()
{
    if (gameManager.getActivePlayers().size() <= 1)
    {
        return true;
    }
    for (Player player : gameManager.getPlayers()) {
    if (player.getDeliveriesCount() >= DELIVERY_GOAL)
    {
        return true;
    }
}
return false;
    }

    private Optional<Spell> getSpellById(int id)
{
    return Stream.of(
        deliveries.stream(),
        tome.stream(),
        delivered.stream()
            .map(del->del.getDelivery()),
        gameManager.getPlayers().stream()
            .flatMap(p->p.getSpells().stream())
    )
        .flatMap(Function.identity())
        .filter(spell->spell.getId() == id)
        .findFirst();
}

private void doReset(Player p) throws GameException
{
        if (p.getSpells().stream().allMatch(spell -> spell.isActive())) {
        throw new GameException("All spells are already castable");
    }
    p.getSpells().stream().forEach(PlayerSpell::activate);

    EventData e = new EventData();
e.type = EventData.RESET;
e.playerIdx = p.getIndex();
viewerEvents.add(e);

    }

    private DeliveryCompletion doDelivery(Player p, DeliverySpell del) throws GameException
{
        if (!p.canDeliver(del.recipe)) {
        throw new GameException("Not enough ingredients for order " + del.getId());
    }
        for (int i = 0; i < INGREDIENT_TYPE_COUNT; ++i) {
        p.getInventory().add(i, del.recipe.delta[i]);
    }
    DeliveryCompletion delCompletion = new DeliveryCompletion(del, deliveries.indexOf(del), getScoreOf(del));
delivered.add(delCompletion);
p.addDelivery();
p.addScore(getScoreOf(del));

EventData e = new EventData();
e.type = EventData.DELIVERY;
e.playerIdx = p.getIndex();
e.spellId = del.getId();
viewerEvents.add(e);
return delCompletion;
    }

    private void doLearn(Player p, TomeSpell spell) throws GameException
{
        int index = tome.indexOf(spell);
        if (p.getInventory().delta [0] < index * READ_AHEAD_COST) {
        throw new GameException("Not enough ingredients to learn " + spell.getId());
    }
        for (int i = 0; i < index; ++i) {
        tomeStockGain[i] += READ_AHEAD_COST;
        p.getInventory().delta[0] -= READ_AHEAD_COST;
    }
    PlayerSpell learnt = new PlayerSpell(spell, p);
p.getSpells().add(learnt);

int maxToGet = MAX_SPACE - p.getInventory().getTotal();
int ingredientsGot = Math.min(maxToGet, spell.stock);
p.getInventory().delta[0] += ingredientsGot;

learntSpells.add(spell);

EventData e = new EventData();
e.type = EventData.LEARN;
e.playerIdx = p.getIndex();
e.spellId = spell.getId();
e.resultId = learnt.getId();
e.tomeIdx = index;
e.acquired = ingredientsGot;
e.lost = spell.stock - ingredientsGot;
viewerEvents.add(e);
    }

    private void doPlayerSpell(Player p, PlayerSpell spell) throws GameException
{
        int repeats = p.getAction().getRepeats();
        if (repeats < 1) {
        throw new GameException("Repeat can't be zero (on " + spell.getId() + ")");
    }
        if (repeats > 1 && !spell.isRepeatable()) {
        throw new GameException("Spell " + spell.getId() + " is not repeatable");
    }
        if (!spell.isActive()) {
        throw new GameException("Spell " + spell.getId() + " is exhausted");
    }
        if (!p.canAfford(spell.recipe, repeats)) {
        throw new GameException("Not enough ingredients for spell " + spell.getId());
    }
        if (!p.enoughSpace(spell.recipe, repeats)) {
        throw new GameException("Not enough space in inventory for spell " + spell.getId());
    }

        //do spell
        for (int k = 0; k < repeats; ++k) {
        for (int i = 0; i < INGREDIENT_TYPE_COUNT; ++i)
        {
            p.getInventory().add(i, spell.getDelta()[i]);
        }
    }
    spell.deactivate();

    EventData e = new EventData();
e.type = EventData.PLAYER_SPELL;
e.playerIdx = p.getIndex();
e.spellId = spell.getId();
e.repeats = repeats;
viewerEvents.add(e);

    }

    public List<String> getGlobalInfoFor(Player player)
{
    return Collections.emptyList();
}

public void resetGameTurnData()
{
    gameManager.getPlayers().stream().forEach(Player::reset);
    gameSummaryManager.clear();
    learntSpells.clear();
    delivered.clear();
    viewerEvents.clear();
    newDeliveries.clear();
}

public List<EventData> getViewerEvents()
{
    return viewerEvents;
}

public List<TomeSpell> getTome()
{
    return tome;
}

public List<DeliverySpell> getDeliveries()
{
    return deliveries;
}

public static String getExpected()
{
    if (!Game.ACTIVE_SPELLS)
    {
        return "BREW <id> | WAIT";
    }
    if (!Game.ACTIVE_TOME)
    {
        return "BREW <id> | CAST <id> | REST | WAIT";
    }
    return "BREW <id> | CAST <id> [<repeat>] | LEARN <id> | REST | WAIT";
}

public void onEnd()
{
    if (INVENTORY_BONUS)
    {
        for (Player player : gameManager.getActivePlayers()) {
    for (int i = 1; i < INGREDIENT_TYPE_COUNT; i++)
    {
        player.addScore(player.getInventory().delta[i]);
    }
}
        }
    }

    public Map<Integer, BonusData> getBonusData()
{
    Map<Integer, BonusData> bonusData = new HashMap<>();
    for (int i = 0; i < 2; ++i)
    {
        if (bonus[i] > 0)
        {
            bonusData.put(i, new BonusData(bonus[i], bonusValue[i]));
        }
    }

    return bonusData;
}
}
}
