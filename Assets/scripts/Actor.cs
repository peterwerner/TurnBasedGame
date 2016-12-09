using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : ListComponent<Actor> {

	static HashSet<Actor> actorsFinished = new HashSet<Actor>();

	protected Level.Node node;

	public static void StartTurns () {
		actorsFinished.Clear();
		foreach (Actor actor in InstanceList) {
			actor.OnTurnStart ();
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

	protected virtual void Start () {
		node = Level.Node.ClosestTo (this.transform.position);
	}

	protected virtual void OnTurnStart () {}

	protected virtual void OnTurnEnd () {}

	protected void EndTurn () {
		actorsFinished.Add(this);
	}

	protected virtual Level.Node.RelationshipTypes TryMoveTo (Level.Node destNode) {
		if (node == null || IsTurnEnded()) {
			return Level.Node.RelationshipTypes.NONE;
		}
		Level.Node.RelationshipTypes relationship = node.GetRelationship (destNode);
		if (relationship != Level.Node.RelationshipTypes.NONE) {
			node.RemoveActor (this);
			node = destNode;
			node.AddActor (this);
		}
		return relationship;
	}

	protected bool IsTurnEnded () {
		return !GameManager.IsInTurn() || actorsFinished.Contains (this);
	}
}
