using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level {

	public class NodeAStar {

		private class NodeMeta {
			public float f = 0, g = 0;
			public Node predecessor = null;
		}

		static Dictionary<Node, NodeMeta> meta;

		public static void Init () {
			meta = new Dictionary<Node, NodeMeta> ();
			foreach (Node node in Node.InstanceList) {
				meta [node] = new NodeMeta ();
			}
		}

		/// <summary>Returns the shortest path from start (exclusive) to destination (inclusive)</summary>
		public static List<Node> ShortestPath (Node start, Node destination, bool allowWallNodes = true) {
			if (!allowWallNodes && (start.IsWall() || destination.IsWall())) {
				return null;
			}
			List<Node> path = new List<Node> ();
			if (start == destination)
				return path;
			List<Node> open = new List<Node>();
			List<Node> closed = new List<Node>();
			Node current;
			open.Add(start);
			while (open.Count > 0) 
			{
				// Take the least-cost open node
				current = null;
				foreach (Node prospective in open)
					if (current == null || meta[prospective].f < meta[current].f)
						current = prospective;
				// Recontruct the path and return if we have reached destination
				if (current == destination)
					return GetPathList(start, destination);
				// Update all successor nodes
				foreach (Node successor in current.GetNeighbors()) {
					// If successor is already fully evaluated, skip it
					if (closed.Contains(successor))
						continue;
					// Calculate but do not store successor's prior cost
					float g_prospective = meta[current].g + 1;
					// If the node has not been discovered, add it to open
					if (!open.Contains(successor) && (allowWallNodes || !successor.IsWall()))
						open.Add(successor);
					// If successor has been discovered but not fully evaluated, update it IFF we can improve its cost
					else if (g_prospective >= meta[successor].g) 
						continue;
					// This is the current best path, update successor accordingly
					NodeMeta successorMeta = meta[successor];
					successorMeta.predecessor = current;
					successorMeta.g = g_prospective;
					successorMeta.f = successorMeta.g + Vector3.Distance(successor.transform.position, destination.transform.position) / (Node.size * 0.707f);
				}
				open.Remove(current);
				closed.Add(current);
			}
			return null;
		}

		static List<Node> GetPathList (Node start, Node destination)
		{
			List<Node> path = new List<Node>();
			Node current = destination;
			while (current != start) {
				path.Add(current);
				current = meta[current].predecessor;
			}
			path.Reverse();
			return path;
		}
	}

}