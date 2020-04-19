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
			game.Play();
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

	public float Distance(Entity e)
	{
		return(Vector2.Distance(Positions.Last(), e.Positions.Last()));
	}
	public float Distance(Vector2 v)
	{
		return(Vector2.Distance(Positions.Last(), v));
	}
	public double AngleBetween(Entity e)
	{
		Vector2 v1 = Positions.Last();
		Vector2 v2 = e.Positions.Last();

		return (Math.Atan2(v2.Y - v1.Y, v2.X - v1.X) * 180 / Math.PI);
	}

	public override string ToString()
	{
		return ($"Entity(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()})");
	}
}

public enum eAction
{
	None,
	Attack,
	Defend,
	Magic
}

public class Wizard : Entity
{
	public bool	CanShootSnaffle {get; set;}
	public bool HasGrabbedSnaffle {get; set;}
	public Snaffle			grabbedSnaffle {get; set;}
	public Snaffle			TargetSnaffle {get; set;}
	public override double	Mass{get {return 1.0;}}
	public override double	Friction {get {return 0.75;}}
	public override int		Radius {get {return 400;}}
	public eAction			Action{get; set;}

	public Wizard(int id, string type, Vector2 position, Vector2 velocity, bool canShootSnaffle) : base(id, type, position, velocity)
	{
		CanShootSnaffle = canShootSnaffle;
		HasGrabbedSnaffle = false;
		Action = eAction.None;
	}

	public void Update(Vector2 position, Vector2 velocity, bool canShootSnaffle)
	{
		base.Update(position, velocity);
		CanShootSnaffle = canShootSnaffle;
		HasGrabbedSnaffle = false;
		grabbedSnaffle = null;
		TargetSnaffle = null;
	}	
	public void Move(Vector2 targetPosition, int thrust)
	{
		Console.WriteLine($"MOVE {targetPosition.X} {targetPosition.Y} {thrust}"); 
	}
	public void Shoot(Vector2 targetPosition, int power)
	{
		Console.WriteLine($"THROW {targetPosition.X} {targetPosition.Y} {power}"); 
	}
	public void Wingardium(Entity entity, Vector2 targetPosition, int magic)
	{
		Console.WriteLine($"WINGARDIUM {entity.Id} {targetPosition.X} {targetPosition.Y} {magic}"); 
	}

	public override string ToString()
	{
		return ($"Wizard(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()};CanShootSnaffle:{CanShootSnaffle})");
	}
}

public class Snaffle : Entity
{
	public bool StillExist {get; set;}
	public bool	CanBeShooted {get; set;}
	public bool HasBeenGrabbed {get; set;}  // the distance between snaffle and wizard is less than 400 unit.
	public Wizard Grabber {get; set;}
	public override double	Mass{get {return 0.5;}}
	public override double	Friction {get {return 0.75;}}
	public override int		Radius {get {return 150;}}
	
