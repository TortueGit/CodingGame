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
        var myGame = new GameInfos();
        bool foundValidMove = false;
        SimulationAgent agent = new SimulationAgent();
        SimulationInfos simulationInfos = new SimulationInfos();
        string[] inputs;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');

            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            myGame.SetNashPosition(new Tuple<int, int>(x, y));

            var humanCount = int.Parse(Console.ReadLine());
            myGame.NumberOfHumans = humanCount;

            for (var i = 0; i < humanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var humanId = int.Parse(inputs[0]);
                var humanX = int.Parse(inputs[1]);
                var humanY = int.Parse(inputs[2]);
                var human = new Human(
                                    humanId,
                                    new Tuple<int, int>(humanX, humanY)
                                );
                myGame.AddHuman(human);
            }

            var zombieCount = int.Parse(Console.ReadLine());
            myGame.NumberOfZombies = zombieCount;
            for (var i = 0; i < zombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var zombieId = int.Parse(inputs[0]);
                var zombieX = int.Parse(inputs[1]);
                var zombieY = int.Parse(inputs[2]);
                var zombieXNext = int.Parse(inputs[3]);
                var zombieYNext = int.Parse(inputs[4]);
                var zombie = new Zombie(
                                        zombieId,
                                        new Tuple<int, int>(zombieX, zombieY),
                                        new Tuple<int, int>(zombieXNext, zombieYNext)
                                    );

                if (myGame.Zombies.Any(x => x.Id == zombieId))
                {
                    DebugInfos.WriteDebugMessage(
                            "Main",
                            new string[] { 
                                $"zombieId [{zombieId}] ",
                                $"should be in [{myGame.Zombies.Single(x => x.Id == zombieId).NextPosition}]",
                                $"is in [{zombie.Position}]"
                            },
                            DebugInfos.INFOS
                        );
                }
                myGame.AddZombie(zombie);
            }

            agent.NewBest = false;
            SimulationGame.Simulation(agent, simulationInfos, myGame, myGame.Nash);

            if (agent.BestResult.Len == 0)
                Console.Error.WriteLine("No winning game found");

            if (!agent.NewBest)
                agent.MoveNum++;

            foundValidMove = false;
            while (agent.MoveNum < agent.BestResult.Len && !foundValidMove)
            {
                if (Tools.IsMoveValid(agent.BestResult.MoveList[agent.MoveNum]))
                {
                    Tools.PrintMove(agent.BestResult.MoveList[agent.MoveNum]);
                    foundValidMove = true;
                }
                else
                    agent.MoveNum++;
            }

            if (!foundValidMove)
            {
                for (int k = 0; k < agent.BestResult.Len; k++)
                    Console.Error.WriteLine($"move {k} is ({agent.BestResult.MoveList[k].Item1}, {agent.BestResult.MoveList[k].Item2})");
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            //Console.WriteLine(string.Format("{0} {1}", nextMovement.Item1, nextMovement.Item2)); // Your destination coordinates
        }
    }
}

class GameInfos
{
    #region CONSTANTES
    public const int LEVEL_DEBUG = 3;

    public const int MAX_X = 16000;
    public const int MAX_Y = 9000;
    public const int NASH_ID = -1;
    public const int MAX_ZOMBIES = 100;
    public const int MAX_HUMANS = 100;
    public const int MAX_MOVES = 100;
    public const int MAX_SIMULATIONS_RUN = 2000;
    public const int EMPTY_ZOMBIE = -4;
    public const int EMPTY_HUMAN = -5;
    public const float TIMEOUT_FOR_A_TURN_IN_MS = 100.0f;
    #endregion

    #region FIELDS
    private PlayerNash _nash = new PlayerNash();
    private List<Human> _humans = new List<Human>();
    private List<Zombie> _zombies = new List<Zombie>();

    private int _numberOfHumans = 0;
    private int _numberOfZombies = 0;
    private int _moveNum = 0;
    #endregion

