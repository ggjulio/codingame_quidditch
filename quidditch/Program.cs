using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

/**
 * Grab Snaffles and try to throw them through the opponent's goal!
 * Move towards a Snaffle to grab it and use your team id to determine towards where you need to throw it.
 * Use the Wingardium spell to move things around at your leisure, the more magic you put it, the further they'll move.
 **/

class Player
{
	static void Main(string[] args)
	{
		Game        game;

		// Readline return my team id, and instanciate game
		game = new Game(int.Parse(Console.ReadLine()));

		// game loop
		while (true)
		{
			game.SyncGame();
			game.Update();
		}
	}
}

public abstract class Entity
{        
	public int 		Id {get;}
	public string	Type {get;}
	public List<Vector2> Positions{get; set;}		
	public List<Vector2> Velocities {get; set;}
	public abstract double	Mass {get;}
	public abstract double	Friction {get;}
	public abstract int		Radius {get;}

	public Entity(int id, string type, Vector2 position, Vector2 velocity)
	{
		Id = id;
		Type = type;
		Positions = new List<Vector2>();
		Positions.Add(position);
		Velocities = new List<Vector2>();
		Velocities.Add(velocity);
	}

	public void Update(Vector2 position, Vector2 velocity)
	{
		Positions.Add(position);
		Velocities.Add(velocity);
	}

	public override string ToString()
	{
		return ($"Entity(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()})");
	}
}

public class Wizard : Entity
{
	public bool	HasGrabbedSnaffle {get; set;}
	public override double	Mass{get {return 1.0;}}
	public override double	Friction {get {return 0.75;}}
	public override int		Radius {get {return 400;}}

	public Wizard(int id, string type, Vector2 position, Vector2 velocity, bool hasGrabbedSnaffle) : base(id, type, position, velocity)
	{
		HasGrabbedSnaffle = hasGrabbedSnaffle;
	}

	public void Update(Vector2 position, Vector2 velocity, bool hasGrabbedSnaffle)
	{
		base.Update(position, velocity);
		this.HasGrabbedSnaffle = hasGrabbedSnaffle;
	}	
	public void Move(Vector2 targetPosition, int thrust)
	{
		Console.WriteLine($"MOVE {targetPosition.X} {targetPosition.Y} {thrust}"); 
	}
	public void Throw_snaffle(Vector2 targetPosition, int power)
	{
		Console.WriteLine($"THROW {targetPosition.X} {targetPosition.Y} {power}"); 
	}
	public void Wingardium(Entity entity, Vector2 targetPosition, int magic)
	{
		Console.WriteLine($"THROW {entity.Id} {targetPosition.X} {targetPosition.Y} {magic}"); 
	}

	public override string ToString()
	{
		return ($"Wizard(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()};HasGrabbedSnaffle:{HasGrabbedSnaffle})");
	}
}

public class Snaffle : Entity
{
	public bool	HasBeenGrabbed{get; set;}
	public override double	Mass{get {return 0.5;}}
	public override double	Friction {get {return 0.75;}}
	public override int		Radius {get {return 150;}}
	public void Update(Vector2 position, Vector2 velocity, bool hasBeenGrabbed)
	{
		base.Update(position, velocity);
		this.HasBeenGrabbed = hasBeenGrabbed;
	}	
	public Snaffle(int id, string type, Vector2 position, Vector2 velocity, bool hasBeenGrabbed) : base(id, type, position, velocity)
	{
		HasBeenGrabbed = hasBeenGrabbed;
	}
	public override string ToString()
	{
		return ($"Snaffle(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()};HasBeenGrabbed:{HasBeenGrabbed})");
	}
}

public class Bludger : Entity
{
	public int 		IdLastVictim {get; set;}
	public int				Thrust{get{ return 1000;}}
	public override double	Mass{get {return 8;}}
	public override double	Friction {get {return 0.9;}}
	public override int		Radius {get {return 200;}}

