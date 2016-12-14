using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : ListComponent<Actor> {

	static HashSet<Actor> actorsFinished = new HashSet<Actor>();

	protected Level.Node node, prevNode;
	protected Character character;
	protected Vector3 lookDir;

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

	public static void HandleInteractions () {
		int i, j;
		Actor a, b;
		for (i = 0; i < InstanceList.Count - 1; i++) {
			a = InstanceList [i];
			for (j = i + 1; j < InstanceList.Count; j++) {
				b = InstanceList [j];
				if (a.node == b.node && a.prevNode != b.prevNode) {
					a.OnMeet (b);
					b.OnMeet (a);
				}
				if (a.node != b.node && a.prevNode == b.prevNode) {
					a.OnSeparate (b);
					b.OnSeparate (a);
				}
				if (a.node == b.prevNode && a.prevNode == b.node) {
					a.OnMeetCrossing (b);
					b.OnMeetCrossing (a);
				}
			}
		}
	}
		
	protected virtual void Start () {
		character = GetComponent<Character> ();
		node = Level.Node.ClosestTo (this.transform.position);
		node.AddActor (this, true);
		lookDir = transform.forward;
	}

	protected void EndTurn () {
		actorsFinished.Add(this);
	}

	protected virtual bool TryMoveTo (Level.Node destNode) {
		if (!IsTurnEnded() && CouldMoveTo(destNode)) {
			node.RemoveActor (this);
			prevNode = node;
			node = destNode;
			node.AddActor (this);
			lookDir = (node.transform.position - prevNode.transform.position).normalized;
			return true;
		}
		return false;
	}

	protected virtual bool CouldMoveTo (Level.Node destNode) {
		if (node != null && node.HasNeighbor(destNode)) {
			return true;
		}
		return false;
	}

	protected bool IsTurnEnded () {
		return !GameManager.IsInTurn || actorsFinished.Contains (this);
	}

	public Character GetCharacter () {
		return character;
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

	public void Meet (Actor other) {
		this.OnMeet (other);
		other.OnMeet (this);
	}

	public Level.Node Node {
		get { return node; }
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

	protected virtual void OnMeet (Actor other) {}

	protected virtual void OnSeparate (Actor other) {}

	protected virtual void OnMeetCrossing (Actor other) {}
}