    #region CONSTRUCTORS
    public GameInfos()
    {
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets the zombies.
    /// </summary>
    /// <value>The zombies.</value>
    public List<Zombie> Zombies => _zombies;
    /// <summary>
    /// Gets the humans.
    /// </summary>
    /// <value>The humans.</value>
    public List<Human> Humans => _humans;

    /// <summary>
    /// Gets or sets the move number.
    /// </summary>
    /// <value>The move number.</value>
    public int MoveNum { get => _moveNum; set => _moveNum = value; }
    /// <summary>
    /// Gets or sets the number of humans.
    /// </summary>
    /// <value>The number of humans.</value>
    public int NumberOfHumans { get => _numberOfHumans; set => _numberOfHumans = value; }
    /// <summary>
    /// Gets or sets the number of zombies.
    /// </summary>
    /// <value>The number of zombies.</value>
    public int NumberOfZombies { get => _numberOfZombies; set => _numberOfZombies = value; }
    /// <summary>
    /// Gets or sets the nash.
    /// </summary>
    /// <value>The nash.</value>
    public PlayerNash Nash { get => _nash; set => _nash = value; }
    #endregion

    #region METHODS
    /// <summary>
    /// Sets Nash position.
    /// </summary>
    /// <param name="pos">Position.</param>
    public void SetNashPosition(Tuple<int, int> pos)
    {
        Nash.Position = pos;
        if (Nash.NextPosition is null)
            Nash.NextPosition = pos;

        DebugInfos.WriteDebugMessage("SetNashPosition", new string[]
            {
                "_nash.Position = ", Nash.Position.ToString(), " ",
                "_nash.NextPosition = ", Nash.NextPosition.ToString()
            });
    }

    /// <summary>
    /// Is zombie or human exist ?
    /// </summary>
    /// <returns><c>true</c>, if human or zombie already exist, <c>false</c> otherwise.</returns>
    /// <param name="humanOrZombie">Human or zombie.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public bool IsExist<T>(T humanOrZombie)
    {
        if (humanOrZombie.GetType().Equals(typeof(Human)))
            return Humans.Exists(x => x.Id == (humanOrZombie as Human).Id);
        else if (humanOrZombie.GetType().Equals(typeof(Zombie)))
            return Zombies.Exists(x => x.Id == (humanOrZombie as Zombie).Id);
        else
            return false;
    }

    /// <summary>
    /// Add or update a human.
    /// </summary>
    /// <param name="human">Human.</param>
    public void AddHuman(Human human)
    {
        if (!IsExist<Human>(human))
        {
            Humans.Add(human);
            DebugInfos.WriteDebugMessage("AddHuman", new string[]
                {
                    "human.Id = ", human.Id.ToString(), " ",
                    "human.Position = ", human.Position.ToString()
                });
        }
        else
            UpdateHumanPosition(human);
    }

    /// <summary>
    /// Add or update a zombie.
    /// </summary>
    /// <param name="zombie">Zombie.</param>
    public void AddZombie(Zombie zombie)
    {
        if (!IsExist<Zombie>(zombie))
        {
            Zombies.Add(zombie);
            DebugInfos.WriteDebugMessage
                (
                    "AddZombie",
                    new string[] { "zombie.Id = ", zombie.Id.ToString(), " ",
                                    "zombie.Position = ", zombie.Position.ToString(), " ",
                                    "zombie.NextPosition = ", zombie.NextPosition.ToString()
                    },
                    DebugInfos.DEBUG
                );
        }
        else
            UpdateZombiePosition(zombie);
    }

    /// <summary>
    /// Updates the human position.
    /// </summary>
    /// <param name="human">Human.</param>
    private void UpdateHumanPosition(Human human)
    {
        DebugInfos.WriteDebugMessage("UpdateHumanPosition begin", new string[]
            {
                    "human.Id = ", Humans.Single(x => x.Id == human.Id).Id.ToString(), " ",
                    "human.Position = ", Humans.Single(x => x.Id == human.Id).Position.ToString()
            });

        Humans.Single(x => x.Id == human.Id).Position = human.Position;

        DebugInfos.WriteDebugMessage("UpdateHumanPosition end", new string[]
            {
                    "human.Id = ", Humans.Single(x => x.Id == human.Id).Id.ToString(), " ",
                    "human.Position = ", Humans.Single(x => x.Id == human.Id).Position.ToString()
            });
    }

    /// <summary>
    /// Updates the zombie position.
    /// </summary>
    /// <param name="zombie">Zombie.</param>
    private void UpdateZombiePosition(Zombie zombie)
    {
        DebugInfos.WriteDebugMessage("UpdateZombiePosition begin", new string[]
            {
                    "zombie.Id = ", Zombies.Single(x => x.Id == zombie.Id).Id.ToString(), " ",
                    "zombie.Position = ", Zombies.Single(x => x.Id == zombie.Id).Position.ToString(), " ",
                    "zombie.NextPosition = ", Zombies.Single(x => x.Id == zombie.Id).NextPosition.ToString()
            });

        Zombies.Single(x => x.Id == zombie.Id).Position = zombie.Position;
        Zombies.Single(x => x.Id == zombie.Id).NextPosition = zombie.NextPosition;

        DebugInfos.WriteDebugMessage("UpdateZombiePosition end", new string[]
            {
                    "zombie.Id = ", Zombies.Single(x => x.Id == zombie.Id).Id.ToString(), " ",
                    "zombie.Position = ", Zombies.Single(x => x.Id == zombie.Id).Position.ToString(), " ",
                    "zombie.NextPosition = ", Zombies.Single(x => x.Id == zombie.Id).NextPosition.ToString()
            });
    }
    #endregion
}

class PlayerNash
{
    #region CONSTANTES
    public const int MOUVEMENT = 1000;
    public const int RANGE = 2000;
    #endregion

