using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Level {

	public class Node : ListComponent<Node> {

		public static readonly float size = 1f;
		static readonly float angleTolerance = 1f, distanceTolerance = 0.05f * size;
		static readonly Vector3[] directionVectors = {
			Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
		};

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
		Dictionary<Vector3, List<Node> > nodesLOS = new Dictionary<Vector3, List<Node> > ();
		HashSet<Actor> actors = new HashSet<Actor> ();

		public void UpdateNodesLOS(List<Node> nodes) {
			nodesLOS.Clear ();
			Ray ray = new Ray (transform.position + transform.up * 0.26f * size, Vector3.zero);
			RaycastHit hit;
			RaycastHit[] hits;
			float maxDist = size * InstanceList.Count;
			LayerMask layers = GameManager.GetNodeLOSLayers (); // All layers that block LOS (including node layer)
			LayerMask nodeLayerMask = 1 << gameObject.layer; // Node layer
			foreach (Vector3 direction in directionVectors) {
				ray.direction = direction;
				// Cast until we hit a boundary
				bool didHit = Physics.Raycast (ray, out hit, maxDist, layers & ~nodeLayerMask);
				// Cast to get all colliders closer than the boundary
				hits = Physics.RaycastAll(ray, didHit ? hit.distance : maxDist, nodeLayerMask);
				nodesLOS [direction] = new List<Node> ();
				foreach (RaycastHit prospectiveHit in hits) {
					Node node = prospectiveHit.collider.GetComponent<Node> ();
					if (node && node != this) {
						nodesLOS[direction].Add(node);
					}
				}
				// Sort by distance such that the shortest distance is the first element
				nodesLOS[direction].OrderBy(o => Vector3.SqrMagnitude(o.transform.position - transform.position)).ToList();
			}
		}

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

		public List<Node> GetNeighbors () {
			List<Node> nodes = new List<Node> ();
			foreach (Relationship relationship in neighbors) {
				nodes.Add (relationship.node);
			}
			return nodes;
		}

		public void AddActor (Actor actor) {
			actors.Add(actor);
		}

		public void RemoveActor (Actor actor) {
			actors.Remove(actor);
		}

		public HashSet<Actor> GetActors () {
			return actors;
		}

		public Dictionary<Vector3, List<Actor> > GetActorsInLOS () {
			Dictionary<Vector3, List<Actor> > actorsDict = new Dictionary<Vector3, List<Actor> >();
			foreach (Vector3 direction in nodesLOS.Keys) {
				List<Actor> actorsLOS = new List<Actor> ();
				foreach (Node node in nodesLOS[direction]) {
					actorsLOS.AddRange (node.actors);
				}
				if (actorsLOS.Count > 0) {
					actorsDict [direction] = actorsLOS;
				}
			}
			return actorsDict;
		}

	
		void Awake () {
			gameObject.AddComponent<SphereCollider> ().radius = 0.3f * size;
		}

		void Start() {
			// TODO: do this at build time not at run time
			UpdateNeighbors (Node.InstanceList);
			UpdateNodesLOS (Node.InstanceList);
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