using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPatrollerRook : ActorPatroller {

	protected override void OnTurnStart () {
		waypointIndex = path != null ? GetNextWaypointIndex () : 0;
		path = Level.NodeAStar.ShortestPath (node, waypoints [waypointIndex]);
		foreach (Level.Node nextNode in path) {
			TryMoveTo (nextNode);
		}
	}
}
