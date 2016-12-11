using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPatroller : ActorMove {

	private enum LoopTypes { LOOP, REVERSE };

	[SerializeField] Level.Node[] waypoints;
	[SerializeField] LoopTypes loopType;
	List<Level.Node> path;
	int waypointIndex = 0, step = 1;

	protected override void OnTurnStart () {
		if (path == null) {
			path = Level.NodeAStar.ShortestPath (node, waypoints [waypointIndex]);
		} 
		if (node == waypoints [waypointIndex]) {
			waypointIndex = GetNextWaypointIndex ();
			path = Level.NodeAStar.ShortestPath (node, waypoints [waypointIndex]);
		}
		TryMoveTo (path[0]);
		path.RemoveAt (0);
	}

	int GetNextWaypointIndex () {
		int nextIndex = waypointIndex + step;
		if (nextIndex >= waypoints.Length || nextIndex < 0) {
			if (loopType == LoopTypes.LOOP) {
				nextIndex = nextIndex < 0 ? waypoints.Length - 1 : 0;
			} else if (loopType == LoopTypes.REVERSE) {
				step *= -1;
				nextIndex += step * 2;
			}
		}
		return nextIndex;
	}
}
