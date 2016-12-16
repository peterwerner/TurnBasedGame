using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
[RequireComponent(typeof(Inventory))]
public class ActorPlayer : ActorMove {

	Dictionary<Level.Node, List<Inventory.Item> > stagedPickups = new Dictionary<Level.Node, List<Inventory.Item> > ();
	Level.Node nodeSelected;
	Inventory inventory;
	bool waitingForInput;

	public void StagePickup (Level.Node node, Inventory.Item item) {
		List<Inventory.Item> items;
		if (!stagedPickups.TryGetValue (node, out items)) {
			items = new List<Inventory.Item> ();
			stagedPickups.Add (node, items);
		}
		stagedPickups[node].Add (item);
	}

	protected override void OnTurnStart () {
		lookDir = this.Node.transform.position - this.transform.position;
		waitingForInput = false;
		nodeSelected = null;
	}

	protected override void OnTurnEnd () {
		stagedPickups.Clear ();
		waitingForInput = true;
	}

	protected override void OnReachNode (Level.Node node) {
		List<Inventory.Item> items;
		if (stagedPickups.TryGetValue (node, out items)) {
			foreach (Inventory.Item item in items) {
				item.PickUp (inventory);
			}
		}
	}

	void Awake() {
		inventory = GetComponent<Inventory> ();
	}

	protected override void Update () {
		base.Update ();
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			LayerMask layerMask = 1 << GameManager.GetNodeLayer ();
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
				Level.Node node = hit.collider.GetComponent<Level.Node> ();
				if (node) {
					OnClickNode (node);
				}
			}
		}
	}

	void OnClickNode (Level.Node node) {
		if (GameManager.PlayerCanMove && waitingForInput) {
			if (this.Node.HasNeighbor (node)) {
				nodeSelected = node;
				UpdateMovePath ();
				GameManager.StartTurn ();
			} else {
				print ("BAD MOVE");
			}
		}
	}

	protected override void UpdateMovePath() {
		MovePath.Clear ();
		MovePath.Add (this.Node);
		if (nodeSelected) {
			MovePath.Add (nodeSelected);
		}
	}

	protected override void OnDrawGizmos() {
		base.OnDrawGizmos ();
		if (this.Node) {
			Actor actor;
			Vector3 pos = transform.position + transform.up * 0.4f;
			Dictionary<Vector3, List<Actor> > actorsInLOS = this.Node.GetActorsInLOS();
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