	public Bludger(int id, string type, Vector2 position, Vector2 velocity, int idLastVictim) : base(id, type, position, velocity)
	{
		IdLastVictim = idLastVictim;
	}
	public void Update(Vector2 position, Vector2 velocity, int idLastVictim)
	{
		base.Update(position, velocity);
		this.IdLastVictim = idLastVictim;
	}	
	public override string ToString()
	{
		return ($"Bludger(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()};IdLastVictim:{IdLastVictim})");
	}
}

public class Team
{
	public int 			Id {get;}
	public Vector2      GoalCenterPosition {get;}
	public int          Score {get; set;}
	public int          Magic {get; set;}
	public int          MagicMax {get {return 100;}}

	public Team(int teamId, Vector2 goalCenterPosition)
	{
		Id = teamId;
		GoalCenterPosition = goalCenterPosition;
		Score = -1;
		Magic = -1;
	}
	public void Update(int score, int magic)
	{
		Score = score;
		Magic = magic;
	}	
}

public class Game
{
	public Team 			MyTeam {get; set;}
	public Team 			OpponentTeam {get; set;}
	public List<Entity>		Entities {get; set;}

	public static Vector2	MapSize {get {return new Vector2(16001, 7501);}}
	
	// TeamId : 0 if MyGoal is on the left, 1 if MyGoal is on the right
	public Game(int myTeamId)
	{
		MyTeam = new Team(
			myTeamId,
			(myTeamId == 0 ? new Vector2(0, 3750) : new Vector2(16000, 3750))
		);
		OpponentTeam = new Team(
			myTeamId == 0 ? 1 : 0,
			(myTeamId == 0 ? new Vector2(16000, 3750) : new Vector2(0, 3750))
		);
		Entities = new List<Entity>();
	}

	public void SyncGame()
	{
		string[] 	inputs;

		inputs = Console.ReadLine().Split(' ');
		MyTeam.Update(int.Parse(inputs[0]), int.Parse(inputs[1]));

		inputs = Console.ReadLine().Split(' ');
		OpponentTeam.Update(int.Parse(inputs[0]), int.Parse(inputs[1]));

		int nb_entities = int.Parse(Console.ReadLine()); // number of entities still in game
		for (int i = 0; i < nb_entities; i++)
		{
			inputs = Console.ReadLine().Split(' ');

			int 		entityId = int.Parse(inputs[0]);
			string		entityType = inputs[1]; // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" or "BLUDGER"
			Vector2 	position = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3]));
			Vector2 	velocity = new Vector2(int.Parse(inputs[4]),int.Parse(inputs[5]));
			int 		state = int.Parse(inputs[6]);

			Entity actualElement = this.Entities.Find(e => e.Id == entityId);
			this.Debug($"{entityId}");
			switch (entityType)
			{
				case "WIZARD":
					// if (actualElement is Entity e)
					// 	e.Id = 9;
					// else
					this.Debug($"({position.X},{position.Y})");
					this.Entities.Add(
						new Wizard(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "OPPONENT_WIZARD":
					this.Entities.Add(
						new Wizard(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "SNAFFLE":
					this.Entities.Add(
						new Snaffle(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "BLUDGER":
					this.Entities.Add(
						new Bludger(entityId, entityType, position, velocity, state));
					break;
				default:
					break;
			}
		}
	}

	public void Update()
	{
		// Edit this line to indicate the action for each wizard (0 ≤ thrust ≤ 150, 0 ≤ power ≤ 500, 0 ≤ magic ≤ 1500)
		foreach (Wizard w in this.Entities.Where(e => e.Type == "WIZZARD"))
		{	
			Strategy.Attack(this, w);

		}
	}

	public Static void Debug(string message)
	{
		Console.Error.WriteLine(message);
	}
}

public class Strategy
{
	public static void Attack(Game game, Wizard wizard)
	{
		List<Entity> closestEntities = game.Entities.OrderBy(
			x => Vector2.Distance(wizard.Positions.Last(), x.Positions.Last())).ToList();

		//if (closestEntities.Any())
			wizard.Move(closestEntities.First().Positions.Last(), 100);
		//else
		//	return;
	}
}

