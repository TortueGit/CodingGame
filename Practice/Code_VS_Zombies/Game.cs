//#define DEBUG_MODE

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Save humans, destroy zombies!
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        var previousHumansCount = 0;
        var previousZombiesCount = 0;
        var simulation = new SimulationGame();

        var gameInfosDebug = new GameInfosForDebug();

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            var nashPosition = new Tuple<int, int>(int.Parse(inputs[0]), int.Parse(inputs[1]));
            var nash = new PlayerNash(nashPosition);

            int humanCount = int.Parse(Console.ReadLine());
            var humans = new List<Human>();
            for (var i = 0; i < humanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var human = new Human(
                    id: int.Parse(inputs[0]),
                    pos: new Tuple<int, int>(int.Parse(inputs[1]), int.Parse(inputs[2]))
                );
                humans.Add(human);
            }

            int zombieCount = int.Parse(Console.ReadLine());
            var zombies = new List<Zombie>();
            for (var i = 0; i < zombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var zombie = new Zombie(
                    id: int.Parse(inputs[0]),
                    pos: new Tuple<int, int>(int.Parse(inputs[1]), int.Parse(inputs[2])),
                    nextPos: new Tuple<int, int>(int.Parse(inputs[3]), int.Parse(inputs[4]))
                );
                zombies.Add(zombie);
            }

            if (zombieCount < previousZombiesCount)
            {
                int zombiesDead = previousZombiesCount - zombieCount;
                int humanPoints = 10 * previousHumansCount * previousHumansCount;

                for (var i = 0; i < zombiesDead; i++)
                {
                    var tmpPoints = humanPoints;

                    if (zombiesDead > 1)
                    {
                        tmpPoints *= Tools.Fibonacci(i + 1);
                    }
                    simulation.ActualScore += tmpPoints;
                }
            }

            var turnInfos = new GameTurnInfos(nash, humans, zombies);

            // For each turn, we want to simulate the maximum of game played to find the best next move possible.
            simulation.InitTurnInfos(turnInfos);
            var simResult = simulation.Simulation(gameInfosDebug);

            if (simulation.NewBest)
            {
                simulation.NumTurn = 0;
                simulation.NewBest = false;
            }

#if DEBUG_MODE
            DebugInfos.WriteSimulTurnInfos(gameInfosDebug.DebugInfosForTurn[simulation.NumTurn], DebugInfos.DEBUG);
#endif

            Tools.PrintMove(simulation.BestSimulation.Moves[simulation.NumTurn]);

            previousHumansCount = humanCount;
            previousZombiesCount = zombieCount;

            simulation.NumTurn++;
        }
    }
}

class GameInfos
{
    #region CONSTANTES
    public const int LEVEL_DEBUG = 0;

    public const int MAX_X = 16000;
    public const int MAX_Y = 9000;

    public const int NASH_ID = -1;

    public const int EMPTY_ZOMBIE = -4;

    public const int MAX_MOVES = 100;

    public const int MAX_SIMULATIONS_RUN = int.MaxValue;
    public const float TIMEOUT_FOR_A_TURN_IN_MS = 140.0f;
    public const int MAX_SIMULATION_RANDOM_STARTING_MOVES = 2;
    #endregion
}

class GameTurnInfos
{
    public PlayerNash Nash { get; set; }
    public List<Zombie> Zombies { get; set; }
    public List<Human> Humans { get; set; }
    public bool NashTargetDiedThisTurn { get; set; }

    public GameTurnInfos(PlayerNash nash, List<Human> humans, List<Zombie> zombies)
    {
        Nash = new PlayerNash(nash);
        SetHumansOrZombies(humans);
        SetHumansOrZombies(zombies);
        NashTargetDiedThisTurn = false;
    }

    public void SetHumansOrZombies<T>(List<T> objects)
    {
        var isHumans = false;
        if (typeof(T) == typeof(Human))
        {
            isHumans = true;
            Humans = new List<Human>();
        }
        else if (typeof(T) == typeof(Zombie))
        {
            Zombies = new List<Zombie>();
        }

        foreach (T obj in objects)
        {
            if (isHumans)
                Humans.Add(new Human((obj as Human)));
            else
                Zombies.Add(new Zombie((obj as Zombie)));
        }
    }
}

