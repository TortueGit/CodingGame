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
            try
            {
                inputs = Console.ReadLine().Split(' ');
            }
            catch (NullReferenceException ex)
            {
                Console.Error.WriteLine(string.Format("This game is fucked ! {0}", ex.Message));
                break;
            }
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
                myGame.AddZombie(zombie);
            }

            Tools.Simulation(agent, simulationInfos, myGame, myGame.Nash);

            DebugInfos.WriteDebugMessage
                (
                    functionName: "Main",
                    strings: new string[] { $"agent.NewBest = {agent.NewBest}, {agent.BestResult.Len}, {agent.BestResult.MoveList.Count}" },
                    debugLevel: DebugInfos.INFOS
                );

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
    private Nash _nash = new Nash();
    private int _numberOfHumans = 0;
    private int _numberOfZombies = 0;
    private List<Human> _humans = new List<Human>();
    private List<Zombie> _zombies = new List<Zombie>();
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
    public Nash Nash { get => _nash; set => _nash = value; }
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
            return _humans.Exists(x => x.Id == (humanOrZombie as Human).Id);
        else if (humanOrZombie.GetType().Equals(typeof(Zombie)))
            return _zombies.Exists(x => x.Id == (humanOrZombie as Zombie).Id);
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
            _humans.Add(human);
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
        Tools.SetClosestHumanForZombie(zombie, Nash, Humans);
        if (!IsExist<Zombie>(zombie))
        {
            _zombies.Add(zombie);
            DebugInfos.WriteDebugMessage("AddZombie", new string[]
                {
                    "zombie.Id = ", zombie.Id.ToString(), " ",
                    "zombie.Position = ", zombie.Position.ToString(), " ",
                    "zombie.NextPosition = ", zombie.NextPosition.ToString()
                });
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
                    "human.Id = ", _humans.Single(x => x.Id == human.Id).Id.ToString(), " ",
                    "human.Position = ", _humans.Single(x => x.Id == human.Id).Position.ToString()
            });

        _humans.Single(x => x.Id == human.Id).Position = human.Position;

        DebugInfos.WriteDebugMessage("UpdateHumanPosition end", new string[]
            {
                    "human.Id = ", _humans.Single(x => x.Id == human.Id).Id.ToString(), " ",
                    "human.Position = ", _humans.Single(x => x.Id == human.Id).Position.ToString()
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
                    "zombie.Id = ", _zombies.Single(x => x.Id == zombie.Id).Id.ToString(), " ",
                    "zombie.Position = ", _zombies.Single(x => x.Id == zombie.Id).Position.ToString(), " ",
                    "zombie.NextPosition = ", _zombies.Single(x => x.Id == zombie.Id).NextPosition.ToString()
            });

        _zombies.Single(x => x.Id == zombie.Id).Position = zombie.Position;
        _zombies.Single(x => x.Id == zombie.Id).NextPosition = zombie.NextPosition;

        DebugInfos.WriteDebugMessage("UpdateZombiePosition end", new string[]
            {
                    "zombie.Id = ", _zombies.Single(x => x.Id == zombie.Id).Id.ToString(), " ",
                    "zombie.Position = ", _zombies.Single(x => x.Id == zombie.Id).Position.ToString(), " ",
                    "zombie.NextPosition = ", _zombies.Single(x => x.Id == zombie.Id).NextPosition.ToString()
            });
    }
    #endregion
}

class Nash
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
    public Tuple<int, int> Position{ get => _position; set => _position = value; }
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

    #region METHODS
    /// <summary>
    /// Sets the closest human.
    /// </summary>
    /// <param name="human">Human.</param>
    /// <param name="nbTurnToReach">Nb turn to reach.</param>
    public void SetClosestHuman(Human human, int nbTurnToReach)
    {
        if (human == null)
        {
            ClosestHumanId = -1;
            Target = null;
            _nbTurnToReachHuman = nbTurnToReach;
        }
        else
        {
            ClosestHumanId = human.Id;
            Target = human;
            _nbTurnToReachHuman = nbTurnToReach;
        }
    }
    #endregion
}

class SimulationInfos
{
    #region Fields
    private Nash _simNash;
    private Zombie _simNashTargetCpy;
    private List<Zombie> _simZombies = new List<Zombie>();
    private List<Human> _simHumans = new List<Human>();

    private bool _simFailure = false;
    private bool _simZombiesAllDead = false;
    private bool _simZombiesDiedThisTurn = false;
    private bool _simNashTargetDiedThisTurn = false;

