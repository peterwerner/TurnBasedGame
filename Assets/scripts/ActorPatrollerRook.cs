using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorPatrollerRook : ActorPatroller {

	protected override void UpdateMovePath() {
		if (waypoints.Length <= 0) {
			return;
		}
		MovePath.Clear ();
		MovePath.Add (this.Node);
		List<Level.Node> path = GetPath (false);
		if (path != null && path.Count > 0) {
			Vector3 direction = VectorUtil.ClosestCardinalDirection (path[0].transform.position - this.Node.transform.position);
			Level.Node prevNode = this.Node;
			for (int i = 0; i < waypoints.Length; i++) {
				foreach (Level.Node node in path) {
					if (VectorUtil.ClosestCardinalDirection (node.transform.position - prevNode.transform.position) != direction) {
						return;
					}
					MovePath.Add (node);
					prevNode = node;
				}
				path = GetPath (false, prevNode);
			}
		}
	}
}
