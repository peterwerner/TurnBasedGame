﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Level {

	public class Node : ListComponent<Node> {

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

		void UpdateNodesLOS(List<Node> nodes) {
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

		/* Helpers for determining relationship */

		bool IsConnectable (Node other) {
			// If other is within 1 'size' of this, it may be connectable
			if (other != this && Vector3.SqrMagnitude (transform.position - other.transform.position) <= size * size + Mathf.Epsilon) {
				// Same plane, must be adjacent
				if (direction == other.direction) {
					return Mathf.Approximately (
							Vector3.Scale (transform.position, direction).sqrMagnitude,
							Vector3.Scale (other.transform.position, direction).sqrMagnitude
						)
						&& Mathf.Approximately (
							Vector3.Distance(transform.position, other.transform.position),
							size
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
						), 0); 
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

		/* Lifecycle */
	
		void Awake () {
			gameObject.AddComponent<SphereCollider> ().radius = 0.3f * size;
			UpdateDirection ();
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
			directionsAllowed.DrawGizmo (transform.position, size * 0.15f);
			Gizmos.color = Color.green;
			if (neighbors != null) {
				foreach (Node neighbor in neighbors) {
					Gizmos.DrawLine (this.transform.position, neighbor.transform.position);
				}
			}
		}

	}

}