    private int _simPoints;
    private int _simTurnNum;
    private int _simCurrentBest;
    private int _simMovesCount;
    private int _simZombieCount;
    private int _simHumanCount;

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
    public Nash SimNash { get => _simNash; set => _simNash = value; }
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
    public void SimulationSetup(GameInfos gameInfos, Nash nash)
    {
        DebugInfos.WriteDebugMessage("SimulationSetup begin");

        _simNash = nash;

        _simZombies = new List<Zombie>();
        _simHumans = new List<Human>();

        for (int i = 0; i < gameInfos.Zombies.Count; i++)
            _simZombies.Add(gameInfos.Zombies[i]);

        for (int i = 0; i < gameInfos.Humans.Count; i++)
            _simHumans.Add(gameInfos.Humans[i]);

        DebugInfos.WriteDebugMessage("SimulationSetup", new string[]
            {
                $"_simZombies.Count = {_simZombies.Count} ",
                $"_simHumans.Count = {_simHumans.Count}",
            }, debugLevel: DebugInfos.DEBUG);

        SimFailure = false;
        SimZombiesAllDead = false;
        SimZombiesDiedThisTurn = false;
        SimNashTargetDiedThisTurn = false;

        SimPoints = 0;
        SimTurnNum = 0;
        SimMovesCount = 0;
        SimZombieCount = gameInfos.Zombies.Count;
        SimHumanCount = gameInfos.Humans.Count;

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
    private int _points = 0;
    private List<Tuple<int, int>> _moveList = new List<Tuple<int, int>>();
    private int _len = 0;
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

    /// <summary>
    /// Is the point on the map in nash range.
    /// </summary>
    /// <returns><c>true</c>, if in nash range, <c>false</c> otherwise.</returns>
    /// <param name="pos">Position we want to know.</param>
    /// <param name="nashPos">Nash position.</param>
    public static bool IsInNashRange(Tuple<int, int> pos, Tuple<int, int> nashPos)
    {
        DebugInfos.WriteDebugMessage("IsInNashRange begin");
        var dx = pos.Item1 - nashPos.Item1;
        var dy = pos.Item2 - nashPos.Item2;

        if (Math.Floor(Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2))) <= Nash.RANGE)
        {
            DebugInfos.WriteDebugMessage("IsInNashRange end");
            return true;
        }

        DebugInfos.WriteDebugMessage("IsInNashRange end");

