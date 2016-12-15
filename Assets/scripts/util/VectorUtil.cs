﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorUtil : MonoBehaviour {

	static readonly Vector3[] directionsHorz = {
		Vector3.left, Vector3.right, Vector3.forward, Vector3.back
	};
	static readonly Vector3[] directionsAll = {
		Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
	};

	public static Vector3 ClosestCardinalDirection (Vector3 targetDir, bool includeVertical = true) {
		float angleMin = 360;
		Vector3 closest = Vector3.zero;
		Vector3[] dirs = includeVertical ? directionsAll : directionsHorz;
		foreach (Vector3 dir in dirs) {
			float angle = Vector3.Angle (dir, targetDir);
			if (angle < angleMin) {
				angleMin = angle;
				closest = dir;
			}
		}
		return closest;
	}
}
