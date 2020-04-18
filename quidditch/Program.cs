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
		string[]    inputs;
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
	public Vector2 	Position {get; set;}
	public Vector2 	Velocity {get; set;}

	public abstract double	Mass {get;}
	public abstract double	Friction {get;}

	public Entity(int id, Vector2 position, Vector2 velocity)
	{
		Id = id;
		Position = position;
		Velocity = velocity;
	}

	public override string ToString()
	{
		return ($"Entity(id:{Id};Position:{Position};Velocity:{Velocity})");
	}
}

public class Wizard : Entity
{
	public bool	HasGrabbedSnaffle {get; set;}
	public override double	Mass{get {return 1.0;}}
	public override double	Friction {get {return 0.75;}}

	public Wizard(int id, Vector2 position, Vector2 velocity, bool hasGrabbedSnaffle) : base(id, position, velocity)
	{
		HasGrabbedSnaffle = hasGrabbedSnaffle;
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
		return ($"Wizard(id:{Id};Position:{Position};Velocity:{Velocity};HasGrabbedSnaffle:{HasGrabbedSnaffle})");
	}
}

public class Snaffle : Entity
{
	public bool	HasBeenGrabbed{get; set;}
	public override double	Mass{get {return 0.5;}}
	public override double	Friction {get {return 0.75;}}
	public Snaffle(int id, Vector2 position, Vector2 velocity, bool hasBeenGrabbed) : base(id, position, velocity)
	{
		HasBeenGrabbed = hasBeenGrabbed;
	}
	public override string ToString()
	{
		return ($"Snaffle(id:{Id};Position:{Position};Velocity:{Velocity};HasBeenGrabbed:{HasBeenGrabbed})");
	}

}

public class Bludger : Entity
{
	public int 		IdLastVictim {get; set;}

	public int				Thrust{get{ return 1000;}}
	public override double	Mass{get {return 8;}}
	public override double	Friction {get {return 0.9;}}

	public Bludger(int id, Vector2 position, Vector2 velocity, int idLastVictim) : base(id, position, velocity)
	{
		IdLastVictim = idLastVictim;
	}

	public override string ToString()
	{
		return ($"Bludger(id:{Id};Position:{Position};Velocity:{Velocity};IdLastVictim:{IdLastVictim})");
	}
}

public class Team
{
	public int 			Id {get;}
	public Vector2      GoalPosition {get;}
	public List<Wizard>	Wizards {get; set;}
	public int          Score {get; set;}
	public int          Magic {get; set;}

	public Team(int teamId, Vector2 goalPosition)
	{
		Id = teamId;
		GoalPosition = goalPosition;
		Wizards = new List<Wizard>();
		Score = -1;
		Magic = -1;
	}

}


public class Game
{
	public Team 			MyTeam {get; set;}
	public Team 			OpponentTeam {get; set;}
	public List<Snaffle>	Snaffles {get; set;}
	public List<Bludger>	Bludgers {get; set;}
	
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
		Snaffles = new List<Snaffle>();
		Bludgers = new List<Bludger>();
	}

	public void SyncGame()
	{
		string[] 	inputs;

		inputs = Console.ReadLine().Split(' ');
		MyTeam.Score = int.Parse(inputs[0]);
		MyTeam.Magic = int.Parse(inputs[1]);

		inputs = Console.ReadLine().Split(' ');
		OpponentTeam.Score = int.Parse(inputs[0]); 
		OpponentTeam.Magic = int.Parse(inputs[1]);

		int nb_entities = int.Parse(Console.ReadLine()); // number of entities still in game
		for (int i = 0; i < nb_entities; i++)
		{
			inputs = Console.ReadLine().Split(' ');

			int 		entityId = int.Parse(inputs[0]);
			string		entityType = inputs[1]; // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" or "BLUDGER"
			Vector2 	position = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3]));
			Vector2 	velocity = new Vector2(int.Parse(inputs[4]),int.Parse(inputs[5]));
			int 		state = int.Parse(inputs[6]);

			switch (entityType)
			{
				case "WIZARD":
					this.MyTeam.Wizards.Add(
						new Wizard(entityId, position, velocity, Convert.ToBoolean(state)));
					break;
				case "OPPONENT_WIZARD":
					this.OpponentTeam.Wizards.Add(
						new Wizard(entityId, position, velocity, Convert.ToBoolean(state)));
					break;
				case "SNAFFLE":
					this.Snaffles.Add(
						new Snaffle(entityId, position, velocity, Convert.ToBoolean(state)));
					break;
				case "BLUDGER":
					this.Bludgers.Add(
						new Bludger(entityId, position, velocity, state));
					break;
				default:
					break;
			}
		}
	}

	public void Update()
	{
		// Edit this line to indicate the action for each wizard (0 ≤ thrust ≤ 150, 0 ≤ power ≤ 500, 0 ≤ magic ≤ 1500)
		foreach (Wizard w in this.MyTeam.Wizards)
		{	
			this.Debug(w.ToString());
			this.Debug($"Id:{w.Id} Vector{w.Position}");
			w.Move(new Vector2(8000, 3750), 100);
		}
	}

	public void Debug(string message)
	{
		Console.Error.WriteLine(message);
	}
}