class PlayerNash
{
    #region CONSTANTES
    public const int MOUVEMENT = 1000;
    public const int RANGE = 2000;
    #endregion

    public Tuple<int, int> Position { get; set; }
    public Tuple<int, int> NextPosition { get; set; }
    public bool Arrived { get; set; }
    public Zombie Target { get; set; }

    public PlayerNash(Tuple<int, int> pos)
    {
        Position = pos;
        NextPosition = pos;
        Arrived = false;
        Target = null;
    }

    public PlayerNash(PlayerNash nash)
    {
        Position = nash.Position;
        NextPosition = nash.NextPosition;
        Arrived = nash.Arrived;
        Target = nash.Target;
    }
}

class Human
{
    public int Id { get; }
    public Tuple<int, int> Position { get; }

    public Human(int id, Tuple<int, int> pos)
    {
        Id = id;
        Position = pos;
    }

    public Human(Human human)
    {
        Id = human.Id;
        Position = human.Position;
    }
}

class Zombie
{
    #region CONSTANTES
    public const int MOUVEMENT = 400;
    #endregion

    public int Id { get; }
    public Tuple<int, int> Position { get; set; }
    public Tuple<int, int> NextPosition { get; set; }
    public bool Arrived { get; set; }
    public Human Target { get; set; }

    public Zombie(int id, Tuple<int, int> pos, Tuple<int, int> nextPos)
    {
        Id = id;
        Position = pos;
        NextPosition = nextPos;
        Arrived = false;
        Target = null;
    }

    public Zombie(Zombie zombie)
    {
        Id = zombie.Id;
        Position = zombie.Position;
        NextPosition = zombie.NextPosition;
        Arrived = zombie.Arrived;
        Target = zombie.Target;
    }
}

class SimulationGame
{
    public GameTurnInfos TurnInfos { get; set; }
    public SimulationInfos BestSimulation { get; set; }
    public int NumTurn { get; set; }
    public int ActualScore { get; set; }

    public bool NewBest { get; set; }

    public SimulationGame()
    {
        TurnInfos = null;
        BestSimulation = new SimulationInfos();
        NewBest = false;
        NumTurn = 0;
        ActualScore = 0;
    }

    public void InitTurnInfos(GameTurnInfos turnInfos)
    {
        TurnInfos = new GameTurnInfos(
            nash: turnInfos.Nash,
            humans: turnInfos.Humans,
            zombies: turnInfos.Zombies
        );
    }

    public SimulationResult Simulation(GameInfosForDebug gameInfosDebug)
    {
        var simResult = new SimulationResult();
        var agent = new SimulationAgent();

        while (agent.TotalMs < GameInfos.TIMEOUT_FOR_A_TURN_IN_MS && agent.SimRun <= GameInfos.MAX_SIMULATIONS_RUN)
        {
            var t0 = DateTime.UtcNow;

            var infosTurn = new GameTurnInfos(
                nash: TurnInfos.Nash,
                humans: TurnInfos.Humans,
                zombies: TurnInfos.Zombies
            );

            var tmpResult = SimulateGame(infosTurn, gameInfosDebug);

            if (tmpResult.Points >= simResult.Points)
                simResult = new SimulationResult(tmpResult);

            var t1 = DateTime.UtcNow;
            agent.TotalMs += Tools.TimeDifferenceInMillisecond(t0, t1);
            agent.SimRun++;
        }

        DebugInfos.WriteDebugMessage(
                functionName: "Simulation end",
                strings: new string[] { $"total sim run {agent.SimRun} in {agent.TotalMs} ms\n" },
                debugLevel: DebugInfos.INFOS
        );

        return simResult;
    }

