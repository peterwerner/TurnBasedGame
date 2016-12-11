﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPlayer : ActorMove {

	public static ActorPlayer Instance;

	Level.Node nodeSelected;
	bool waitingForInput = true;

	public static void OnHoverNode (Level.Node node) {
		if (Instance != null && !GameManager.IsInTurn() && Instance.waitingForInput) {
		}
	}

	public static void OnUnhoverNode (Level.Node node) {
		if (Instance != null && !GameManager.IsInTurn() && Instance.waitingForInput) {
		}
	}

	public static void OnClickNode (Level.Node node) {
		if (Instance != null && !GameManager.IsInTurn() && Instance.waitingForInput) {
			Level.Node.RelationshipTypes relationship = Instance.node.GetRelationship (node);
			if (relationship != Level.Node.RelationshipTypes.NONE) {
				Instance.nodeSelected = node;
				GameManager.StartTurn ();
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
		if (character && otherCharacter && character.IsEnemy(otherCharacter)) {
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

	protected override void OnTurnStart () {
		waitingForInput = false;
		TryMoveTo (nodeSelected);
	}

	protected override void OnTurnEnd () {
		waitingForInput = true;
	}

	void Awake() {
		Instance = this;
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos ();
		if (node) {
			Gizmos.color = Color.cyan;
			Vector3 pos = transform.position + transform.up * 0.4f;
			foreach (Actor actor in node.GetActorsInLOS()) {
				Gizmos.DrawLine (pos, actor.transform.position + actor.transform.up * 0.4f);
			}
		}
	}
}