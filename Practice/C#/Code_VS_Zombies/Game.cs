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
    private Nash _nash;
    private int _numberOfHumans;
    private int _numberOfZombies;
    private List<Human> _humans = new List<Human>();
    private List<Zombie> _zombies = new List<Zombie>();

    public GameInfos()
    {
        _nash = new Nash();
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
    private Tuple<int, int> _position;
    private Tuple<int, int> _nextPosition;
    private int _zombieIdMovingTo = -1;

    public const int MOUVEMENT = 1000;

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

    public void SetClosestHuman(int humanId, int nbTurnToReach)
    {
        _closestHumanId = humanId;
        _nbTurnToReachHuman = nbTurnToReach;
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

        if (pos1.Item1 > pos2.Item2)
            distX = pos1.Item1 - pos2.Item1;
        else
            distX = pos2.Item1 - pos1.Item1;

        if (pos1.Item2 > pos2.Item2)
            distY = pos1.Item2 - pos2.Item2;
        else
            distY = pos2.Item2 - pos1.Item2;

        distance = Math.Sqrt(Math.Abs(Math.Pow(distX, 2) + Math.Pow(distY, 2)));
        try
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
        }
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