    private SimulationResult SimulateGame(GameTurnInfos infosTurn, GameInfosForDebug gameInfosDebug)
    {
        var rand = new Random();
        var sr = new SimulationResult();
        var simInfos = new SimulationInfos();
        var moves = new List<Tuple<int, int>>();
        var gameDebug = new GameInfosForDebug();
        var turnInfosDebug = new DebugInfosForEachTurn();
        var scoreForThisTurn = 0;

        simInfos.SimStartingRandomMovesNum = rand.Next(GameInfos.MAX_SIMULATION_RANDOM_STARTING_MOVES + 1);

        ComputePlayerTarget(infosTurn, simInfos);

        while (!simInfos.SimZombieAllDead && !simInfos.SimFailure && simInfos.SimMovesCount < GameInfos.MAX_MOVES)
        {
            // Simulate a turn of the game.
            simInfos.Moves.Add(Turn(infosTurn, sr, simInfos));

#if DEBUG_MODE
            turnInfosDebug.Nash = infosTurn.Nash;
            turnInfosDebug.SetHumansOrZombies(infosTurn.Humans);
            turnInfosDebug.SetHumansOrZombies(infosTurn.Zombies);
            turnInfosDebug.Points = sr.Points;
            turnInfosDebug.Move = simInfos.Moves.Last();
            turnInfosDebug.NumTurn = simInfos.SimMovesCount;
            gameDebug.DebugInfosForTurn.Add(new DebugInfosForEachTurn(turnInfosDebug));
#endif

            simInfos.SimMovesCount++;
        }

        if (simInfos.SimZombieAllDead && 
            !simInfos.SimFailure && 
            ((sr.Points + ActualScore) > BestSimulation.SimPoints ||
            (sr.Points + ActualScore) == BestSimulation.SimPoints && (simInfos.SimMovesCount < (BestSimulation.SimMovesCount - NumTurn))))
        {
            simInfos.SimPoints = sr.Points + ActualScore;
            BestSimulation = simInfos;
            NewBest = true;
#if DEBUG_MODE
            gameInfosDebug.SetDebugInfosForTurn(gameDebug.DebugInfosForTurn);
#endif
        }

        return sr;
    }

    private Tuple<int, int> Turn(GameTurnInfos infosTurn, SimulationResult simResult, SimulationInfos simInfos)
    {
        var move = new Tuple<int, int>(-1, -1);

        foreach (Zombie zombie in infosTurn.Zombies)
        {
            FindZombieTarget(zombie, infosTurn);
            MoveZombie(zombie, infosTurn.Nash);
        }

        move = GetPlayerDestination(infosTurn.Nash);

        MovePlayer(infosTurn.Nash);

        Evaluate(infosTurn, simResult);

        ZombiesEat(infosTurn);

        if ((infosTurn.Humans.Count) > 0 && (infosTurn.Zombies.Count > 0))
        {
            if (infosTurn.Nash.Arrived || infosTurn.NashTargetDiedThisTurn)
            {
                ComputePlayerTarget(infosTurn, simInfos);
                infosTurn.NashTargetDiedThisTurn = false;
            }
        }
        else
        {
            simInfos.SimFailure = (infosTurn.Humans.Count <= 0);
            simInfos.SimZombieAllDead = (infosTurn.Zombies.Count <= 0);
        }

        return move;
    }

    private void FindZombieTarget(Zombie zombie, GameTurnInfos infosTurn)
    {
        float minDist = float.PositiveInfinity;
        float tmpDist;

        foreach (Human human in infosTurn.Humans)
        {
            tmpDist = Tools.GetDistance(zombie.Position, human.Position);
            if (tmpDist < minDist)
            {
                zombie.Target = human;
                minDist = tmpDist;
            }
        }

        tmpDist = Tools.GetDistance(zombie.Position, infosTurn.Nash.Position);
        if (tmpDist < minDist)
        {
            zombie.Target = null;
            minDist = tmpDist;
        }
    }

    private void MoveZombie(Zombie zombie, PlayerNash nash)
    {
        zombie.Arrived = NextPosZombie(zombie, nash, out var zombiePosition);
        zombie.Position = zombiePosition;
    }

