using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Actor : ListComponent<Actor> {

	static HashSet<Actor> actorsFinished = new HashSet<Actor>();

	protected Character character;
	protected Vector3 lookDir;

	Level.Node node;
	[SerializeField] [Tooltip("Lower order actors move first")] int moveOrder = 0;
	List<Level.Node> movePath;


	// current node -> intermediate nodes -> destination node (next current node)
	protected List<Level.Node> MovePath {
		get { return movePath; }
		set { movePath = value; }
	}
	// intermediate nodes -> destination node (next current node)
	protected List<Level.Node> MovePathHeadless {
		get {
			List<Level.Node> headless = new List<Level.Node> (MovePath);
			headless.RemoveAt (0);
			return headless;
		}
	}

	// current node -> intermediate nodes -> destination node (next current node)
	protected virtual void UpdateMovePath() { MovePath = MovePath; }
		
	public static void StartTurns () {
		actorsFinished.Clear();

		ComputeInteractions ();

		foreach (Actor actor in InstanceList) {
			actor.OnTurnStart ();
		}
	}

	static void ComputeInteractions () {
		for (int i = 0; i < InstanceList.Count; i++) {
			Actor a = InstanceList [i];
			// TODO: currently only handling player <-> NPC interactions (not NPC <-> NPC)
			if (!(a is ActorPlayer)) {
				continue;
			}
			for (int j = 0; j < InstanceList.Count; j++) {
				Actor b = InstanceList [j];
				if (a == b || !a.character.IsEnemy(b.character)) {
					continue;
				}
				// a moves before b
				// 		meet if a's headless path intersects b's start point
				//		meet if b's headless path intersects a's end point
				if (a.moveOrder < b.moveOrder) {
					if (a.MovePathHeadless.Contains (b.MovePath.First()))
					{
						int index = a.MovePath.IndexOf (b.MovePath.First ());
						Vector3 aDir = a.MovePath [index].transform.position - a.MovePath [index - 1].transform.position;
						Vector3 bDir = b.MovePath.Count >= 2 ? b.MovePath [1].transform.position - b.MovePath [0].transform.position : b.lookDir;
						// If b is facing a (assume a is player), b kills a
						if (VectorUtil.ClosestCardinalDirection (-1 * aDir) == VectorUtil.ClosestCardinalDirection (bDir)) {
							AddKillInteraction (b, a);
						} else {
							AddKillInteraction (a, b);
						}
					}
					else if (b.MovePathHeadless.Contains (a.MovePath.Last()))
					{
						AddKillInteraction (b, a);
					}
				}
				// a moves after b
				// 		meet if a's headless path intersects b's end point
				//		meet if b's headless path intersects a's start point
				else if (a.moveOrder > b.moveOrder) {
					// TODO: only need to handle this if player moves after any NPC
					throw new UnityException ("not implemented (" + a.moveOrder + " > " + b.moveOrder + ")");
				}
				// a and b move at the same time
				//		meet if their headless paths intersect
				else {
					// TODO: only need to handle this if player moves after any NPC
					throw new UnityException ("not implemented (" + a.moveOrder + " == " + b.moveOrder + ")");
				}
			}
		}
	}

	static void AddKillInteraction (Actor killer, Actor victim) {
		// TODO: actually do stuff
		if (killer.character && victim.character && killer.character.IsEnemy (victim.character)) {
			victim.character.Alive = false;
		}
	}

	public static void EndTurns () {
		foreach (Actor actor in InstanceList) {
			actor.OnTurnEnd ();
		}
	}

	public static bool AllActorsFinished () {
		return actorsFinished.Count >= InstanceList.Count;
	}

	protected void EndTurn () {
		actorsFinished.Add(this);
		UpdateMovePath ();
		if (MovePath.Count <= 0 || MovePath [0] != Node) {
			throw new UnityException ("Actor MovePath must start with the current node");
		}
	}

	protected bool IsTurnEnded () {
		return !GameManager.IsInTurn || actorsFinished.Contains (this);
	}
		
	public bool IsFacingTowards (Vector3 pos, bool horizontalOnly = true) {
		Vector3 dir = pos - transform.position;
		Vector3 look = lookDir;
		if (horizontalOnly) {
			dir.y = 0;
			look.y = 0;
		}
		return Vector3.Angle (dir, look) <= 45;
	}

	public Character GetCharacter () {
		return character;
	}

	public Level.Node Node {
		get { return node; }
		protected set { node = value; }
	}



	public static void InitAll () {
		foreach (Actor actor in InstanceList) {
			actor.Init ();
		}
	}

	void Init () {
		character = GetComponent<Character> ();
		node = Level.Node.ClosestTo (this.transform.position);
		movePath = new List<Level.Node>();
		movePath.Add (node);
		lookDir = transform.forward;
		EndTurn ();
	}

	protected virtual void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Vector3 pos = transform.position + transform.up * 0.1f;
		Gizmos.DrawLine (pos, pos + lookDir * 0.5f);
	}

	void OnDestroy () {
		if (node) {
			node.RemoveActor (this);
		}
	}
		
	// Events for subclasses to override

	protected virtual void OnTurnStart () { EndTurn (); }

	protected virtual void OnTurnEnd () {}

}
