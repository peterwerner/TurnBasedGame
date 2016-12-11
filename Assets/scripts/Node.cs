using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Level {

	public class Node : ListComponent<Node> {

		public static readonly float size = 1f;
		static readonly float angleTolerance = 1f, distanceTolerance = 0.05f * size;

		public enum RelationshipTypes {
			NONE,
			GROUND_GROUND,
			WALL_WALL,
			GROUND_WALL_UP,
			GROUND_WALL_DOWN,
			WALL_GROUND_UP,
			WALL_GROUND_DOWN
		};

		// Relationship from this node to another node
		private class Relationship {

			public Node node;
			public RelationshipTypes type;

			public Relationship(Node node, RelationshipTypes type) {
				this.node = node;
				this.type = type;
			}

			// Returns the relationship a -> b (where the node field of the resulting Relationship is b)
			public static Relationship GetRelationship(Node a, Node b) {
				float verticalDistance = Mathf.Abs (a.transform.position.y - b.transform.position.y);
				float horizontalDistance = Mathf.Sqrt (
					Mathf.Pow(a.transform.position.x - b.transform.position.x, 2)
					+ Mathf.Pow(a.transform.position.z - b.transform.position.z, 2)
				);
				// ground -> ...
				if (Vector3.Angle (a.transform.up, Vector3.up) < 45) {
					Vector3 horizontalDirection = b.transform.position - a.transform.position;
					horizontalDirection.y = 0;
					if (a.directions.IsEnabledInDirection(horizontalDirection)) {
						// ground -> ground
						if (Vector3.Angle (b.transform.up, Vector3.up) < angleTolerance) {
							if (verticalDistance < distanceTolerance
								&& Mathf.Abs (horizontalDistance - size) < distanceTolerance
								&& b.directions.IsEnabledInDirection(horizontalDirection * -1)
							) {
								return new Relationship (b, RelationshipTypes.GROUND_GROUND);
							}
						}
						// ground -> wall (climb up)
						// ground -> wall (climb down)
					}
				} 
				// wall -> ...
				else {
					// wall -> wall
					// wall -> ground (climb up)
					// wall -> ground (climb down)
					// none
				}
				return null;
			}
		}


		// Directions in which this can have neighbors (by default any direction is allowed)
		[Serializable] private class Directions {
			
			public bool up = true, down = false, forward = true, back = true, left = true, right = true;

			public void DrawGizmo(Vector3 pos, float size) {
				DrawLine (pos, size, Vector3.up, up);
				DrawLine (pos, size, Vector3.down, down);
				DrawLine (pos, size, Vector3.forward, forward);
				DrawLine (pos, size, Vector3.back, back);
				DrawLine (pos, size, Vector3.left, left);
				DrawLine (pos, size, Vector3.right, right);
			}

			public bool IsEnabledInDirection(Vector3 direction) {
				if (Vector3.Angle(direction, Vector3.up) < angleTolerance) { return up; }
				if (Vector3.Angle(direction, Vector3.down) < angleTolerance) { return down; }
				if (Vector3.Angle(direction, Vector3.forward) < angleTolerance) { return forward; }
				if (Vector3.Angle(direction, Vector3.back) < angleTolerance) { return back; }
				if (Vector3.Angle(direction, Vector3.left) < angleTolerance) { return left; }
				if (Vector3.Angle(direction, Vector3.right) < angleTolerance) { return right; }
				return false;
			}

			private void DrawLine(Vector3 pos, float size, Vector3 dir, bool condition) {
				Gizmos.color = condition ? Color.green : Color.red;
				Gizmos.DrawLine(pos, pos + size * dir);
			}
		}


		/* Node */

			
		[SerializeField] Directions directions;
		List<Relationship> neighbors = new List<Relationship> ();
		HashSet<Actor> actors = new HashSet<Actor> ();


		public void UpdateNeighbors(List<Node> nodes) {
			neighbors.Clear();
			foreach (Node node in nodes){
				if (node != this)
				{
					Relationship relationship = Relationship.GetRelationship (this, node);
					if (relationship != null) {
						neighbors.Add (relationship);
					}
				}
			}
		}

		public RelationshipTypes GetRelationship (Node node) {
			foreach (Relationship relationship in neighbors) {
				if (relationship.node == node) {
					return relationship.type;
				}
			}
			return RelationshipTypes.NONE;
		}

		public void AddActor (Actor actor) {
			actors.Add(actor);
		}

		public void RemoveActor (Actor actor) {
			actors.Remove(actor);
		}

		public List<Node> GetNeighbors () {
			List<Node> nodes = new List<Node> ();
			foreach (Relationship relationship in neighbors) {
				nodes.Add (relationship.node);
			}
			return nodes;
		}
	

		void Start() {
			// TODO: do this at build time not at run time
			UpdateNeighbors (Node.InstanceList);
			gameObject.AddComponent<SphereCollider> ().radius = 0.25f * size;
		}

		void OnMouseOver() {
			ActorPlayer.OnHoverNode (this);
		}
		void OnMouseDown() {
			ActorPlayer.OnClickNode (this);
		}
		void OnMouseExit() {
			ActorPlayer.OnUnhoverNode (this);
		}

		void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawLine (transform.position, transform.position + transform.up * 0.3f);
			directions.DrawGizmo (transform.position, size * 0.15f);
			Gizmos.color = Color.green;
			if (neighbors != null) {
				foreach (Relationship relationship in neighbors) {
					Gizmos.DrawLine (this.transform.position, relationship.node.transform.position);
				}
			}
		}

	}

}