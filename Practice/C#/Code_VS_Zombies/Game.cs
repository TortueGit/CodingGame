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
        GameInfos newGame = new GameInfos();
        SimulationAgent agent = new SimulationAgent();
        string[] inputs;

        // game loop
        while (true)
        {
            /** 
             * Initialize game infos with inputs.
            */
            inputs = Console.ReadLine().Split(' ');

            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            newGame.Nash.Position = new Tuple<int, int>(x, y);

            var humanCount = int.Parse(Console.ReadLine());
            newGame.NumberOfHumans = humanCount;
            List<Human> humans = new List<Human>();
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
                humans.Add(human);
            }
            newGame.Humans = new List<Human>(humans);

            var zombieCount = int.Parse(Console.ReadLine());
            newGame.NumberOfZombies = zombieCount;
            List<Zombie> zombies = new List<Zombie>();
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
                zombies.Add(zombie);
            }
            newGame.Zombies = new List<Zombie>(zombies);
            /** 
             * end inputs
            */

            /**
             * Simulations begin
            */
            agent.NewBest = false;
            SimulationGame.Simulation(agent, newGame, newGame.Nash);
            /**
             * Simulations end
            */

            foreach (Tuple<int, int> move in agent.BestResult.MoveList)
            {
                Console.Error.WriteLine($"{move}");
            }

            /**
             * Work with results.
            */
            if (agent.BestResult.Len == 0)
                Console.Error.WriteLine("No winning game found");

            bool foundValidMove = false;

            foundValidMove = false;
            if (!agent.NewBest)
            {
                if (Tools.IsMoveValid(agent.BestResult.MoveList[newGame.MoveNum]))
                {
                    Tools.PrintMove(agent.BestResult.MoveList[newGame.MoveNum]);
                    foundValidMove = true;
                }
            }
            else if (Tools.IsMoveValid(agent.BestResult.MoveList.First()))
            {
                Tools.PrintMove(agent.BestResult.MoveList.First());
                foundValidMove = true;
            }

            if (!foundValidMove)
            {
                for (int k = 0; k < agent.BestResult.Len; k++)
                    Console.Error.WriteLine($"move {k} is ({agent.BestResult.MoveList[k].Item1}, {agent.BestResult.MoveList[k].Item2}) " +
                        $"agent.BestResult.MoveList.Count [{agent.BestResult.MoveList.Count}] " +
                        $"agent.MoveNum [{agent.MoveNum}]");
            }

            newGame.MoveNum++;
            /**
             * Work end
            */

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

    public const int EMPTY_ZOMBIE = -4;
    public const int EMPTY_HUMAN = -5;

    public const int MAX_ZOMBIES = 100;
    public const int MAX_HUMANS = 100;
    public const int MAX_MOVES = 100;

    public const int MAX_SIMULATIONS_RUN = int.MaxValue;
    public const float TIMEOUT_FOR_A_TURN_IN_MS = 99.0f;
    public const float ACCEPTABLE_TIME_REPONSE_FOR_METHODS = 50.0f;
    #endregion

    #region FIELDS
    private int _numberOfHumans;
    private int _numberOfZombies;
    private int _moveNum;
    private PlayerNash _nash;
    private List<Human> _humans;
    private List<Zombie> _zombies;
    #endregion

    #region CONSTRUCTORS
    public GameInfos()
    {
        _nash = new PlayerNash();
        _numberOfHumans = 0;
        _numberOfZombies = 0;
        _moveNum = 0;
    }
    #endregion

    #region PROPERTIES
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
    /// Gets or sets the move number.
    /// </summary>
    /// <value>The move number.</value>
    public int MoveNum { get => _moveNum; set => _moveNum = value; }

    /// <summary>
    /// Gets or sets the player nash.
    /// </summary>
    /// <value>The nash.</value>
    internal PlayerNash Nash { get => _nash; set => _nash = value; }
    /// <summary>
    /// Gets or sets the humans.
    /// </summary>
    /// <value>The humans.</value>
    internal List<Human> Humans { get => _humans; set => _humans = value; }
    /// <summary>
    /// Gets or sets the zombies.
    /// </summary>
    /// <value>The zombies.</value>
    internal List<Zombie> Zombies { get => _zombies; set => _zombies = value; }
    #endregion

    #region METHODS
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
    private int _zombieIdMovingTo;
    private bool _targetingZombie;
    private bool _arrived;
    private Zombie _target;
    #endregion

    #region CONSTRUCTORS
    public PlayerNash()
    {
        _zombieIdMovingTo = -1;
        _targetingZombie = false;
        _arrived = false;
        _target = null;
    }

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
    /// <summary>
    /// Gets or sets the zombie identifier moving to.
    /// </summary>
    /// <value>The zombie identifier moving to.</value>
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
    /// Gets the position.
    /// </summary>
    /// <value>The position.</value>
    public Tuple<int, int> Position => _position;
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
    /// Gets or sets the closest human identifier.
    /// </summary>
    /// <value>The closest human identifier.</value>
    public int ClosestHumanId { get => _closestHumanId; set => _closestHumanId = value; }
    /// <summary>
    /// Gets or sets the target.
    /// </summary>
    /// <value>The target.</value>
    internal Human Target { get => _target; set => _target = value; }
    #endregion
}

