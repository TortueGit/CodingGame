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
                var human = new Human(humanId, new Tuple<int, int>(humanX, humanY));
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

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Tuple<int, int> nextMovement = myGame.WhereShouldNashMove();
            Console.WriteLine(string.Format("{0} {1}", nextMovement.Item1, nextMovement.Item2)); // Your destination coordinates
        }
    }
}

class GameInfos
{
    public const int MAX_X = 16000;
    public const int MAX_Y = 9000;
    public const int NASH_ID = -1;
    public const int MAX_MOVES = 100;

    private Nash _nash = new Nash();
    private int _numberOfHumans = 0;
    private int _numberOfZombies = 0;
    private List<Human> _humans = new List<Human>();
    private List<Zombie> _zombies = new List<Zombie>();
    private int _moveNum = 0;

    public GameInfos()
    {
    }

    public List<Zombie> Zombies => _zombies;
    public List<Human> Humans => _humans;

    public int MoveNum
    {
        get => _moveNum;
        set => _moveNum = value;
    }

    /// <summary>
    /// Sets Nash position.
    /// </summary>
    /// <param name="pos">Position.</param>
    public void SetNashPosition(Tuple<int, int> pos)
    {
        _nash.Position = pos;
        if (_nash.NextPosition is null)
            _nash.NextPosition = pos;

        DebugInfos.WriteDebugMessage("SetNashPosition", new string[]
            {
                "_nash.Position = ", _nash.Position.ToString(), " ",
                "_nash.NextPosition = ", _nash.NextPosition.ToString()
            });
    }

    /// <summary>
    /// Gets or sets the number of humans.
    /// </summary>
    /// <value>The number of humans.</value>
    public int NumberOfHumans
    {
        get => _numberOfHumans;
        set => _numberOfHumans = value;
    }

