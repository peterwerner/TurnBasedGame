using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActorPatroller : ActorMove {

	private enum LoopTypes { LOOP, REVERSE };

	[SerializeField] LoopTypes loopType;
	[SerializeField] protected Level.Node[] waypoints;
	int waypointIndex = 0, step = 1;

	protected override void UpdateMovePath() {
		if (waypoints.Length <= 0) {
			return;
		}
		MovePath.Clear ();
		MovePath.Add (this.Node);
		List<Level.Node> path = GetPath ();
		if (path != null && path.Count > 0) {
			MovePath.Add (path [0]);
		}
	}

	protected List<Level.Node> GetPath (bool allowWallNodes = true, Level.Node startingNode = null) {
		if (startingNode == null) {
			startingNode = this.Node;
		}
		if (startingNode == waypoints [waypointIndex]) {
			waypointIndex = GetNextWaypointIndex ();
		}
		List<Level.Node> path;
		for (int attempts = 0; attempts < waypoints.Length; attempts++) {
			path = Level.NodeAStar.ShortestPath (startingNode, waypoints [waypointIndex], allowWallNodes);
			if (path != null && path.Count > 0) {
				return path;
			} else {
				waypointIndex = GetNextWaypointIndex ();
			}
		}
		return null;
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