    #region FIELDS
    private Tuple<int, int> _position;
    private Tuple<int, int> _nextPosition;
    private int _zombieIdMovingTo = -1;
    private bool _targetingZombie = false;
    private bool _arrived = false;
    private Zombie _target = null;
    #endregion

    #region CONSTRUCTORS
    public PlayerNash() { }

    public PlayerNash(PlayerNash nash)
    {
        _position = nash.Position;
        _nextPosition = nash.NextPosition;
        _zombieIdMovingTo = nash.ZombieIdMovingTo;
        _targetingZombie = nash.TargetingZombie;
        _arrived = nash.Arrived;
        _target = nash.Target;
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    public Tuple<int, int> Position { get => _position; set => _position = value; }
    /// <summary>
    /// Gets or sets the next position.
    /// </summary>
    /// <value>The next position.</value>
    public Tuple<int, int> NextPosition { get => _nextPosition; set => _nextPosition = value; }
    public int ZombieIdMovingTo { get => _zombieIdMovingTo; set => _zombieIdMovingTo = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:Nash"/> targeting zombie.
    /// </summary>
    /// <value><c>true</c> if targeting zombie; otherwise, <c>false</c>.</value>
    public bool TargetingZombie { get => _targetingZombie; set => _targetingZombie = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:Nash"/> is arrived.
    /// </summary>
    /// <value><c>true</c> if arrived; otherwise, <c>false</c>.</value>
    public bool Arrived { get => _arrived; set => _arrived = value; }
    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    /// <value>The target.</value>
    public Zombie Target { get => _target; set => _target = value; }
    #endregion
}

class Human
{
    #region FIELDS
    private int _id;
    private Tuple<int, int> _position;
    #endregion

    #region CONSTRUCTORS
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Human"/> class.
    /// </summary>
    /// <param name="id">Identifier.</param>
    /// <param name="pos">Position.</param>
    public Human(int id, Tuple<int, int> pos)
    {
        _id = id;
        _position = pos;
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    public int Id => _id;

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    public Tuple<int, int> Position { get => _position; set => _position = value; }
    #endregion
}

class Zombie
{
    #region CONSTANTES
    public const int MOUVEMENT = 400;
    #endregion

    #region FIELDS
    private int _id;
    private Tuple<int, int> _position;
    private Tuple<int, int> _nextPosition;
    private int _closestHumanId;    // ID of a human or -1 if the closest human is Nash.
    private int _nbTurnToReachHuman;    // The number of turn the zombie needs to reach the closest human.
    private bool _arrived;
    private Human _target = null;
    #endregion

    #region CONSTRUCTORS
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Zombie"/> class.
    /// </summary>
    /// <param name="id">Identifier.</param>
    /// <param name="pos">Position.</param>
    /// <param name="nextPos">Next position.</param>
    public Zombie(int id, Tuple<int, int> pos, Tuple<int, int> nextPos)
    {
        _id = id;
        _position = pos;
        _nextPosition = nextPos;
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    public int Id => _id;
    /// <summary>
    /// Gets or sets the nb turn to reach human.
    /// </summary>
    /// <value>The nb turn to reach human.</value>
    public int NbTurnToReachHuman => _nbTurnToReachHuman;
    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    public Tuple<int, int> Position { get => _position; set => _position = value; }
    /// <summary>
    /// Gets or sets the next position.
    /// </summary>
    /// <value>The next position.</value>
    public Tuple<int, int> NextPosition { get => _nextPosition; set => _nextPosition = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:Zombie"/> is arrived.
    /// </summary>
    /// <value><c>true</c> if arrived; otherwise, <c>false</c>.</value>
    public bool Arrived { get => _arrived; set => _arrived = value; }
    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    /// <value>The target.</value>
    internal Human Target { get => _target; set => _target = value; }
    /// <summary>
    /// Gets or sets the closest human identifier.
    /// </summary>
    /// <value>The closest human identifier.</value>
    public int ClosestHumanId { get => _closestHumanId; set => _closestHumanId = value; }
    #endregion
}

class SimulationInfos
{
    #region Fields
    private PlayerNash _simNash;
    private Zombie _simNashTargetCpy;
    private List<Zombie> _simZombies = new List<Zombie>();
    private List<Human> _simHumans = new List<Human>();

    private bool _simFailure = false;
    private bool _simZombiesAllDead = false;
    private bool _simZombiesDiedThisTurn = false;
    private bool _simNashTargetDiedThisTurn = false;

    private int _simPoints = 0;
    private int _simTurnNum = 0;
    private int _simCurrentBest = 0;
    private int _simMovesCount = 0;
    private int _simZombieCount = 0;
    private int _simHumanCount = 0;

    private int _simStartingRandomMovesNum = -1;
    private int _simMaxStartingRandomMoves = 3;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the sim max starting random moves.
    /// </summary>
    /// <value>The sim max starting random moves.</value>
    public int SimMaxStartingRandomMoves => _simMaxStartingRandomMoves;
    /// <summary>
    /// Gets or sets the sim nash.
    /// </summary>
    /// <value>The sim nash.</value>
    public PlayerNash SimNash { get => _simNash; set => _simNash = value; }
    /// <summary>
    /// Gets or sets the sim nash target cpy.
    /// </summary>
    /// <value>The sim nash target cpy.</value>
    public Zombie SimNashTargetCpy { get => _simNashTargetCpy; set => _simNashTargetCpy = value; }
    /// <summary>
    /// Gets or sets the sim zombies.
    /// </summary>
    /// <value>The sim zombies.</value>
    public List<Zombie> SimZombies { get => _simZombies; set => _simZombies = value; }
    /// <summary>
    /// Gets or sets the sim humans.
    /// </summary>
    /// <value>The sim humans.</value>
    public List<Human> SimHumans { get => _simHumans; set => _simHumans = value; }
    /// <summary>
    /// Gets or sets the sim current best.
    /// </summary>
    /// <value>The sim current best.</value>
    public int SimCurrentBest { get => _simCurrentBest; set => _simCurrentBest = value; }
    /// <summary>
    /// Gets or sets the sim starting random moves number.
    /// </summary>
    /// <value>The sim starting random moves number.</value>
    public int SimStartingRandomMovesNum { get => _simStartingRandomMovesNum; set => _simStartingRandomMovesNum = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim zombies all dead.
    /// </summary>
    /// <value><c>true</c> if sim zombies all dead; otherwise, <c>false</c>.</value>
    public bool SimZombiesAllDead { get => _simZombiesAllDead; set => _simZombiesAllDead = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim failure.
    /// </summary>
    /// <value><c>true</c> if sim failure; otherwise, <c>false</c>.</value>
    public bool SimFailure { get => _simFailure; set => _simFailure = value; }
    /// <summary>
    /// Gets or sets the sim moves count.
    /// </summary>
    /// <value>The sim moves count.</value>
    public int SimMovesCount { get => _simMovesCount; set => _simMovesCount = value; }
    /// <summary>
    /// Gets or sets the sim points.
    /// </summary>
    /// <value>The sim points.</value>
    public int SimPoints { get => _simPoints; set => _simPoints = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim nash target died this turn.
    /// </summary>
    /// <value><c>true</c> if sim nash target died this turn; otherwise, <c>false</c>.</value>
    public bool SimNashTargetDiedThisTurn { get => _simNashTargetDiedThisTurn; set => _simNashTargetDiedThisTurn = value; }
    /// <summary>
    /// Gets or sets the sim zombie count.
    /// </summary>
    /// <value>The sim zombie count.</value>
    public int SimZombieCount { get => _simZombieCount; set => _simZombieCount = value; }
    /// <summary>
    /// Gets or sets the sim human count.
    /// </summary>
    /// <value>The sim human count.</value>
    public int SimHumanCount { get => _simHumanCount; set => _simHumanCount = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim zombies died this turn.
    /// </summary>
    /// <value><c>true</c> if sim zombies died this turn; otherwise, <c>false</c>.</value>
    public bool SimZombiesDiedThisTurn { get => _simZombiesDiedThisTurn; set => _simZombiesDiedThisTurn = value; }
    /// <summary>
    /// Gets or sets the sim turn number.
    /// </summary>
    /// <value>The sim turn number.</value>
    public int SimTurnNum { get => _simTurnNum; set => _simTurnNum = value; }
    #endregion

    #region METHODS
    /// <summary>
    /// Initialize the simulation infos.
    /// </summary>
    /// <param name="gameInfos">Game infos.</param>
    /// <param name="nash">Nash.</param>
    public void SimulationSetup(GameInfos gameInfos, PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("SimulationSetup begin");

        SimNash = new PlayerNash(nash);

        SimZombies = new List<Zombie>(gameInfos.Zombies);
        /*for (int i = 0; i < gameInfos.Zombies.Count; i++)
            _simZombies.Add(gameInfos.Zombies[i]);*/

        SimHumans = new List<Human>(gameInfos.Humans);
        /*for (int i = 0; i < gameInfos.Humans.Count; i++)
            _simHumans.Add(gameInfos.Humans[i]);*/

        DebugInfos.WriteDebugMessage("SimulationSetup", new string[]
            {
                $"SimZombies.Count = {SimZombies.Count} ",
                $"SimHumans.Count = {SimHumans.Count}",
            }, debugLevel: DebugInfos.DEBUG);

        SimFailure = false;
        SimZombiesAllDead = false;
        SimZombiesDiedThisTurn = false;
        SimNashTargetDiedThisTurn = false;

        SimPoints = 0;
        SimTurnNum = 0;
        SimMovesCount = 0;
        SimZombieCount = SimZombies.Count;
        SimHumanCount = SimHumans.Count;

        SimStartingRandomMovesNum = 0;
        _simMaxStartingRandomMoves = 1;

        DebugInfos.WriteDebugMessage("SimulationSetup end");
    }
    #endregion
}

class SimulationAgent
{
    #region FIELDS
    private int _simRun = 0;
    private bool _newBest = false;
    private float _totalMs = 0;
    private int _moveNum = 0;
    private SimulationResult _bestResult = new SimulationResult();
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets or sets the sim run.
    /// </summary>
    /// <value>The sim run.</value>
    public int SimRun { get => _simRun; set => _simRun = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationAgent"/> new best.
    /// </summary>
    /// <value><c>true</c> if new best; otherwise, <c>false</c>.</value>
    public bool NewBest { get => _newBest; set => _newBest = value; }
    /// <summary>
    /// Gets or sets the total ms.
    /// </summary>
    /// <value>The total ms.</value>
    public float TotalMs { get => _totalMs; set => _totalMs = value; }
    /// <summary>
    /// Gets or sets the move number.
    /// </summary>
    /// <value>The move number.</value>
    public int MoveNum { get => _moveNum; set => _moveNum = value; }
    /// <summary>
    /// Gets or sets the best result.
    /// </summary>
    /// <value>The best result.</value>
    public SimulationResult BestResult { get => _bestResult; set => _bestResult = value; }
    #endregion
}

class SimulationResult
{
    #region FIELDS
    private int _points;
    private List<Tuple<int, int>> _moveList;
    private int _len;
    #endregion

    #region CONSTRUCTORS
    public SimulationResult()
    {
        _points = 0;
        _moveList = new List<Tuple<int, int>>();
        _len = 0;
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets or sets the points.
    /// </summary>
    /// <value>The points.</value>
    public int Points { get => _points; set => _points = value; }
    /// <summary>
    /// Gets or sets the move list.
    /// </summary>
    /// <value>The move list.</value>
    public List<Tuple<int, int>> MoveList { get => _moveList; set => _moveList = value; }
    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    /// <value>The length.</value>
    public int Len { get => _len; set => _len = value; }
    #endregion
}

static class Tools
{
    /// <summary>
    /// Gets the distance between 2 positions.
    /// </summary>
    /// <returns>The distance.</returns>
    /// <param name="pos1">Pos1.</param>
    /// <param name="pos2">Pos2.</param>
    public static int GetDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
    {
        DebugInfos.WriteDebugMessage("GetDistance begin");
        double distance;
        var distX = 0;
        var distY = 0;

        distX = pos1.Item1 - pos2.Item1;
        distY = pos1.Item2 - pos2.Item2;

        distance = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

        DebugInfos.WriteDebugMessage("GetDistance end");

        return Convert.ToInt32(Math.Floor(distance));
    }

    public static int Fibonacci(int n)
    {
        int w;
        if (n <= 0) return 0;
        if (n == 1) return 1;
        int u = 0;
        int v = 1;
        for (int i = 2; i <= n; i++)
        {
            w = u + v;
            u = v;
            v = w;
        };
        return v;
    }

    public static bool IsMoveValid(Tuple<int, int> move)
    {
        return (int)Math.Floor((decimal)move.Item1) != -1 && (int)Math.Floor((decimal)move.Item2) != -1;
    }

    public static float TimeDifferenceInMillisecond(DateTime t0, DateTime t1)
    {
        return (t1.Second - t0.Second) * 1000.0f + (t1.Millisecond - t0.Millisecond) / 1000.0f;
    }

    public static void PrintMove(Tuple<int, int> move)
    {
        Console.WriteLine($"{(int)Math.Floor((decimal)move.Item1)} {(int)Math.Floor((decimal)move.Item2)}");
    }
}

static class SimulationGame
{
    /// <summary>
    /// Simulation to find the best list of moves with the higest score for the game.
    /// </summary>
    /// <param name="agent">Agent.</param>
    /// <param name="simulationInfos">Simulation infos.</param>
    /// <param name="gameInfos">Game infos.</param>
    /// <param name="nash">Nash.</param>
    public static void Simulation(SimulationAgent agent, SimulationInfos simulationInfos, GameInfos gameInfos, PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("Simulation begin", debugLevel: DebugInfos.DEBUG);

        for (int i = 0; agent.TotalMs < GameInfos.TIMEOUT_FOR_A_TURN_IN_MS && agent.SimRun <= GameInfos.MAX_SIMULATIONS_RUN; i++)
        {
            int zombiesBefore;
            int zombiesAfter;
            var t0 = DateTime.UtcNow;
            SimulationResult tmpResults = new SimulationResult();

            simulationInfos.SimulationSetup(gameInfos, nash);
            zombiesBefore = simulationInfos.SimZombies.Count;
            tmpResults = Simulate(simulationInfos);
            zombiesAfter = simulationInfos.SimZombies.Count;

            if (tmpResults.Points > agent.BestResult.Points ||
                (tmpResults.Points == agent.BestResult.Points && tmpResults.Len < agent.BestResult.Len))
            {
                DebugInfos.WriteDebugMessage
                    (
                        functionName: "Simulation",
                        strings: new string[] { "Found a new best result : ",
                            $"tempResults.Points [{tmpResults.Points}] ",
                            $"& agent.BestResult.Points [{agent.BestResult.Points}] ",
                            $"& agent.BestResult.MoveList.Count [{agent.BestResult.MoveList.Count}] ",
                            $"& agent.BestResult.Len [{agent.BestResult.Len}]",
                            $"& tempResults.Len [{tmpResults.Len}]" },
                        debugLevel: DebugInfos.INFOS
                    );
                agent.BestResult = tmpResults;
                agent.NewBest = true;
                agent.MoveNum = 0;
                simulationInfos.SimCurrentBest = agent.BestResult.Points;
            }

            var t1 = DateTime.UtcNow;
            agent.TotalMs += Tools.TimeDifferenceInMillisecond(t0, t1);
            agent.SimRun++;

            if (agent.TotalMs > GameInfos.TIMEOUT_FOR_A_TURN_IN_MS)
            {
                DebugInfos.WriteDebugMessage
                    (
                        functionName: "Simulation",
                        strings: new string[] { $"agent.TotalMs = {agent.TotalMs}" },
                        debugLevel: DebugInfos.DEBUG
                    );
            }
        }

        DebugInfos.WriteDebugMessage
            (
                functionName: "Simulation end",
                strings: new string[] { $"total sim run {agent.SimRun} in {agent.TotalMs} ms\n" },
                debugLevel: DebugInfos.INFOS
            );
    }

    public static SimulationResult Simulate(SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("Simulate begin");

        Random rand = new Random();
        int i;
        SimulationResult simResult = new SimulationResult();

        simulationInfos.SimStartingRandomMovesNum = rand.Next(simulationInfos.SimMaxStartingRandomMoves + 1);

        ComputePlayerTarget(simulationInfos);

        while (!simulationInfos.SimZombiesAllDead && !simulationInfos.SimFailure && simulationInfos.SimMovesCount < GameInfos.MAX_MOVES)
        {
            // We we can't get more point that our best result so far, this simulation is a failure. Maybe we can do a break right now ! TODO: later try a break in the if.
            if ((MaxHypotheticalScore(simulationInfos) + simulationInfos.SimPoints) < simulationInfos.SimCurrentBest)
                simulationInfos.SimFailure = true;

            // Simulate a turn of the game.
            Turn(simResult.MoveList, simulationInfos);

            DebugInfos.WriteDebugMessage
                (
                    $"Simulate",
                    new string[] { $"simulationInfos.SimZombiesAllDead [{simulationInfos.SimZombiesAllDead}] ",
                            $"(MaxHypotheticalScore(simulationInfos) + simulationInfos.SimPoints) [{(MaxHypotheticalScore(simulationInfos) + simulationInfos.SimPoints)}]",
                            $"simulationInfos.SimCurrentBest [{simulationInfos.SimCurrentBest}]"
                         },
                    DebugInfos.DEBUG
                );
        }

        if (simulationInfos.SimZombiesAllDead && !simulationInfos.SimFailure)
        {
            simResult.Points = simulationInfos.SimPoints;
            simResult.Len = simulationInfos.SimMovesCount;
        }

        DebugInfos.WriteDebugMessage("Simulate end");

        return simResult;
    }

    public static void Evaluate(SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("Evaluate begin");

        int tmpPoints;
        int humanNum = simulationInfos.SimHumans.Count;
        int humanPoints = 10 * humanNum * humanNum;
        List<Zombie> killableZombies = new List<Zombie>();
        int killableZombiesLen = ZombiesInRangeOfPlayer(killableZombies, simulationInfos);

        int tmpId = (simulationInfos.SimNash.TargetingZombie) ? simulationInfos.SimNash.Target.Id : GameInfos.EMPTY_ZOMBIE;

        for (int i = 0; i < killableZombiesLen; i++)
        {
            tmpPoints = humanPoints;

            if (killableZombiesLen > 1)
            {
                tmpPoints *= Tools.Fibonacci(i + 1);
            }
            simulationInfos.SimPoints += tmpPoints;
        }

        if (killableZombies.Any(x => x.Id == tmpId))
            simulationInfos.SimNashTargetDiedThisTurn = true;

        var zombiesToRemove = new HashSet<Zombie>(killableZombies);
        simulationInfos.SimZombies.RemoveAll(x => zombiesToRemove.Contains(x));

        // TODO: Not sure this is really usefull. Maybe our target die this turn, do I not have to set the Nash target to null ? Try and see it later.
        if (killableZombiesLen > 0)
        {
            if (simulationInfos.SimZombies.Any(x => x.Id == tmpId))
                simulationInfos.SimNash.Target = simulationInfos.SimZombies.Single(x => x.Id == tmpId);
        }

        DebugInfos.WriteDebugMessage("Evaluate end");
    }

    /// <summary>
    /// Computes the player target by finding a zombie targetting a human.
    /// </summary>
    /// <param name="simulationInfos">Simulation infos.</param>
    public static void ComputePlayerTarget(SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("ComputePlayerTarget begin");

        Random rand = new Random();
        List<Zombie> zombiesThatDoNotTargetPlayer = new List<Zombie>();

        // If there is some random moves (begining of the game), we made Nash do the moves; otherwise we set a target for Nash.
        if (simulationInfos.SimStartingRandomMovesNum > 0)
        {
            simulationInfos.SimNash.NextPosition = new Tuple<int, int>(rand.Next(GameInfos.MAX_X), rand.Next(GameInfos.MAX_Y));
            simulationInfos.SimNash.TargetingZombie = false;
            simulationInfos.SimStartingRandomMovesNum--;
        }
        else
        {
            zombiesThatDoNotTargetPlayer.AddRange(simulationInfos.SimZombies.Where(x => x.ClosestHumanId != GameInfos.NASH_ID));

            // Define a zombie target for Nash : target is a zombie targetting a human, if there is at least one; any zombie in the map, otherwise.
            simulationInfos.SimNash.Target = (zombiesThatDoNotTargetPlayer.Count > 0) ?
                                                zombiesThatDoNotTargetPlayer[rand.Next(zombiesThatDoNotTargetPlayer.Count)] :
                                                simulationInfos.SimZombies[rand.Next(simulationInfos.SimZombies.Count)];

            simulationInfos.SimNash.Arrived = false;
            simulationInfos.SimNash.TargetingZombie = true;
        }

        DebugInfos.WriteDebugMessage("ComputePlayerTarget end");
    }

    public static void Turn(List<Tuple<int, int>> moveHistory, SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("Turn begin");

        foreach (Zombie zombie in simulationInfos.SimZombies)
        {
            FindZombieTarget(zombie, simulationInfos);
            MoveZombie(zombie, simulationInfos.SimNash);
        }

        moveHistory.Add(GetPlayerDestination(simulationInfos.SimNash));
        simulationInfos.SimMovesCount++;

        MovePlayer(simulationInfos.SimNash);

        Evaluate(simulationInfos);

        ZombiesEat(simulationInfos);

        if ((simulationInfos.SimHumans.Count) > 0 && (simulationInfos.SimZombies.Count > 0))
        {
            if (simulationInfos.SimNash.Arrived || simulationInfos.SimNashTargetDiedThisTurn)
            {
                ComputePlayerTarget(simulationInfos);
                simulationInfos.SimNashTargetDiedThisTurn = false;
            }
        }
        else
        {
            simulationInfos.SimFailure = (simulationInfos.SimHumans.Count <= 0);
            simulationInfos.SimZombiesAllDead = (simulationInfos.SimZombies.Count <= 0);
        }

        DebugInfos.WriteDebugMessage("Turn end");
    }

    public static void FindZombieTarget(Zombie zombie, SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("FindZombieTarget begin");

        float minDist = float.PositiveInfinity;
        float tmpDist;

        zombie.Arrived = false;

        tmpDist = Tools.GetDistance(zombie.Position, simulationInfos.SimNash.Position);
        if (tmpDist < minDist)
        {
            zombie.ClosestHumanId = GameInfos.NASH_ID;
            zombie.Target = null;
            minDist = tmpDist;
        }

        foreach (Human human in simulationInfos.SimHumans)
        {
            tmpDist = Tools.GetDistance(zombie.Position, human.Position);
            if (tmpDist <= minDist)
            {
                zombie.ClosestHumanId = human.Id;
                zombie.Target = human;
                minDist = tmpDist;
            }
        }

        DebugInfos.WriteDebugMessage("FindZombieTarget end");
    }

    public static void MoveZombie(Zombie zombie, PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("MoveZombie begin");

        Tuple<int, int> zombiePosition;
        zombie.Arrived = NextPosZombie(zombie, nash, out zombiePosition);
        zombie.Position = zombiePosition;

        DebugInfos.WriteDebugMessage("MoveZombie end");
    }

    public static void MovePlayer(PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("MovePlayer begin");

        Tuple<int, int> nashNextPos;
        nash.Arrived = NextPosNash(nash, out nashNextPos);
        nash.Position = nashNextPos;

        DebugInfos.WriteDebugMessage("MovePlayer end");
    }

    public static bool NextPosZombie(Zombie zombie, PlayerNash nash, out Tuple<int, int> posOut)
    {
        DebugInfos.WriteDebugMessage("NextPosZombie begin");

        bool arrived = false;
        Tuple<int, int> targetPos;
        float dist;
        float t;

        // If the zombie has a human target he well go for eat it; otherwise the target must be Nash, so the zombie will try to go to Nash position.
        if (zombie.Target != null)
        {
            targetPos = zombie.Target.Position;
        }
        else if (zombie.ClosestHumanId == GameInfos.NASH_ID)
        {
            targetPos = nash.Position;
        }
        else
        {
            posOut = new Tuple<int, int>(1, 1);
            return arrived;
        }

        dist = Tools.GetDistance(zombie.Position, targetPos);
        if (Math.Floor(dist) <= Zombie.MOUVEMENT)
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

        DebugInfos.WriteDebugMessage("NextPosZombie end");

        return arrived;
    }

    public static bool NextPosNash(PlayerNash nash, out Tuple<int, int> posOut)
    {
        DebugInfos.WriteDebugMessage("NextPosPlayer begin");

        Tuple<int, int> destination;
        float distance;
        float t;

        bool arrived = false;

        if (nash.Target != null || nash.NextPosition != null)
        {
            destination = GetPlayerDestination(nash);
            distance = Tools.GetDistance(nash.Position, destination);

            if (Math.Floor(distance) <= PlayerNash.MOUVEMENT)
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
            posOut = new Tuple<int, int>(1, 1);
        }

        DebugInfos.WriteDebugMessage("NextPosPlayer end");

        return arrived;
    }

    public static int ZombiesInRangeOfPlayer(List<Zombie> zombiesInRange, SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("ZombiesInRangeOfPlayer begin");

        int len = 0;
        float dx, dy;

        foreach (Zombie zombie in simulationInfos.SimZombies)
        {
            dx = zombie.Position.Item1 - simulationInfos.SimNash.Position.Item1;
            dy = zombie.Position.Item2 - simulationInfos.SimNash.Position.Item2;

            if (Math.Floor(Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2))) <= PlayerNash.RANGE)
            {
                zombiesInRange.Add(zombie);
                len++;
            }
        }

        DebugInfos.WriteDebugMessage("ZombiesInRangeOfPlayer end");

        return len;
    }

    public static void ZombiesEat(SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("ZombiesEat begin");

        List<int> zombieTargetIdTmp = new List<int>();

        foreach (Zombie zombie in simulationInfos.SimZombies.Where(x => x.ClosestHumanId != GameInfos.NASH_ID))
        {
            if (ZombieArrivedAtTarget(zombie))
            {
                if (simulationInfos.SimHumans.Any(x => x.Id == zombie.Target.Id))
                {
                    simulationInfos.SimHumans.RemoveAll(x => x.Id == zombie.Target.Id);
                }
            }
        }

        DebugInfos.WriteDebugMessage("ZombiesEat end");
    }

    public static bool ZombieArrivedAtTarget(Zombie zombie)
    {
        DebugInfos.WriteDebugMessage("ZombieArrivedAtTarget");
        if (zombie.Target == null)
            return false;

        return (int)zombie.Position.Item1 == (int)zombie.Target.Position.Item1 && (int)zombie.Position.Item2 == (int)zombie.Target.Position.Item2;
    }

    /// <summary>
    /// Gets the player destination.
    /// If Nash is targetting a zombie, the destination will be the next position of this zombie.
    /// Oterwise the destination will be the Nash.NextPosition.
    /// </summary>
    /// <returns>The player destination.</returns>
    /// <param name="nash">Nash.</param>
    public static Tuple<int, int> GetPlayerDestination(PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("GetPlayerDestination begin");

        Zombie target;
        Tuple<int, int> destination;

        if (nash.TargetingZombie && nash.Target != null)
        {
            target = nash.Target;
            NextPosZombie(target, nash, out destination);
            return destination;
        }
        else
        {
            return nash.NextPosition;
        }

        DebugInfos.WriteDebugMessage("GetPlayerDestination end");
    }

    public static int MaxHypotheticalScore(SimulationInfos simulationInfos)
    {
        DebugInfos.WriteDebugMessage("MaxHypotheticalScore begin");

        int tmpPoints = 0;
        int totPoints = 0;
        int totHumans = simulationInfos.SimHumans.Count;
        int totZombies = simulationInfos.SimZombies.Count;
        int humanPoints = 10 * totHumans * totHumans;

        for (int i = 0; i < totZombies; i++)
        {
            tmpPoints = humanPoints;
            if (totZombies > 1)
            {
                tmpPoints *= Tools.Fibonacci(i + 1);
            }
            totPoints += tmpPoints;
        }

        DebugInfos.WriteDebugMessage("MaxHypotheticalScore end");

        return totPoints;
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
}