        return false;
    }

    /// <summary>
    /// Number of turn to reach the position.
    /// </summary>
    /// <returns>The number of turn</returns>
    /// <param name="distance">Distance.</param>
    /// <param name="mouvement">Mouvement allowed for the char (zombie or Nash).</param>
    public static int NumberOfTurnToReachPos(int distance, int mouvement)
    {
        DebugInfos.WriteDebugMessage("NumberOfTurnToReachPos");

        return Convert.ToInt32(Math.Floor((decimal)(distance / mouvement)));
    }

    /// <summary>
    /// Simulation to find the best list of moves with the higest score for the game.
    /// </summary>
    /// <param name="agent">Agent.</param>
    /// <param name="simulationInfos">Simulation infos.</param>
    /// <param name="gameInfos">Game infos.</param>
    /// <param name="nash">Nash.</param>
    public static void Simulation(SimulationAgent agent, SimulationInfos simulationInfos, GameInfos gameInfos, Nash nash)
    {
        DebugInfos.WriteDebugMessage("Simulation begin", debugLevel: DebugInfos.INFOS);

        for (int i = 0; agent.TotalMs < GameInfos.TIMEOUT_FOR_A_TURN_IN_MS && agent.SimRun <= GameInfos.MAX_SIMULATIONS_RUN; i++)
        {
            var t0 = DateTime.UtcNow;

            simulationInfos.SimulationSetup(gameInfos, nash);
            int zombiesBefore = simulationInfos.SimZombies.Count;
            SimulationResult tmpResults = Simulate(simulationInfos);
            int zombiesAfter = simulationInfos.SimZombies.Count;

            if (tmpResults.Points > agent.BestResult.Points ||
                (tmpResults.Points == agent.BestResult.Points && tmpResults.Len > agent.BestResult.Len))
            {
                agent.BestResult = tmpResults;
                agent.NewBest = true;
                agent.MoveNum = 0;
                simulationInfos.SimCurrentBest = agent.BestResult.Points;
            }

            var t1 = DateTime.UtcNow;
            agent.TotalMs += TimeDifferenceInMillisecond(t0, t1);
            agent.SimRun++;

            if (agent.TotalMs > GameInfos.TIMEOUT_FOR_A_TURN_IN_MS)
            {
                DebugInfos.WriteDebugMessage
                    (
                        functionName: "Simulation", 
                        strings: new string[] { $"agent.TotalMs = {agent.TotalMs}" }, 
                        debugLevel: DebugInfos.INFOS
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

        simResult.Points = 0;

        simulationInfos.SimStartingRandomMovesNum = rand.Next(simulationInfos.SimMaxStartingRandomMoves + 1);

        ComputePlayerTarget(simulationInfos);

        while (!simulationInfos.SimZombiesAllDead && !simulationInfos.SimFailure && simulationInfos.SimMovesCount < GameInfos.MAX_MOVES)
        {
            if ((MaxHypotheticalScore(simulationInfos) + simulationInfos.SimPoints) < simulationInfos.SimCurrentBest)
                simulationInfos.SimFailure = true;
            Turn(simResult.MoveList, simulationInfos);
        }

        if (simulationInfos.SimZombiesAllDead && !simulationInfos.SimFailure)
        {
            simResult.Points = simulationInfos.SimPoints;
            simResult.Len = simulationInfos.SimMovesCount;
        }

        DebugInfos.WriteDebugMessage("Simulate end");

        return simResult;
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

        if (simulationInfos.SimStartingRandomMovesNum > 0)
        {
            simulationInfos.SimNash.NextPosition = new Tuple<int, int>(rand.Next(GameInfos.MAX_X), rand.Next(GameInfos.MAX_Y));
            simulationInfos.SimNash.TargetingZombie = false;
            simulationInfos.SimStartingRandomMovesNum--;
        }
        else
        {
            zombiesThatDoNotTargetPlayer.AddRange(simulationInfos.SimZombies.Where(x => x.ClosestHumanId != GameInfos.NASH_ID));

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

        int i, j, k;
        bool foundHuman;

        foreach(Zombie zombie in simulationInfos.SimZombies)
        {
            FindZombieTarget(zombie, simulationInfos);
            MoveZombie(zombie);
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

        tmpDist = GetDistance(zombie.Position, simulationInfos.SimNash.Position);
        if (tmpDist < minDist)
        {
            zombie.ClosestHumanId = GameInfos.NASH_ID;
            minDist = tmpDist;
        }

        for (int i = 0; i < simulationInfos.SimHumans.Count; i++)
        {
            tmpDist = GetDistance(zombie.Position, simulationInfos.SimHumans[i].Position);
            if (tmpDist <= minDist)
            {
                zombie.ClosestHumanId = simulationInfos.SimHumans[i].Id;
                zombie.Target = simulationInfos.SimHumans[i];
                minDist = tmpDist;
            }
        }

        DebugInfos.WriteDebugMessage("FindZombieTarget end");
    }

    public static void MoveZombie(Zombie zombie)
    {
        DebugInfos.WriteDebugMessage("MoveZombie begin");

        Tuple<int, int> zombiePosition;
        zombie.Arrived = NextPosZombie(zombie, out zombiePosition);
        zombie.Position = zombiePosition;

        DebugInfos.WriteDebugMessage("MoveZombie end");
    }

    public static void MovePlayer(Nash nash)
    {
        DebugInfos.WriteDebugMessage("MovePlayer begin");

        Tuple<int, int> nashNextPos;
        nash.Arrived = NextPosNash(nash, out nashNextPos);
        nash.Position = nashNextPos;

        DebugInfos.WriteDebugMessage("MovePlayer end");
    }

    public static bool NextPosZombie(Zombie zombie, out Tuple<int, int> posOut)
    {
        DebugInfos.WriteDebugMessage("NextPosZombie begin");

        bool arrived = false;

        if (zombie.Target != null)
        {
            float dft = GetDistance(zombie.Position, zombie.Target.Position);
            float t;

            if (Math.Floor(dft) <= Zombie.MOUVEMENT)
            {
                arrived = true;
                posOut = new Tuple<int, int>(zombie.Target.Position.Item1, zombie.Target.Position.Item2);
            }
            else
            {
                t = Zombie.MOUVEMENT / dft;
                posOut = new Tuple<int, int>(
                    zombie.Position.Item1 + (int)Math.Floor(t * (zombie.Target.Position.Item1 - zombie.Position.Item1)), 
                    zombie.Position.Item2 + (int)Math.Floor(t * (zombie.Target.Position.Item2 - zombie.Position.Item2))
                    );
            }
        }
        else
        {
            posOut = new Tuple<int, int>(1, 1);
        }

        DebugInfos.WriteDebugMessage("NextPosZombie end");

        return arrived;
    }

    public static bool NextPosNash(Nash nash, out Tuple<int, int> posOut)
    {
        DebugInfos.WriteDebugMessage("NextPosPlayer begin");

        Tuple<int, int> dst;
        float dft;
        float t;
        bool arrived = false;

        if (nash.Target != null || nash.NextPosition != null)
        {
            dst = GetPlayerDestination(nash);
            dft = GetDistance(nash.Position, dst);

            if (Math.Floor(dft) <= Nash.MOUVEMENT)
            {
                arrived = true;
                posOut = new Tuple<int, int>(dst.Item1, dst.Item2);
            }
            else
            {
                t = Nash.MOUVEMENT / dft;
                posOut = new Tuple<int, int>(
                        nash.Position.Item1 + (int)Math.Floor(t * (dst.Item1 - nash.Position.Item1)),
                        nash.Position.Item2 + (int)Math.Floor(t * (dst.Item2 - nash.Position.Item2))
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
                tmpPoints *= Fibonacci(i + 1);
            }
            simulationInfos.SimPoints += tmpPoints;
        }
        if (killableZombies.Any(x => x.Id == tmpId))
            simulationInfos.SimNashTargetDiedThisTurn = true;

        var zombiesToRemove = new HashSet<Zombie>(killableZombies);
        simulationInfos.SimZombies.RemoveAll(x => zombiesToRemove.Contains(x));

        if (killableZombiesLen > 0)
        {
            if (simulationInfos.SimZombies.Any(x => x.Id == tmpId))
                simulationInfos.SimNash.Target = simulationInfos.SimZombies.Single(x => x.Id == tmpId);
        }

        DebugInfos.WriteDebugMessage("Evaluate end");
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

            if (Math.Floor(Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy,2))) <= Nash.RANGE)
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

        int i, j, k;
        List<int> zombieTargetIdTmp = new List<int>();

        foreach (Zombie zombie in simulationInfos.SimZombies.Where(x => x.ClosestHumanId != GameInfos.NASH_ID))
        {
            if (ZombieArrivedAtTarget(zombie))
            {
                if (simulationInfos.SimHumans.Any(x => x.Id == zombie.Target.Id))
                {
                    simulationInfos.SimHumans.RemoveAll(x => x.Id == zombie.Target.Id);
                    SetClosestHumanForZombie(zombie, simulationInfos.SimNash, simulationInfos.SimHumans);
                }
            }
        }

        DebugInfos.WriteDebugMessage("ZombiesEat end");
    }

    public static void SetClosestHumanForZombie(Zombie zombie, Nash nash, List<Human> humans)
    {
        DebugInfos.WriteDebugMessage("SetClosestHuman begin");

        var distance = 0;
        var nbTurn = -1;

        // Determine the shortest nb of turn to reach a human in the map for the zombie (zombies are moving to the closest human).
        foreach (Human human in humans)
        {
            distance = GetDistance(zombie.Position, human.Position);
            var nt = NumberOfTurnToReachPos(distance, Zombie.MOUVEMENT);
            if (nbTurn == -1 || nbTurn > nt)
            {
                zombie.SetClosestHuman(human, nt);
            }
        }

        // Zombie can move to Nash, if he's closest.
        distance = GetDistance(zombie.Position, nash.Position);
        nbTurn = NumberOfTurnToReachPos(distance, Zombie.MOUVEMENT);
        if (zombie.NbTurnToReachHuman > nbTurn)
            zombie.SetClosestHuman(null, nbTurn);

        DebugInfos.WriteDebugMessage("SetClosestHuman end");
    }

    public static bool ZombieArrivedAtTarget(Zombie zombie)
    {
        DebugInfos.WriteDebugMessage("ZombieArrivedAtTarget");
        if (zombie.Target == null)
            return false;

        return (int)zombie.Position.Item1 == (int)zombie.Target.Position.Item1 && (int)zombie.Position.Item2 == (int)zombie.Target.Position.Item2;
    }

    public static Tuple<int, int> GetPlayerDestination(Nash nash)
    {
        DebugInfos.WriteDebugMessage("GetPlayerDestination begin");

        Zombie target;
        Tuple<int, int> dst;
        if (nash.TargetingZombie)
        {
            target = nash.Target;
            NextPosZombie(target, out dst);
            return dst;
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
        int humanPoints = 10 * totHumans * totHumans;

        for (int i = 0; i < simulationInfos.SimZombies.Count; i++)
        {
            tmpPoints = humanPoints;
            if (simulationInfos.SimZombies.Count > 1)
            {
                tmpPoints *= Fibonacci(i + 1);
            }
            totPoints += tmpPoints;
        }

        DebugInfos.WriteDebugMessage("MaxHypotheticalScore end");

        return totPoints;
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