class SimulationInfos
{
    #region Fields
    private PlayerNash _simNash;
    private Zombie _simNashTargetCpy;
    private List<Zombie> _simZombies;
    private List<Human> _simHumans;

    private bool _simFailure;
    private bool _simZombiesAllDead;
    private bool _simZombiesDiedThisTurn;
    private bool _simNashTargetDiedThisTurn;

    private int _simPoints;
    private int _simTurnNum;
    private int _simCurrentBest;
    private int _simMovesCount;
    private int _simZombieCount;
    private int _simHumanCount;

    private int _simStartingRandomMovesNum;
    private int _simMaxStartingRandomMoves;
    #endregion

    #region CONSTRUCTORS
    public SimulationInfos()
    {
        _simZombies = new List<Zombie>();
        _simHumans = new List<Human>();

        _simFailure = false;
        _simZombiesAllDead = false;
        _simZombiesDiedThisTurn = false;
        _simNashTargetDiedThisTurn = false;

        _simPoints = 0;
        _simTurnNum = 0;
        _simCurrentBest = 0;
        _simMovesCount = 0;
        _simZombieCount = 0;
        _simHumanCount = 0;

        _simStartingRandomMovesNum = -1;
        _simMaxStartingRandomMoves = -1;
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim failure.
    /// </summary>
    /// <value><c>true</c> if sim failure; otherwise, <c>false</c>.</value>
    public bool SimFailure { get => _simFailure; set => _simFailure = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim zombies all dead.
    /// </summary>
    /// <value><c>true</c> if sim zombies all dead; otherwise, <c>false</c>.</value>
    public bool SimZombiesAllDead { get => _simZombiesAllDead; set => _simZombiesAllDead = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim zombies died this turn.
    /// </summary>
    /// <value><c>true</c> if sim zombies died this turn; otherwise, <c>false</c>.</value>
    public bool SimZombiesDiedThisTurn { get => _simZombiesDiedThisTurn; set => _simZombiesDiedThisTurn = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationInfos"/> sim nash target died this turn.
    /// </summary>
    /// <value><c>true</c> if sim nash target died this turn; otherwise, <c>false</c>.</value>
    public bool SimNashTargetDiedThisTurn { get => _simNashTargetDiedThisTurn; set => _simNashTargetDiedThisTurn = value; }
    /// <summary>
    /// Gets or sets the sim points.
    /// </summary>
    /// <value>The sim points.</value>
    public int SimPoints { get => _simPoints; set => _simPoints = value; }
    /// <summary>
    /// Gets or sets the sim turn number.
    /// </summary>
    /// <value>The sim turn number.</value>
    public int SimTurnNum { get => _simTurnNum; set => _simTurnNum = value; }
    /// <summary>
    /// Gets or sets the sim current best.
    /// </summary>
    /// <value>The sim current best.</value>
    public int SimCurrentBest { get => _simCurrentBest; set => _simCurrentBest = value; }
    /// <summary>
    /// Gets or sets the sim moves count.
    /// </summary>
    /// <value>The sim moves count.</value>
    public int SimMovesCount { get => _simMovesCount; set => _simMovesCount = value; }
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
    /// Gets or sets the sim starting random moves number.
    /// </summary>
    /// <value>The sim starting random moves number.</value>
    public int SimStartingRandomMovesNum { get => _simStartingRandomMovesNum; set => _simStartingRandomMovesNum = value; }
    /// <summary>
    /// Gets or sets the sim max starting random moves.
    /// </summary>
    /// <value>The sim max starting random moves.</value>
    public int SimMaxStartingRandomMoves { get => _simMaxStartingRandomMoves; set => _simMaxStartingRandomMoves = value; }
    /// <summary>
    /// Gets or sets the sim nash.
    /// </summary>
    /// <value>The sim nash.</value>
    internal PlayerNash SimNash { get => _simNash; set => _simNash = value; }
    /// <summary>
    /// Gets or sets the sim nash target cpy.
    /// </summary>
    /// <value>The sim nash target cpy.</value>
    internal Zombie SimNashTargetCpy { get => _simNashTargetCpy; set => _simNashTargetCpy = value; }
    /// <summary>
    /// Gets or sets the sim zombies.
    /// </summary>
    /// <value>The sim zombies.</value>
    internal List<Zombie> SimZombies { get => _simZombies; set => _simZombies = value; }
    /// <summary>
    /// Gets or sets the sim humans.
    /// </summary>
    /// <value>The sim humans.</value>
    internal List<Human> SimHumans { get => _simHumans; set => _simHumans = value; }
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
        SimHumans = new List<Human>(gameInfos.Humans);

        SimFailure = false;
        SimZombiesAllDead = false;
        SimZombiesDiedThisTurn = false;
        SimNashTargetDiedThisTurn = false;

        SimPoints = 0;
        SimTurnNum = 0;
        SimMovesCount = 0;
        SimZombieCount = SimZombies.Count;
        SimHumanCount = SimHumans.Count;

        DebugInfos.WriteDebugMessage("SimulationSetup end");
    }
    #endregion
}

class SimulationAgent
{
    #region FIELDS
    private int _simRun;
    private int _moveNum;
    private float _totalMs;
    private bool _newBest;
    private SimulationResult _bestResult;
    #endregion

    #region CONSTRUCTORS
    public SimulationAgent()
    {
        _simRun = 0;
        _newBest = false;
        _totalMs = 0.0f;
        _moveNum = 0;
        _bestResult = new SimulationResult();
    }
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Gets or sets the sim run.
    /// </summary>
    /// <value>The sim run.</value>
    public int SimRun { get => _simRun; set => _simRun = value; }
    /// <summary>
    /// Gets or sets the move number.
    /// </summary>
    /// <value>The move number.</value>
    public int MoveNum { get => _moveNum; set => _moveNum = value; }
    /// <summary>
    /// Gets or sets the total ms.
    /// </summary>
    /// <value>The total ms.</value>
    public float TotalMs { get => _totalMs; set => _totalMs = value; }
    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="T:SimulationAgent"/> new best.
    /// </summary>
    /// <value><c>true</c> if new best; otherwise, <c>false</c>.</value>
    public bool NewBest { get => _newBest; set => _newBest = value; }
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

    public SimulationResult(SimulationResult simResult)
    {
        _points = simResult.Points;
        _moveList = new List<Tuple<int, int>>(simResult.MoveList);
        _len = simResult.Len;
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

static class SimulationGame
{
    /// <summary>
    /// Simulation to find the best list of moves with the higest score for the game.
    /// </summary>
    /// <param name="agent">Agent.</param>
    /// <param name="simulationInfos">Simulation infos.</param>
    /// <param name="gameInfos">Game infos.</param>
    /// <param name="nash">Nash.</param>
    public static void Simulation(SimulationAgent agent, GameInfos gameInfos, PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("Simulation begin", debugLevel: DebugInfos.DEBUG);
        SimulationInfos simulationInfos = new SimulationInfos();

        //Console.Error.WriteLine($"New Turn ! Nash is on [{nash.Position}]");

        while (agent.TotalMs < GameInfos.TIMEOUT_FOR_A_TURN_IN_MS && agent.SimRun <= GameInfos.MAX_SIMULATIONS_RUN)
        {
            var t0 = DateTime.UtcNow;
            SimulationResult tmpResults = new SimulationResult();

            simulationInfos.SimulationSetup(gameInfos, nash);

            tmpResults = Simulate(simulationInfos);

            if (tmpResults.Points > agent.BestResult.Points ||
                (tmpResults.Points == agent.BestResult.Points && tmpResults.Len < agent.BestResult.Len))
            {
                //Console.Error.WriteLine($"New good Simulation ! Nash is on [{tmpResults.MoveList.First()}] for begining.");
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
                agent.BestResult = new SimulationResult(tmpResults);
                agent.NewBest = true;
                agent.MoveNum = 0;

                // TODO: I think we don't need this ! Remove the Properties SimCurrentBest later.
                simulationInfos.SimCurrentBest = agent.BestResult.Points; 
                //Console.Error.WriteLine($"New good Simulation ! Nash is on [{tmpResults.MoveList.Last()}] for end.");
            }

            var t1 = DateTime.UtcNow;
            var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
            agent.TotalMs += simulationTime;
            agent.SimRun++;

            if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
                Console.Error.WriteLine($"Simulation time [{simulationTime}ms]");

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
            // If we can't get more point that our best result so far, this simulation is a failure. Maybe we can do a break right now ! TODO: later try a break in the if.
            if ((MaxHypotheticalScore(simulationInfos) + simulationInfos.SimPoints) < simulationInfos.SimCurrentBest)
            {
                simulationInfos.SimFailure = true;
                break;
            }

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

            if (simResult.MoveList.Count == 1)
            {
                Console.Error.WriteLine($"Unbelievable, game win in 1 move ! {simulationInfos.SimZombies.Count} | {simulationInfos.SimHumans.Count} | {simulationInfos.SimNash.Position}");
            }
        }

        DebugInfos.WriteDebugMessage("Simulate end");

        return simResult;
    }

    public static void Turn(List<Tuple<int, int>> moveHistory, SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
        var move = new Tuple<int, int>(-1, -1);
        DebugInfos.WriteDebugMessage("Turn begin");

        foreach (Zombie zombie in simulationInfos.SimZombies)
        {
            FindZombieTarget(zombie, simulationInfos);
            MoveZombie(zombie, simulationInfos.SimNash);
        }

        move = GetPlayerDestination(simulationInfos.SimNash);
        moveHistory.Add(move);
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
        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"Turn time too long [{simulationTime}ms]");
        }
    }

    /// <summary>
    /// Computes the player target by finding a zombie targetting a human.
    /// </summary>
    /// <param name="simulationInfos">Simulation infos.</param>
    public static void ComputePlayerTarget(SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
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
        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"ComputePlayerTarget time too long [{simulationTime}ms]");
        }
    }

    public static void FindZombieTarget(Zombie zombie, SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
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
        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"FindZombieTarget time too long [{simulationTime}ms]");
        }
    }

    public static void MoveZombie(Zombie zombie, PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("MoveZombie begin");

        Tuple<int, int> zombiePosition;
        zombie.Arrived = NextPosZombie(zombie, nash, out zombiePosition);
        zombie.Position = zombiePosition;

        DebugInfos.WriteDebugMessage("MoveZombie end");
    }

    public static bool NextPosZombie(Zombie zombie, PlayerNash nash, out Tuple<int, int> posOut)
    {
        var t0 = DateTime.UtcNow;
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

        DebugInfos.WriteDebugMessage("NextPosZombie end");

        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"NextPosZombie time too long [{simulationTime}ms]");
        }

        return arrived;
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

        DebugInfos.WriteDebugMessage("GetPlayerDestination end");
    }

    /// <summary>
    /// Moves the player.
    /// </summary>
    /// <param name="nash">Nash.</param>
    public static void MovePlayer(PlayerNash nash)
    {
        DebugInfos.WriteDebugMessage("MovePlayer begin");

        Tuple<int, int> nashNextPos;
        nash.Arrived = NextPosNash(nash, out nashNextPos);
        nash.Position = nashNextPos;

        DebugInfos.WriteDebugMessage("MovePlayer end");
    }

    public static bool NextPosNash(PlayerNash nash, out Tuple<int, int> posOut)
    {
        var t0 = DateTime.UtcNow;
        DebugInfos.WriteDebugMessage("NextPosPlayer begin");

        Tuple<int, int> destination;
        float distance;
        float t;

        bool arrived = false;

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

        DebugInfos.WriteDebugMessage("NextPosPlayer end");

        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"NextPosNash time too long [{simulationTime}ms]");
        }

        return arrived;
    }

    public static void Evaluate(SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
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
        {
            simulationInfos.SimNashTargetDiedThisTurn = true;
            simulationInfos.SimNash.Target = null;
        }

        var zombiesToRemove = new HashSet<Zombie>(killableZombies);
        simulationInfos.SimZombies.RemoveAll(x => zombiesToRemove.Contains(x));

        DebugInfos.WriteDebugMessage("Evaluate end");
        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"Evaluate time too long [{simulationTime}ms]");
        }
    }

    public static int ZombiesInRangeOfPlayer(List<Zombie> zombiesInRange, SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
        DebugInfos.WriteDebugMessage("ZombiesInRangeOfPlayer begin");

        int len = 0;
        float dx, dy;

        foreach (Zombie zombie in simulationInfos.SimZombies)
        {
            if (Tools.GetDistance(simulationInfos.SimNash.Position, zombie.Position) <= PlayerNash.RANGE)
            {
                zombiesInRange.Add(zombie);
                len++;
            }
        }

        DebugInfos.WriteDebugMessage("ZombiesInRangeOfPlayer end");

        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"ZombiesInRangeOfPlayer time too long [{simulationTime}ms]");
        }

        return len;
    }

    public static void ZombiesEat(SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
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

        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"ZombiesEat time too long [{simulationTime}ms]");
        }
    }

    public static bool ZombieArrivedAtTarget(Zombie zombie)
    {
        DebugInfos.WriteDebugMessage("ZombieArrivedAtTarget");
        if (zombie.Target == null)
            return false;

        return (int)zombie.Position.Item1 == (int)zombie.Target.Position.Item1 && (int)zombie.Position.Item2 == (int)zombie.Target.Position.Item2;
    }

    public static int MaxHypotheticalScore(SimulationInfos simulationInfos)
    {
        var t0 = DateTime.UtcNow;
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

        var t1 = DateTime.UtcNow;
        var simulationTime = Tools.TimeDifferenceInMillisecond(t0, t1);
        if (simulationTime > GameInfos.ACCEPTABLE_TIME_REPONSE_FOR_METHODS)
        {
            Console.Error.WriteLine($"MaxHypotheticalScore time too long [{simulationTime}ms]");
        }

        return totPoints;
    }
}

static class Tools
{
    /// <summary>
    /// Gets the distance between 2 positions.
    /// </summary>
    /// <returns>The distance.</returns>
    /// <param name="pos1">Pos1.</param>
    /// <param name="pos2">Pos2.</param>
    public static float GetDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
    {
        DebugInfos.WriteDebugMessage("GetDistance begin");
        double distance;
        var distX = 0;
        var distY = 0;

        distX = pos1.Item1 - pos2.Item1;
        distY = pos1.Item2 - pos2.Item2;

        distance = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

        DebugInfos.WriteDebugMessage("GetDistance end");

        return (float)distance;
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
}