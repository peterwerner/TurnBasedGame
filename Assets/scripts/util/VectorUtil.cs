using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorUtil : MonoBehaviour {

	static readonly Vector3[] directionsHorz = {
		Vector3.left, Vector3.right, Vector3.forward, Vector3.back
	};
	static readonly Vector3[] directionsAll = {
		Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
	};

	public static Vector3 ClosestDirection (Vector3 targetDir, IEnumerable<Vector3> dirs) {
		float angleMin = Mathf.Infinity;
		Vector3 closest = Vector3.zero;
		foreach (Vector3 dir in dirs) {
			float angle = Vector3.Angle (dir, targetDir);
			if (angle < angleMin) {
				angleMin = angle;
				closest = dir;
			}
		}
		return closest;
	}

	public static Vector3 ClosestCardinalDirection (Vector3 targetDir, bool includeVertical = true) {
		return ClosestDirection (targetDir, includeVertical ? directionsAll : directionsHorz);
	}

	public static Vector3 Round (Vector3 v, int decimalPlaces = 0) {
		float factor = Mathf.Pow (10, decimalPlaces);
		return new Vector3 (
			Mathf.Round (v.x * factor) / factor,
			Mathf.Round (v.y * factor) / factor,
			Mathf.Round (v.z * factor) / factor
		);
	}
}
