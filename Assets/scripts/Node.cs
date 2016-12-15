using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Level {

	public class Node : ListComponentLive<Node> {

		public static readonly float size = 1f;
		static readonly Vector3[] directionVectors = {
			Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
		};


		// Directions in which this can have neighbors (by default any direction is allowed)
		[Serializable] private class Directions {
			
			[SerializeField] bool up = true, forward = true, back = true, left = true, right = true;
			Dictionary<Vector3, bool> table = new Dictionary<Vector3, bool> ();

			public void Init () {
				table [Vector3.up] = up;
				table [Vector3.left] = left;
				table [Vector3.right] = right;
				table [Vector3.forward] = forward;
				table [Vector3.back] = back;
				table [Vector3.down] = false;
			}

			public void DrawGizmo(Vector3 pos, float size) {
				foreach (Vector3 dir in table.Keys) {
					Gizmos.color = table[dir] ? Color.green : Color.red;
					Gizmos.DrawLine(pos, pos + size * dir);
				}
			}

			public bool IsEnabledInDirection(Vector3 direction) {
				bool result;
				return table.TryGetValue (direction, out result) && result;
			}
		}


		/* Node */

			
		[SerializeField] Directions directionsAllowed;
		List<Node> neighbors = new List<Node> ();
		Dictionary<Vector3, List<Node> > nodesLOS = new Dictionary<Vector3, List<Node> > ();
		HashSet<Actor> actors = new HashSet<Actor> ();
		Vector3 direction;
		SphereCollider sphereCol; // Used only in LOS calculations

		public static void InitAll () {
			foreach (Node node in InstanceList) {
				node.directionsAllowed.Init ();
				node.UpdateDirection ();
				foreach (Collider col in node.gameObject.GetComponents<Collider> ()) {
					DestroyImmediate (col);
				}
			}
			foreach (Node node in InstanceList) {
				node.UpdateNeighbors (InstanceList);
				node.sphereCol = node.gameObject.AddComponent<SphereCollider> ();
			}
			foreach (Node node in InstanceList) {
				node.UpdateNodesLOS (InstanceList);
			}
			foreach (Node node in InstanceList) {
				DestroyImmediate (node.gameObject.GetComponent<SphereCollider> ());
				BoxCollider boxCollider = node.gameObject.AddComponent<BoxCollider> ();
				boxCollider.size = new Vector3 (size, 0.02f, size);
			}
		}

		void UpdateNodesLOS(List<Node> nodes) {
			nodesLOS.Clear ();
			sphereCol.radius = 0.3f * size;

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
				nodesLOS[direction].OrderBy(o => Vector3.SqrMagnitude(o.transform.position - transform.position));
			}
		}

		void UpdateNeighbors(List<Node> nodes) {
			neighbors.Clear();
			foreach (Node node in nodes){
				if (IsConnectable(node)) {
					neighbors.Add (node);
				}
			}
		}

		void UpdateDirection() {
			float angleMin = 181;
			foreach (Vector3 direction in directionVectors) {
				float angle = Vector3.Angle (direction, transform.up);
				if (angle < angleMin) {
					angleMin = angle;
					this.direction = direction;
				}
			}
		}

		public Node[] GetNeighbors () {
			return neighbors.ToArray();
		}
	
		public void AddActor (Actor actor, bool doCallMeetCallback = false) {
			if (doCallMeetCallback) {
				foreach (Actor other in actors) {
					actor.Meet (other);
				}
			}
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

		/* Helpers for determining relationship */

		bool IsConnectable (Node other) {
			// If other is within 1 'size' of this, it may be connectable
			if (other != this && Vector3.SqrMagnitude (transform.position - other.transform.position) <= size * size * 1.1f) {
				// Same plane, must be adjacent
				if (direction == other.direction) {
					return Mathf.Approximately (
						Vector3.Scale (transform.position, direction).sqrMagnitude,
						Vector3.Scale (other.transform.position, direction).sqrMagnitude
					)
					&& Mathf.Approximately (
						Vector3.Distance (transform.position, other.transform.position),
						size
					)
					&& directionsAllowed.IsEnabledInDirection (
						VectorUtil.ClosestCardinalDirection (other.transform.position - transform.position)
					)
					&& other.directionsAllowed.IsEnabledInDirection (
						VectorUtil.ClosestCardinalDirection (transform.position - other.transform.position)
					);
				}
				// 90-degree difference between planes, must be on the same 'cube' ( _| or |_ etc )
				if (Mathf.Approximately (Vector3.Angle (direction, other.direction), 90)) {
					return Mathf.Approximately (Vector3.Distance (
						transform.position + direction * size * 0.5f,
						other.transform.position + other.direction * size * 0.5f
					), 0)
					|| Mathf.Approximately (Vector3.Distance (
						transform.position - direction * size * 0.5f,
						other.transform.position - other.direction * size * 0.5f
					), 0)
					&& directionsAllowed.IsEnabledInDirection (
						VectorUtil.ClosestCardinalDirection (other.transform.position - transform.position, false)
					)
					&& other.directionsAllowed.IsEnabledInDirection (
						VectorUtil.ClosestCardinalDirection (transform.position - other.transform.position, false)
					);
				}
			}
			return false;
		}

		public Vector3 Direction {
			get { return direction; }
		}

		public bool IsWall () {
			return direction != Vector3.up && direction != Vector3.down;
		}

		public bool IsAbove (Node other) {
			return transform.position.y > other.transform.position.y + Mathf.Epsilon;
		}

		public bool IsBelow (Node other) {
			return transform.position.y + Mathf.Epsilon < other.transform.position.y;
		}

		public bool HasNeighbor (Node other) {
			return neighbors.Contains(other);
		}


		void Awake () {
			if (directionsAllowed == null) {
				directionsAllowed = new Directions ();
			}
		}

		// Editor only

		void OnValidate () {
			directionsAllowed.Init ();
			foreach (Node node in InstanceList) {
				UpdateNeighbors (InstanceList);
			}
		}

		void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawLine (transform.position, transform.position + transform.up * 0.3f);
			directionsAllowed.DrawGizmo (transform.position + transform.up * 0.01f, size * 0.15f);
			Gizmos.color = Color.green;
			if (neighbors != null) {
				foreach (Node neighbor in neighbors) {
					Gizmos.DrawLine (this.transform.position, neighbor.transform.position);
				}
			}
		}

	}

}