    private bool NextPosZombie(Zombie zombie, PlayerNash nash, out Tuple<int, int> posOut)
    {
        var arrived = false;
        Tuple<int, int> targetPos;
        float dist;
        float t;

        // If the zombie has a human target he well go for eat it; otherwise the target must be Nash, so the zombie will try to go to Nash position.
        if (zombie.Target != null)
        {
            targetPos = zombie.Target.Position;
        }
        else
        {
            targetPos = nash.Position;
        }

        dist = Tools.GetDistance(zombie.Position, targetPos);
        if (dist <= Zombie.MOUVEMENT)
        {
            arrived = true;
            posOut = new Tuple<int, int>(targetPos.Item1, targetPos.Item2);
        }
        else
        {
            t = Zombie.MOUVEMENT / dist;
            posOut = new Tuple<int, int>(
                (int)Math.Floor(zombie.Position.Item1 + (t * (targetPos.Item1 - zombie.Position.Item1))),
                (int)Math.Floor(zombie.Position.Item2 + (t * (targetPos.Item2 - zombie.Position.Item2)))
                );
        }

        return arrived;
    }

    private Tuple<int, int> GetPlayerDestination(PlayerNash nash)
    {
        Zombie target;

        if (nash.Target != null)
        {
            target = nash.Target;
            NextPosZombie(target, nash, out var destination);

            if (destination is null)
                return new Tuple<int, int>(-1, -1);

            return destination;
        }
        else
        {
            if (nash.NextPosition is null)
                return new Tuple<int, int>(-1, -1);

            return nash.NextPosition;
        }
    }

    private void MovePlayer(PlayerNash nash)
    {
        nash.Arrived = NextPosNash(nash, out var nashNextPos);
        nash.Position = nashNextPos;
    }

    private bool NextPosNash(PlayerNash nash, out Tuple<int, int> posOut)
    {
        Tuple<int, int> destination;
        float distance;
        float t;

        var arrived = false;

        if (nash.Target != null || nash.NextPosition != null)
        {
            destination = GetPlayerDestination(nash);
            distance = Tools.GetDistance(nash.Position, destination);

            if (distance <= PlayerNash.MOUVEMENT)
            {
                arrived = true;
                posOut = new Tuple<int, int>(destination.Item1, destination.Item2);
            }
            else
            {
                t = PlayerNash.MOUVEMENT / distance;
                posOut = new Tuple<int, int>(
                        (int)Math.Floor(nash.Position.Item1 + (t * (destination.Item1 - nash.Position.Item1))),
                        (int)Math.Floor(nash.Position.Item2 + (t * (destination.Item2 - nash.Position.Item2)))
                    );
            }
        }
        else
        {
            posOut = new Tuple<int, int>(-1, -1);
        }

        return arrived;
    }

    private void Evaluate(GameTurnInfos infosTurn, SimulationResult sr)
    {
        int tmpPoints;
        int humanNum = infosTurn.Humans.Count;
        int humanPoints = 10 * humanNum * humanNum;
        var killableZombies = new List<Zombie>();
        var killableZombiesLen = ZombiesInRangeOfPlayer(killableZombies, infosTurn);

        var tmpId = (infosTurn.Nash.Target != null) ? infosTurn.Nash.Target.Id : GameInfos.EMPTY_ZOMBIE;

        for (var i = 0; i < killableZombiesLen; i++)
        {
            tmpPoints = humanPoints;

            if (killableZombiesLen > 1)
            {
                tmpPoints *= Tools.Fibonacci(i + 1);
            }
            sr.Points += tmpPoints;
        }

        if (killableZombies.Any(x => x.Id == tmpId))
        {
            infosTurn.Nash.Target = null;
            infosTurn.NashTargetDiedThisTurn = true;
        }

        var zombiesToRemove = new HashSet<Zombie>(killableZombies);
        infosTurn.Zombies.RemoveAll(x => zombiesToRemove.Contains(x));
    }

