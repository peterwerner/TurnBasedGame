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
		List<Level.Node> path = GetPath ();
		if (path != null && path.Count > 0) {
			Vector3 direction = VectorUtil.ClosestCardinalDirection (path[0].transform.position - this.Node.transform.position);
			Vector3 prevPos = this.Node.transform.position;
			while (true) {
				foreach (Level.Node node in path) {
					if (VectorUtil.ClosestCardinalDirection (node.transform.position - prevPos) != direction) {
						return;
					}
					MovePath.Add (node);
					prevPos = node.transform.position;
				}
				path = GetPath ();
			}
		}
	}
}
