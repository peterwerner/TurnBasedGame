using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TileGhost : MonoBehaviour {

	new Renderer renderer;
	new Collider collider;
	bool isVertical;

	void Awake () {
		gameObject.layer = Constants.tileGhostLayer;
		renderer = GetComponent<Renderer> ();
		collider = GetComponent<Collider> ();
		isVertical = Vector3.SqrMagnitude (transform.forward - Vector3.up) > 0.001f
			&& Vector3.SqrMagnitude (transform.forward - Vector3.down) > 0.001f;
	}

	void Update () {
		bool visible = IsVisible ();
		renderer.enabled = visible;
		collider.enabled = visible;
	}

	bool IsVisible () {
		return Vector3.Angle (transform.forward, Camera.main.transform.forward) < 90;
	}

	public bool IsVertical () {
		return isVertical;
	}

}
