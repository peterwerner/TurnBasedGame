﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Actor : ListComponent<Actor> {

	static HashSet<Actor> actorsFinished = new HashSet<Actor>();

	protected Character character;
	protected Vector3 lookDir;

	protected Dictionary<Level.Node, List<Actor> > stagedDeaths = new Dictionary<Level.Node, List<Actor> > ();
	protected Dictionary<Level.Node, List<Actor> > stagedKills = new Dictionary<Level.Node, List<Actor> > ();

	Level.Node closestNode, prevClosestNode;
	[SerializeField] [Tooltip("Lower order actors move first")] int moveOrder = 0;
	List<Level.Node> movePath;


	/* Turn lifecycle */

	public static void StartTurns () {
		actorsFinished.Clear();

		ComputeInteractions ();

		foreach (Actor actor in InstanceList) {
			actor.OnTurnStart ();
		}
	}

	public static void EndTurns () {
		foreach (Actor actor in InstanceList) {
			actor.stagedKills.Clear ();
			actor.stagedDeaths.Clear ();
			actor.OnTurnEnd ();
		}
	}


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


	/* Interactions between actors over the course of one turn */

	static void ComputeInteractions () {
		foreach (Actor a in InstanceList) {
			// TODO: currently only handling player <-> NPC interactions (not NPC <-> NPC)
			if (!(a is ActorPlayer)) {
				continue;
			}
			foreach (Actor b in InstanceList) {
				if (a == b) {
					continue;
				}
				// Player gets item
				else if (b is Inventory.Item) {
					if (a.MovePathHeadless.Contains (b.MovePath [0])) {
						AddPickupInteraction (b.MovePath [0], (ActorPlayer)a, (Inventory.Item)b);
					}
				}
				// Player meets enemy
				else if (a.character.IsEnemy (b.character)) {
					// a moves before b
					// 		meet if a's headless path intersects b's start point
					//		meet if b's headless path intersects a's end point
					if (a.moveOrder < b.moveOrder) {
						if (a.MovePathHeadless.Contains (b.MovePath.First ())) {
							int index = a.MovePath.IndexOf (b.MovePath.First ());
							Vector3 aDir = a.MovePath [index].transform.position - a.MovePath [index - 1].transform.position;
							Vector3 bDir = b.MovePath.Count >= 2 ? b.MovePath [1].transform.position - b.MovePath [0].transform.position : b.lookDir;
							// If b is facing a (assume a is player), b kills a
							if (VectorUtil.ClosestCardinalDirection (-1 * aDir) == VectorUtil.ClosestCardinalDirection (bDir)) {
								AddKillInteraction (b.MovePath.First(), b, a);
							} else {
								AddKillInteraction (b.MovePath.First(), a, b);
							}
						} else if (b.MovePathHeadless.Contains (a.MovePath.Last ())) {
							AddKillInteraction (a.MovePath.Last(), b, a);
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
		CleanupKillInteractions ();
	}

	static void AddKillInteraction (Level.Node node, Actor killer, Actor victim) {
		if (killer.character && victim.character && killer.character.IsEnemy (victim.character)) {
			// Update killer
			List<Actor> kills;
			if (!killer.stagedKills.TryGetValue (node, out kills)) {
				kills = new List<Actor> ();
				killer.stagedKills [node] = kills;
			}
			killer.stagedKills [node].Add (victim);
			// Update victim
			List<Actor> deaths;
			if (!victim.stagedDeaths.TryGetValue (node, out deaths)) {
				deaths = new List<Actor> ();
				victim.stagedDeaths [node] = deaths;
			}
			victim.stagedDeaths [node].Add (killer);
		}
	}

	static void AddPickupInteraction (Level.Node node, ActorPlayer player, Inventory.Item item) {
		player.StagePickup (node, item);
	}

	// Ensure characters can only die once per turn (taking the first death event in their path)
	static void CleanupKillInteractions () {
		foreach (Actor actor in InstanceList) {
			Actor killer = null;
			foreach (Level.Node node in actor.MovePath) {
				List<Actor> deaths;
				if (actor.stagedDeaths.TryGetValue (node, out deaths)) {
					if (killer) {
						foreach (Actor otherKiller in deaths) {
							otherKiller.stagedKills[node].Remove (actor);
						}
						actor.stagedDeaths.Remove (node);
					} else {
						if (deaths.Count > 0) {
							for (int i = 1; i < deaths.Count; i++) {
								deaths[i].stagedKills[node].Remove (actor);
							}
							killer = deaths [0];
							deaths.Clear ();
							deaths.Add (killer);
						}
					}
				}
			}
		}
	}


	/* Turn lifecycle continued */

	public static bool AllActorsFinished () {
		foreach (Actor actor in InstanceList) {
			if (!actorsFinished.Contains (actor)) {
				return false;
			}
		}
		return true;
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
		

	/* Object lifecycle */

	public static void InitAll () {
		foreach (Actor actor in InstanceList) {
			actor.Init ();
		}
	}

	void Init () {
		character = GetComponent<Character> ();
		closestNode = Level.Node.ClosestTo (this.transform.position);
		closestNode.AddActor (this);
		movePath = new List<Level.Node>();
		movePath.Add (Node);
		lookDir = transform.forward;
		EndTurn ();
	}

	protected virtual void Update () {
		// Update closest node
		if (MovePath.Count > 1) {
			float sqrDistBest = Vector3.SqrMagnitude (closestNode.transform.position - transform.position);
			foreach (Level.Node prospectiveNode in MovePath) {
				float sqrDist = Vector3.SqrMagnitude (prospectiveNode.transform.position - transform.position);
				if (sqrDist < sqrDistBest) {
					sqrDistBest = sqrDist;
					closestNode.RemoveActor (this);
					closestNode = prospectiveNode;
					closestNode.AddActor (this);
				}
			}
			if (closestNode != prevClosestNode) {
				OnReachNode (closestNode);
				prevClosestNode = closestNode;
			}
		}
		// Handle kills and deaths
		List<Actor> kills;
		if (stagedKills.TryGetValue (this.Node, out kills)) {
			for (int i = kills.Count - 1; i >= 0; i--) {
				if (kills[i].Node == this.Node) {
					OnKill (kills[i]);
					kills [i].stagedDeaths [this.Node].Remove (this);
					kills.RemoveAt (i);
				}
			}
		}
	}

	protected virtual void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Vector3 pos = transform.position + transform.up * 0.1f;
		Gizmos.DrawLine (pos, pos + lookDir * 0.5f);
	}

	void OnDestroy () {
		if (closestNode) {
			closestNode.RemoveActor (this);
		}
	}
		

	/* Helpers */

	public Character GetCharacter () {
		return character;
	}

	public Level.Node Node {
		get { return closestNode; }
	}


	/* Events for subclasses to override */

	protected virtual void OnTurnStart () { EndTurn (); }

	protected virtual void OnTurnEnd () {}

	protected virtual void OnReachNode (Level.Node node) {}

	protected virtual void OnKill (Actor actor) { 
		if (actor.character) {
			actor.character.Alive = false;
		}
	}

}