    private int ZombiesInRangeOfPlayer(List<Zombie> zombiesInRange, GameTurnInfos infosTurn)
    {
        var len = 0;
        float dx, dy;

        foreach (Zombie zombie in infosTurn.Zombies)
        {
            if (Tools.GetDistance(infosTurn.Nash.Position, zombie.Position) <= PlayerNash.RANGE)
            {
                zombiesInRange.Add(zombie);
                len++;
            }
        }

        return len;
    }

    private void ZombiesEat(GameTurnInfos infosTurn)
    {
        var zombieTargetIdTmp = new List<int>();

        foreach (Zombie zombie in infosTurn.Zombies.Where(x => x.Target != null))
        {
            if (ZombieArrivedAtTarget(zombie))
            {
                if (infosTurn.Humans.Any(x => x.Id == zombie.Target.Id))
                {
                    infosTurn.Humans.RemoveAll(x => x.Id == zombie.Target.Id);
                }
            }
        }
    }

    private bool ZombieArrivedAtTarget(Zombie zombie)
    {
        if (zombie.Target == null)
            return false;

        return (int)zombie.Position.Item1 == (int)zombie.Target.Position.Item1 && (int)zombie.Position.Item2 == (int)zombie.Target.Position.Item2;
    }

    private void ComputePlayerTarget(GameTurnInfos infosTurn, SimulationInfos simInfos)
    {
        var rand = new Random();
        var zombiesThatDoNotTargetPlayer = new List<Zombie>();

        // If there is some random moves, we made Nash do the moves; otherwise we set a target for Nash.
        if (simInfos.SimStartingRandomMovesNum > 0)
        {
            infosTurn.Nash.NextPosition = new Tuple<int, int>(rand.Next(GameInfos.MAX_X), rand.Next(GameInfos.MAX_Y));
            infosTurn.Nash.Target = null;
            simInfos.SimStartingRandomMovesNum--;
        }
        else
        {
            zombiesThatDoNotTargetPlayer.AddRange(infosTurn.Zombies.Where(x => x.Target != null));

            // Define a zombie target for Nash : target is a zombie targetting a human, if there is at least one; any zombie in the map, otherwise.
            infosTurn.Nash.Target = (zombiesThatDoNotTargetPlayer.Count > 0) ?
                                                zombiesThatDoNotTargetPlayer[rand.Next(zombiesThatDoNotTargetPlayer.Count)] :
                                                infosTurn.Zombies[rand.Next(infosTurn.Zombies.Count)];

            infosTurn.Nash.Arrived = false;
        }
    }
}

class SimulationAgent
{
    public float TotalMs { get; set; }
    public int SimRun { get; set; }

    public SimulationAgent()
    {
        TotalMs = 0.0f;
        SimRun = 0;
    }
}

class SimulationResult
{
    public int Points { get; set; }
    public int NumberOfTurn { get; set; }

    public SimulationResult()
    {
        Points = 0;
        NumberOfTurn = 0;
    }

    public SimulationResult(SimulationResult simResult)
    {
        Points = simResult.Points;
        NumberOfTurn = simResult.NumberOfTurn;
    }
}

class SimulationInfos
{
    public bool SimZombieAllDead { get; set; }
    public bool SimFailure { get; set; }
    public int SimMovesCount { get; set; }
    public int SimPoints { get; set; }
    public List<Tuple<int, int>> Moves { get; set; }
    public int SimStartingRandomMovesNum { get; set; }

    public SimulationInfos()
    {
        SimZombieAllDead = false;
        SimFailure = false;
        SimMovesCount = 0;
        SimPoints = 0;
        Moves = new List<Tuple<int, int>>();
        SimStartingRandomMovesNum = 0;
    }
}

static class Tools
{
    public static float GetDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
    {
        double distance;
        var distX = 0;
        var distY = 0;

        distX = pos1.Item1 - pos2.Item1;
        distY = pos1.Item2 - pos2.Item2;

        distance = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

        return (float)distance;
    }