	public Snaffle(int id, string type, Vector2 position, Vector2 velocity, bool canBeShooted) : base(id, type, position, velocity)
	{
		CanBeShooted = canBeShooted;
		StillExist = true;
		HasBeenGrabbed = false;
	}
	public void Update(Vector2 position, Vector2 velocity, bool canBeShooted)
	{
		base.Update(position, velocity);
		CanBeShooted = canBeShooted;
		StillExist = true;
		Grabber = null;
		HasBeenGrabbed = false;
	}
	public override string ToString()
	{
		return ($"Snaffle(id:{Id};Position:{Positions.Last()};Velocity:{Velocities.Last()};CanBeShooted:{CanBeShooted})");
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

		SetSnaffleStillExistToFalse();

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
			switch (entityType)
			{
				case "WIZARD":
					if (actualElement is Wizard e)
						e.Update(position, velocity, Convert.ToBoolean(state));
					else
						this.Entities.Add(
							new Wizard(entityId, entityType, position, velocity, 
										Convert.ToBoolean(state)));
					break;
				case "OPPONENT_WIZARD":
					if (actualElement is Wizard oe)
						oe.Update(position, velocity, Convert.ToBoolean(state));
					else
					this.Entities.Add(
						new Wizard(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "SNAFFLE":
					if (actualElement is Snaffle s)
						s.Update(position, velocity, Convert.ToBoolean(state));
					else
					this.Entities.Add(
						new Snaffle(entityId, entityType, position, velocity, Convert.ToBoolean(state)));
					break;
				case "BLUDGER":
					if (actualElement is Bludger b)
						b.Update(position, velocity, state);
					else
					this.Entities.Add(
						new Bludger(entityId, entityType, position, velocity, state));
					break;
				default:
					break;
			}
		}
		ComputeAdditionalInfos();
	}
	private void ComputeAdditionalInfos()
	{	
		foreach (Wizard w in GetAllWizards())
			foreach (Snaffle s in GetSnaffles().Where(e => !e.HasBeenGrabbed).ToList())
			{
				if (Vector2.Distance(w.Positions.Last(),s.Positions.Last()) < 400)
				{
					Game.Debug("Id" + s.Id.ToString());
					Game.Debug(">" + w.Id.ToString());

					w.HasGrabbedSnaffle = true;
					w.grabbedSnaffle = s;
					s.Grabber = w;
					s.HasBeenGrabbed = true;
					break;
				}
				else
				{

				}
			}
	}
	private void SetSnaffleStillExistToFalse()
	{
		foreach (Snaffle s in GetSnaffles())
			s.StillExist = false;
	}
	public List<Snaffle> GetSnaffles()
	{
		return (this.Entities.Where(e => e is Snaffle s && s.StillExist).Cast<Snaffle>().ToList());
	}
	public List<Wizard> GetAllWizards()
	{
		return (GetMyWizards().Concat(GetOpponentWizards()).ToList());
	}
	public List<Wizard> GetMyWizards()
	{
		return (this.Entities.Where(e => e is Wizard w && w.Type == "WIZARD").Cast<Wizard>().ToList());
	}

	public List<Wizard> GetOpponentWizards()
	{
		return (this.Entities.Where(e => e is Wizard w && w.Type == "OPPONENT_WIZARD").Cast<Wizard>().ToList());
	}

	public List<Bludger> GetBludgers()
	{
		return (this.Entities.Where(e => e is Bludger).Cast<Bludger>().ToList());
	}

	public void Play()
	{
		// Edit this line to indicate the action for each wizard (0 ≤ thrust ≤ 150, 0 ≤ power ≤ 500, 0 ≤ magic ≤ 1500)
		// foreach (Wizard w in GetMyWizards())
		// {	
		// 	// can set logic here
		// 	w.Action = eAction.Attack;
		// }
		GetMyWizards()[1].Action = eAction.Defend;
		GetMyWizards()[0].Action = eAction.Attack; 
 
		if (MyTeam.Magic == MyTeam.MagicMax - 1 ||
			!GetSnaffles().Any(
				e => e.Distance(MyTeam.GoalCenterPosition) < 2500 
				|| e.Distance(OpponentTeam.GoalCenterPosition) < 2500))
			if (MyTeam.Magic > 15)
				GetMyWizards()[0].Action = eAction.Magic;
		foreach (Wizard w in GetMyWizards())
		{
			switch (w.Action)
			{
				case eAction.Attack:
					Strategy.Attack(this, w);
					break;
				case eAction.Defend:
					Strategy.Defend(this, w);
					break;
				case eAction.Magic:
					Strategy.Magic(this, w);
					break;
				default:
					w.Move(new Vector2(0), 0);
					break;
			}
		}
	}
	public static void Debug(string message)
	{
		Console.Error.WriteLine(message);
	}
}

public class Strategy
{
	static List<Snaffle> GetSnafflesOrderClosestToDistance(Game game, Vector2 position)
	{
		return (
			game.GetSnaffles().OrderBy(
				x => x.Distance(position)).ToList()
			);
	}
	static List<Snaffle> 	GetSnafflesClosestToDistanceNotGrabed(Game game, Vector2 position)
	{
		return (
			GetSnafflesOrderClosestToDistance(game, position).Where(
			e => !e.HasBeenGrabbed).ToList()
		);
	}
	static List<Snaffle> 	GetSnafflesInAngleRange(List<Snaffle> pSnaffles, Wizard wizard, double min, double max)
	{
		return (
			pSnaffles.Where(e => AngleInRange(wizard.AngleBetween(e), min, max)).ToList()
		);
	}
	public static bool AngleInRange(double angle, double min, double max)
	{
		if (angle >= min && angle <= max)
			return (true);
		return (false);
	}

	public static void Attack(Game game, Wizard wizard)
	{
		Snaffle s = null;
		List<Snaffle> sList =  GetSnafflesClosestToDistanceNotGrabed(game, wizard.Positions.Last());

		if (sList.Any())
			if (sList.First().Distance(wizard.Positions.Last()) > 1000)
				sList = GetSnafflesInAngleRange(sList, wizard, -45, 45);
		
		if (!sList.Any())
			sList =  GetSnafflesClosestToDistanceNotGrabed(game, wizard.Positions.Last());
		if (sList.Any())
			s = sList.First();
		// If we can shoot 
		if (wizard.grabbedSnaffle != null && wizard.grabbedSnaffle.CanBeShooted)
		{
			if (wizard.Distance(game.OpponentTeam.GoalCenterPosition) >= 6000 
				&& !game.GetOpponentWizards().Any(e => e.Distance(s) < 900))
				wizard.Shoot(s.Positions.Last(), 500);
			else
				wizard.Shoot(game.OpponentTeam.GoalCenterPosition, 500);
		}
		else if (s != null)
			wizard.Move(s.Positions.Last(), 100);
		else
			wizard.Move(new Vector2(0), 0);

	}
	public static void Defend(Game game, Wizard wizard)
	{
		List<Snaffle> sList = game.GetSnaffles().OrderBy(e => e.Distance(game.MyTeam.GoalCenterPosition)).ToList();

		Wizard otherW = game.GetMyWizards().Find(e => e.Id != wizard.Id);

		if (wizard.grabbedSnaffle != null && wizard.grabbedSnaffle.CanBeShooted)
			if (wizard.Distance(game.OpponentTeam.GoalCenterPosition) > otherW.Distance(game.OpponentTeam.GoalCenterPosition) && wizard.Distance(otherW) > 1000)
				wizard.Shoot(otherW.Positions.Last(), 500);
			else
				wizard.Shoot(game.OpponentTeam.GoalCenterPosition, 500);
		else
			wizard.Move(sList.First().Positions.Last(), 100);
	}
	public static void Magic(Game game, Wizard wizard)
	{
		List<Snaffle> sList = game.GetSnaffles()
		.OrderBy(e => e.Distance(game.OpponentTeam.GoalCenterPosition))
		.ThenBy(e => e.Distance(game.MyTeam.GoalCenterPosition)).ToList();

		wizard.Wingardium(sList.Find(e =>  e.Grabber == null || !game.GetMyWizards().Any(x  => x.Id == e.Grabber.Id)),
		 game.OpponentTeam.GoalCenterPosition, game.MyTeam.Magic);
	}
}

