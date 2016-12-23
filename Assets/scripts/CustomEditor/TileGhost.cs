using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class TileGhost : MonoBehaviour {

	public enum Type { VERTICAL, HORIZONTAL };
	public enum Mode { FACE, NONFACE };

	public static Mode modeVertical = Mode.FACE;
	public static Mode modeHorizontal = Mode.NONFACE;

	[SerializeField] Type type;
	[SerializeField] Mode mode;
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
		return IsRightMode () && Vector3.Angle (transform.forward, Camera.main.transform.forward) < 90;
	}

	bool IsRightMode () {
		if (type == Type.VERTICAL) {
			return mode == modeVertical;
		} else {
			return mode == modeHorizontal;
		}
	}
}