    public static int Fibonacci(int n)
    {
        int w;
        if (n <= 0) return 0;
        if (n == 1) return 1;
        var u = 0;
        var v = 1;
        for (var i = 2; i <= n; i++)
        {
            w = u + v;
            u = v;
            v = w;
        }

        return v;
    }

    public static bool IsMoveValid(Tuple<int, int> move)
    {
        return move.Item1 != -1 && move.Item2 != -1;
    }

    public static float TimeDifferenceInMillisecond(DateTime t0, DateTime t1)
    {
        return (float)((t1 - t0).TotalMilliseconds);
    }

    public static void PrintMove(Tuple<int, int> move)
    {
        Console.WriteLine($"{move.Item1} {move.Item2}");
    }
}

static class DebugInfos
{
    public const int ERROR = 1;
    public const int WARNING = 2;
    public const int INFOS = 3;
    public const int DEBUG = 4;

    public static void WriteDebugMessage(string functionName, string[] strings = null, int debugLevel = 4)
    {
        if (GameInfos.LEVEL_DEBUG >= debugLevel)
        {
            StringBuilder sb;
            if (strings == null)
            {
                sb = new StringBuilder(functionName);
            }
            else
            {
                sb = new StringBuilder(functionName + " : ");
                foreach (string str in strings)
                    sb.Append(str);
            }

            Console.Error.WriteLine(sb);
        }
    }

    public static void WriteSimulTurnInfos(DebugInfosForEachTurn turnInfos, int debugLevel)
    {
        var strings = new string[]
        {
            $"turnInfos.NumTurn [{turnInfos.NumTurn}] ",
            $"turnInfos.Humans.Count [{turnInfos.Humans.Count}] ",
            $"turnInfos.Zombies.Count [{turnInfos.Zombies.Count}] ",
            $"turnInfos.Points [{turnInfos.Points}] ",
            $"turnInfos.Move [{turnInfos.Move}] "
        };

        WriteDebugMessage("WriteSimulTurnInfos", strings, debugLevel);
    }
}

class GameInfosForDebug
{
    public List<DebugInfosForEachTurn> DebugInfosForTurn { get; set; }

    public GameInfosForDebug()
    {
        DebugInfosForTurn = new List<DebugInfosForEachTurn>();
    }

    public void SetDebugInfosForTurn(List<DebugInfosForEachTurn> debugInfos)
    {
        DebugInfosForTurn = new List<DebugInfosForEachTurn>();
        foreach (DebugInfosForEachTurn infosTurn in debugInfos)
            DebugInfosForTurn.Add(new DebugInfosForEachTurn(infosTurn));
    }
}

class DebugInfosForEachTurn
{
    public PlayerNash Nash { get; set; }
    public List<Human> Humans { get; set; }
    public List<Zombie> Zombies { get; set; }
    public int Points { get; set; }
    public Tuple<int, int> Move { get; set; }
    public int NumTurn { get; set; }

    public DebugInfosForEachTurn()
    {
        Nash = null;
        Humans = new List<Human>();
        Zombies = new List<Zombie>();
        Points = 0;
        Move = new Tuple<int, int>(-1, -1);
        NumTurn = 0;
    }

    public DebugInfosForEachTurn(DebugInfosForEachTurn debugInfosForEachTurn)
    {
        Nash = debugInfosForEachTurn.Nash;
        Humans = debugInfosForEachTurn.Humans;
        Zombies = debugInfosForEachTurn.Zombies;
        Points = debugInfosForEachTurn.Points;
        Move = debugInfosForEachTurn.Move;
        NumTurn = debugInfosForEachTurn.NumTurn;
    }

    public void SetHumansOrZombies<T>(List<T> objects)
    {
        if (typeof(T) == typeof(Human))
            Humans = new List<Human>();
        if (typeof(T) == typeof(Zombie))
            Zombies = new List<Zombie>();

        foreach (T obj in objects)
        {
            if (obj is Human)
                Humans.Add(new Human((obj as Human)));

            if (obj is Zombie)
                Zombies.Add(new Zombie((obj as Zombie)));
        }
    }
}

