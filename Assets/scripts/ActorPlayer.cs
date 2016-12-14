using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(Inventory))]
public class ActorPlayer : ActorMove {

	public static ActorPlayer Instance;

	Level.Node nodeSelected;
	Inventory inventory;
	bool waitingForInput = true;

	public static void OnHoverNode (Level.Node node) {
		if (Instance != null && !GameManager.IsInTurn && Instance.waitingForInput) {
		}
	}

	public static void OnUnhoverNode (Level.Node node) {
		if (Instance != null && !GameManager.IsInTurn && Instance.waitingForInput) {
		}
	}

	public static void OnClickNode (Level.Node node) {
		if (Instance != null && GameManager.PlayerCanMove && Instance.waitingForInput) {
			if (Instance.CouldMoveTo (node)) {
				Instance.nodeSelected = node;
				GameManager.StartTurn ();
			} else {
				print ("BAD MOVE");
			}
		}
	}

	protected override void OnMeetCrossing (Actor other) {
		if (character && character.IsEnemy(other.GetCharacter())) {
			// TODO handle this somehow
			// Enemy kills player
			if (other.IsFacingTowards (transform.position)) {
				character.Alive = false;
				print ("PLAYER DIED");
			}
		}
	}

	protected override void OnMeet (Actor other) {
		Character otherCharacter = other.GetCharacter ();
		// Meet another character
		if (otherCharacter) {
			if (character.IsEnemy (otherCharacter)) {
				// TODO handle this somehow
				// Enemy kills player
				if (other.IsFacingTowards (transform.position)) {
					character.Alive = false;
					print ("PLAYER DIED");
				}
				// Player kills enemy
				else {
					other.GetCharacter ().Alive = false;
					print ("Player killed enemy");
				}
			}
		} 
		// Meet an item pickup
		else if (other is Inventory.Item) {
			Inventory.Item item = (Inventory.Item) other;
			item.PickUp (inventory);
		}
	}

	protected override void OnTurnStart () {
		waitingForInput = false;
		TryMoveTo (nodeSelected);
	}

	protected override void OnTurnEnd () {
		waitingForInput = true;
	}

	void Awake() {
		Instance = this;
		inventory = GetComponent<Inventory> ();
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos ();
		if (node) {
			Actor actor;
			Vector3 pos = transform.position + transform.up * 0.4f;
			Dictionary<Vector3, List<Actor> > actorsInLOS = node.GetActorsInLOS();
			foreach (Vector3 direction in actorsInLOS.Keys) {
				Gizmos.color = Color.yellow;
				for (int i = 1; i < actorsInLOS [direction].Count; i++) {
					actor = actorsInLOS [direction] [i];
					if (actor) {
						Gizmos.DrawLine (pos, actor.transform.position + actor.transform.up * 0.4f);
					}
				}
				actor = actorsInLOS [direction] [0];
				if (actor) {
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine (pos, actor.transform.position + actor.transform.up * 0.4f);
				}
			}
		}
	}
}