    /// <summary>
    /// Gets or sets the number of zombies.
    /// </summary>
    /// <value>The number of zombies.</value>
    public int NumberOfZombies
    {
        get => _numberOfZombies;
        set => _numberOfZombies = value;
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
        SetClosestHuman(zombie);
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

    public Tuple<int, int> WhereShouldNashMove()
    {
        Tuple<int, int> nashNextPosition = _nash.NextPosition;

        DebugInfos.WriteDebugMessage("WhereShouldNashMove : ", new string[]
            {
                "IsNashMoving() : ", IsNashMoving().ToString(), " ",
                "_zombies.Any(x => x.Id == _nash.ZombieIdMovingTo) : ", _zombies.Any(x => x.Id == _nash.ZombieIdMovingTo).ToString(), " ",
                "_zombies.Any(x => x.ClosestHumanId != -1) : ", _zombies.Any(x => x.ClosestHumanId != -1).ToString()
            });

        if (!IsNashMoving() && !_zombies.Any(x => x.Id == _nash.ZombieIdMovingTo) /*&& _zombies.Any(x => x.ClosestHumanId != -1)*/)
        {
            Zombie dangerousZombie = GetMostDangerousZombie();
            int nbTurnToReachHuman = Tools.NumberOfTurnToReachPos(Tools.GetDistance(_nash.Position, dangerousZombie.NextPosition), Nash.MOUVEMENT);
            if (nbTurnToReachHuman <= dangerousZombie.NbTurnToReachHuman)
            {
                DebugInfos.WriteDebugMessage("WhereShouldNashMove goToZombie", new string[]
                    {
                        "nbTurnToReachHuman = ", nbTurnToReachHuman.ToString(), " ",
                        "dangerousZombie.NbTurnToReachHuman = ", dangerousZombie.NbTurnToReachHuman.ToString()
                    });
                nashNextPosition = dangerousZombie.NextPosition;
                _nash.ZombieIdMovingTo = dangerousZombie.Id;
            }
            else
            {
                DebugInfos.WriteDebugMessage("WhereShouldNashMove humanIsDead", new string[]
                    {
                        "nbTurnToReachHuman = ", nbTurnToReachHuman.ToString(), " ",
                        "dangerousZombie.NbTurnToReachHuman = ", dangerousZombie.NbTurnToReachHuman.ToString()
                    });

                if (_humans.Exists(x => x.Id == dangerousZombie.ClosestHumanId))
                {
                    _humans.Remove(_humans.Single(x => x.Id == dangerousZombie.ClosestHumanId));
                    nashNextPosition = GetClosestHumanFromNash().Position;
                }
            }
        }
        else if (IsNashMoving())
        {
            if (_nash.ZombieIdMovingTo != -1)
            {
                if (Tools.GetDistance(_nash.Position, _zombies.Single(x => x.Id == _nash.ZombieIdMovingTo).Position) < 2000)
                {
                    _zombies.Remove(_zombies.Single(x => x.Id == _nash.ZombieIdMovingTo));
                    nashNextPosition = _nash.Position;
                }
                else
                    nashNextPosition = _zombies.Single(x => x.Id == _nash.ZombieIdMovingTo).NextPosition;
            }
        }

        DebugInfos.WriteDebugMessage("WhereShouldNashMove", new string[]
            {
                "nashNextPosition = ", nashNextPosition.ToString()
            });

        _nash.NextPosition = nashNextPosition;

        return nashNextPosition;
    }

    private bool IsNashMoving()
    {
        return !_nash.Position.Equals(_nash.NextPosition);
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

    private void SetClosestHuman(Zombie zombie)
    {
        var distance = 0;
        var nbTurn = -1;

        // Determine the shortest nb of turn to reach a human in the map for the zombie (zombies are moving to the closest human).
        foreach (Human human in _humans)
        {
            distance = Tools.GetDistance(zombie.Position, human.Position);
            var nt = Tools.NumberOfTurnToReachPos(distance, Zombie.MOUVEMENT);
            if (nbTurn == -1 || nbTurn > nt)
            {
                zombie.SetClosestHuman(human.Id, nt);
            }
        }

        // Zombie can move to Nash, if he's closest.
        distance = Tools.GetDistance(zombie.Position, _nash.Position);
        nbTurn = Tools.NumberOfTurnToReachPos(distance, Zombie.MOUVEMENT);
        if (zombie.NbTurnToReachHuman > nbTurn)
            zombie.SetClosestHuman(-1, nbTurn);
    }

    private Human GetClosestHumanFromNash()
    {
        var distance = 0;
        Human closestHuman = null;

        foreach (Human human in _humans)
        {
            var dist = Tools.GetDistance(_nash.Position, human.Position);
            if (distance == 0 || distance > dist)
            {
                distance = dist;
                closestHuman = human;
            }
        }

        return closestHuman;
    }

    /// <summary>
    /// Gets the badest human.
    /// </summary>
    /// <returns>The badest human.</returns>
    private Zombie GetMostDangerousZombie()
    {
        //return _zombies.Where(x => x.ClosestHumanId != -1).OrderBy(x => x.NbTurnToReachHuman).First();
        return _zombies.OrderBy(x => x.NbTurnToReachHuman).First();
    }
}

class Nash
{
    public const int MOUVEMENT = 1000;
    public const int RANGE = 2000;

    private Tuple<int, int> _position;
    private Tuple<int, int> _nextPosition;
    private int _zombieIdMovingTo = -1;
    private bool _targetingZombie = false;
    private bool _arrived = false;
    private Zombie _target = null;

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    public Tuple<int, int> Position
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>
    /// Gets or sets the next position.
    /// </summary>
    /// <value>The next position.</value>
    public Tuple<int, int> NextPosition
    {
        get => _nextPosition;
        set => _nextPosition = value;
    }

    public int ZombieIdMovingTo
    {
        get => _zombieIdMovingTo;
        set => _zombieIdMovingTo = value;
    }

    public bool TargetingZombie
    {
        get => _targetingZombie;
        set => _targetingZombie = value;
    }

    public bool Arrived
    {
        get => _arrived;
        set => _arrived = value;
    }

    public Zombie Target
    {
        get => _target;
        set => _target = value;
    }
}

class Human
{
    private int _id;
    private Tuple<int, int> _position;

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

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    public int Id
    {
        get => _id;
    }

    /// <summary>
    /// Gets or sets the position.
    /// </summary>
    /// <value>The position.</value>
    public Tuple<int, int> Position
    {
        get => _position;
        set => _position = value;
    }
}

class Zombie
{
    private int _id;
    private Tuple<int, int> _position;
    private Tuple<int, int> _nextPosition;
    private int _closestHumanId;    // ID of a human or -1 if the closest human is Nash.
    private int _nbTurnToReachHuman;    // The number of turn the zombie needs to reach the closest human.
    private bool _arrived;
    private Human _target = null;

    public const int MOUVEMENT = 400;

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

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    public int Id => _id;

    /// <summary>
    /// Gets or sets the closest human identifier.
    /// </summary>
    /// <value>The closest human identifier.</value>
    public int ClosestHumanId => _closestHumanId;

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
    public bool Arrived { get => _arrived; set => _arrived = value; }
    internal Human Target { get => _target; set => _target = value; }

    public void SetClosestHuman(int humanId, int nbTurnToReach)
    {
        _closestHumanId = humanId;
        _nbTurnToReachHuman = nbTurnToReach;
    }
}

class SimulationInfos
{
    #region Fields
    private Nash _simNash;
    private Zombie _simNashTargetCpy;
    private List<Zombie> _simZombies = new List<Zombie>();
    private List<Human> _simHumans = new List<Human>();
    private SimulationResult _bestResult = new SimulationResult();

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
    public int SimZombieCount => _simZombieCount;
    public int SimHumanCount => _simHumanCount;
    public int SimMaxStartingRandomMoves => _simMaxStartingRandomMoves;

    public Nash SimNash { get => _simNash; set => _simNash = value; }
    public Zombie SimNashTargetCpy { get => _simNashTargetCpy; set => _simNashTargetCpy = value; }
    public List<Zombie> SimZombies { get => _simZombies; set => _simZombies = value; }
    public List<Human> SimHumans { get => _simHumans; set => _simHumans = value; }
    public SimulationResult BestResult { get => _bestResult; set => _bestResult = value; }
    public int SimCurrentBest { get => _simCurrentBest; set => _simCurrentBest = value; }
    public int SimStartingRandomMovesNum { get => _simStartingRandomMovesNum; set => _simStartingRandomMovesNum = value; }
    public bool SimZombiesAllDead { get => _simZombiesAllDead; set => _simZombiesAllDead = value; }
    public bool SimFailure { get => _simFailure; set => _simFailure = value; }
    public int SimMovesCount { get => _simMovesCount; set => _simMovesCount = value; }
    public int SimPoints { get => _simPoints; set => _simPoints = value; }
    #endregion

    public void SimulationSetup(GameInfos gameInfos, Nash nash)
    {
        _simNash = nash;

        for (int i = 0; i < gameInfos.Zombies.Count; i++)
            _simZombies.Add(gameInfos.Zombies[i]);

        for (int i = 0; i < gameInfos.Humans.Count; i++)
            _simHumans.Add(gameInfos.Humans[i]);

        _simFailure = false;
        SimZombiesAllDead = false;
        _simZombiesDiedThisTurn = false;
        _simNashTargetDiedThisTurn = false;

        SimPoints = 0;
        _simTurnNum = 0;
        SimMovesCount = 0;
        _simZombieCount = gameInfos.Zombies.Count;
        _simHumanCount = gameInfos.Humans.Count;

        _simStartingRandomMovesNum = 0;
        _simMaxStartingRandomMoves = 3;
    }
}

class SimulationAgent
{
    private int _simRun = 0;
    private bool _newBest = false;
    private float _totalMs = 0;

    public int SimRun
    {
        get => _simRun;
        set => _simRun = value;
    }

    public bool NewBest
    {
        get => _newBest;
        set => _newBest = value;
    }

    public float TotalMs
    {
        get => _totalMs;
        set => _totalMs = value;
    }
}

class SimulationResult
{
    private int _points = 0;
    private List<Tuple<int, int>> _moveList = new List<Tuple<int, int>>();
    private int _len = 0;

    public int Points
    { 
        get => _points;
        set => _points = value;
    }

    public List<Tuple<int, int>> MoveList
    {
        get => _moveList;
        set => _moveList = value;
    }

    public int Len
    {
        get => _len;
        set => _len = value;
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
    public static int GetDistance(Tuple<int, int> pos1, Tuple<int, int> pos2)
    {
        double distance;
        var distX = 0;
        var distY = 0;

        /*if (pos1.Item1 > pos2.Item2)
            distX = pos1.Item1 - pos2.Item1;
        else
            distX = pos2.Item1 - pos1.Item1;*/
        distX = pos1.Item1 - pos2.Item1;

        /*if (pos1.Item2 > pos2.Item2)
            distY = pos1.Item2 - pos2.Item2;
        else
            distY = pos2.Item2 - pos1.Item2;*/
        distY = pos1.Item2 - pos2.Item2;

        /*distance = Math.Sqrt(Math.Abs(Math.Pow(distX, 2) + Math.Pow(distY, 2)));*/
        distance = Math.Sqrt(Math.Pow(distX, 2) + Math.Pow(distY, 2));

        /*try
        {
            return Convert.ToInt32(Math.Round(distance));
        }
        catch (OverflowException ex)
        {
            DebugInfos.WriteDebugMessage("GetDistance", new string[]
                {
                    "distance = ", distance.ToString(), " ",
                    "ex.Message = ", ex.Message
                });
            return 0;
        }*/
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
        var dx = pos.Item1 - nashPos.Item1;
        var dy = pos.Item2 - nashPos.Item2;

        if (Math.Floor(Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2))) <= Nash.RANGE)
            return true;

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
        // var mvt = Convert.ToInt32(Math.Sqrt(Math.Pow(mouvement, 2) + Math.Pow(mouvement, 2)));
        return Convert.ToInt32(Math.Floor((decimal)(distance / mouvement)));
    }

    public static void Simulation(SimulationAgent agent, SimulationInfos simulationInfos, GameInfos gameInfos, Nash nash)
    {
        for (int i = 0; agent.TotalMs < 140.0f; i++)
        {
            var t0 = DateTime.UtcNow;

            simulationInfos.SimulationSetup(gameInfos, nash);
            int zombiesBefore = simulationInfos.SimZombieCount;
            SimulationResult tmpResults = simulate();
            int zombiesAfter = simulationInfos.SimZombieCount;

            if (tmpResults.Points > simulationInfos.BestResult.Points ||
                (tmpResults.Points == simulationInfos.BestResult.Points && tmpResults.Len > simulationInfos.BestResult.Len))
            {
                simulationInfos.BestResult = tmpResults;
                agent.NewBest = true;
                moveNum = 0;
                simulationInfos.SimCurrentBest = simulationInfos.BestResult.Points;
            }

            var t1 = DateTime.UtcNow;
            agent.TotalMs += TimeDifferenceInMillisecond(t0, t1);
            agent.SimRun++;
        }

        DebugInfos.WriteDebugMessage("Simulation", new string[]
            {
                $"total sim run {agent.SimRun} in {agent.TotalMs} ms\n"
            });
    }

    public static SimulationResult Simulate(SimulationInfos simulationInfos)
    {
        Random rand = new Random();
        int i;
        SimulationResult simResult;

        for (i = 0; i < MAX_MOVES; i++)
        {
            simResult.MoveList[i].Item1 = -1.0f;
            simResult.MoveList[i].Item2 = -1.0f;
        }

        simResult.Points = 0;

        simulationInfos.SimStartingRandomMovesNum = rand.Next(simulationInfos.SimMaxStartingRandomMoves + 1);

        ComputePlayerTarget(simulationInfos);

        while (!simulationInfos.SimZombiesAllDead && !simulationInfos.SimFailure && simulationInfos.SimMovesCount < GameInfos.MAX_MOVES)
        {
            if ((MaxHypotheticalScore(simulationInfos) + simulationInfos.SimPoints) < simulationInfos.SimCurrentBest)
                simulationInfos.SimFailure = true;
            turn(simResult.moveList);
        }

        if (simZombiesAllDead && !simFailure)
        {
            simResult.points = simPoints;
            simResult.len = simMovesCount;
        }

        return simResult;
    }

    /// <summary>
    /// Computes the player target by finding a zombie targetting a human.
    /// </summary>
    /// <param name="simulationInfos">Simulation infos.</param>
    public static void ComputePlayerTarget(SimulationInfos simulationInfos)
    {
        Random rand = new Random();
        List<Zombie> zombiesThatDoNotTargetPlayer;
        int len = 0;

        if (simulationInfos.SimStartingRandomMovesNum > 0)
        {
            simulationInfos.SimNash.NextPosition.Item1 = rand.Next(GameInfos.MAX_X);
            simulationInfos.SimNash.NextPosition.Item2 = rand.Next(GameInfos.MAX_Y);
            simulationInfos.SimNash.TargetingZombie = false;
            simulationInfos.SimStartingRandomMovesNum--;
        }
        else
        {
            for (int i = 0; i < simulationInfos.SimZombieCount; i++)
            {
                if (simulationInfos.SimZombies[i].ClosestHumanId != NULL && simulationInfos.SimZombies[i].ClosestHumanId != GameInfos.NASH_ID)
                {
                    zombiesThatDoNotTargetPlayer[len] = simulationInfos.SimZombies[i];
                    len++;
                }
            }

            simulationInfos.SimNash.Target = (len > 0) ?
                                                        zombiesThatDoNotTargetPlayer[rand.Next(len)] :
                                                        simulationInfos.SimZombies[rand(simulationInfos.SimZombieCount)];

            simulationInfos.SimNash.Arrived = false;
            simulationInfos.SimNash.TargetingZombie = true;
        }
    }

    public static void Turn(List<Tuple<int, int>> moveHistory, SimulationInfos simulationInfos)
    {
        int i, j, k;
        bool foundHuman;
        List<int> zombieTargetIdTmp;

        for (i = 0; i < simulationInfos.SimZombieCount; i++)
        {
            FindZombieTarget(simulationInfos.SimZombies[i]);
            MoveZombie(simulationInfos.SimZombies[i]);
        }

        moveHistory[simulationInfos.SimMovesCount] = GetPlayerDestination(simulationInfos);
        simulationInfos.SimMovesCount++;
        MovePlayer(simulationInfos.SimNash);

        // TODO: adapter code depuis solution https://github.com/cpiemontese/code-vs-zombies/blob/master/solution.c
        evaluate();

        zombiesEat();

        if ((simHumanCount) > 0 && (simZombieCount > 0))
        {
            if (simPlayer.arrived || simPlayerTargetDiedThisTurn)
            {
                computePlayerTarget();
                simPlayerTargetDiedThisTurn = false;
            }
        }
        else
        {
            simFailure = (simHumanCount <= 0);
            simZombiesAllDead = simZombieCount <= 0;
        }
    }

    public static void FindZombieTarget(Zombie zombie, SimulationInfos simulationInfos)
    {
        float minDist = float.PositiveInfinity;
        float tmpDist;
        bool targetFound = false;

        zombie.Arrived = false;

        tmpDist = GetDistance(zombie.position, simulationInfos.SimNash.Position);
        if (tmpDist < minDist)
        {
            zombie.ClosestHumanId = GameInfos.NASH_ID;
            targetFound = true;
            minDist = tmpDist;
        }

        for (int i = 0; i < simulationInfos.SimHumanCount; i++)
        {
            tmpDist = GetDistance(zombie.Position, simulationInfos.SimHumans[i].Position);
            if (tmpDist < minDist)
            {
                zombie.ClosestHumanId = simulationInfos.SimHumans[i];
                targetFound = true;
                minDist = tmpDist;
            }
        }
    }

    public static void MoveZombie(Zombie zombie)
    {
        zombie.Arrived = NextPosZombie(zombie, out zombie.Position);
    }

    public static void MovePlayer(Nash nash)
    {
        nash.Arrived = nextPosPlayer(*player, &player->gocomp.position);
    }

    public static bool NextPosZombie(Zombie zombie, out Tuple<int, int> posOut)
    {
        bool arrived = false;

        if (zombie.Target != NULL)
        {
            float dft = GetDistance(zombie.Position, zombie.Target.Position);
            float t;

            if (Math.Floor(dft) <= Zombie.MOUVEMENT)
            {
                arrived = true;
                posOut.Item1 = zombie.Target.Position.Item1;
                posOut.Item2 = zombie.Target.Position.Item2;
            }
            else
            {
                t = Zombie.MOUVEMENT / dft;
                posOut.Item1 = zombie.Position.Item1 + Math.Floor(t * (zombie.Target.Position.Item1 - zombie.Position.Item1));
                posOut.Item2 = zombie.Position.Item2 + Math.Floor(t * (zombie.Target.Position.Item2 - zombie.Position.Item2));
            }
        }
        else
        {
            posOut.Item1 = -1.0f;
            posOut.Item2 = -1.0f;
        }

        return arrived;
    }

    public static bool nextPosPlayer(Nash nash, out Tuple<int, int> posOut)
    {
        Tuple<int, int> dst;
        float dft;
        float t;
        bool arrived = false;

        if (nash.Target != NULL || nash.NextPosition != NULL)
        {
            dst = GetPlayerDestination(nash);
            dft = GetDistance(nash.Position, dst);

            if (Math.Floor(dft) <= Nash.MOUVEMENT)
            {
                arrived = true;
                posOut.Item1 = dst.Item1;
                posOut.Item2 = dst.Item2;
            }
            else
            {
                t = Nash.MOUVEMENT / dft;
                posOut.Item1 = nash.Position.Item1 + Math.Floor(t * (dst.Item1 - nash.Position.Item1));
                posOut.Item2 = nash.Position.Item2 + Math.Floor(t * (dst.y - nash.Position.Item2));
            }
        }
        else
        {
            posOut.Item1 = -1.0f;
            posOut.Item2 = -1.0f;
        }

        return arrived;
    }

    public static Tuple<int, int> GetPlayerDestination(Nash nash)
    {
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
    }

    public static int MaxHypotheticalScore(SimulationInfos simulationInfos)
    {
        int tmpPoints = 0;
        int totPoints = 0;
        int totHumans = simulationInfos.SimHumanCount;
        int humanPoints = 10 * totHumans * totHumans;

        for (int i = 0; i < simulationInfos.SimZombieCount; i++)
        {
            tmpPoints = humanPoints;
            if (simZombieCount > 1)
            {
                tmpPoints *= Fibonacci(i + 1);
            }
            totPoints += tmpPoints;
        }

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

    public static float TimeDifferenceInMillisecond(DateTime t0, DateTime t1)
    {
        return (t1.Second - t0.Second) * 1000.0f + (t1.Millisecond - t0.Millisecond) / 1000.0f;
    }
}

static class DebugInfos
{
    public static void WriteDebugMessage(string functionName, string[] strings)
    {
        StringBuilder sb = new StringBuilder(functionName + " : ");
        foreach (string str in strings)
            sb.Append(str);

        Console.Error.WriteLine(sb);
    }
}