using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TileGhost : MonoBehaviour {

	new Renderer renderer;
	new Collider collider;

	void Awake () {
		gameObject.layer = Constants.tileGhostLayer;
		renderer = GetComponent<Renderer> ();
		collider = GetComponent<Collider> ();
	}

	void Update () {
		bool visible = IsVisible ();
		renderer.enabled = visible;
		collider.enabled = visible;
	}

	bool IsVisible () {
		return Vector3.Angle (transform.forward, Camera.main.transform.forward) < 90;
	}

}
