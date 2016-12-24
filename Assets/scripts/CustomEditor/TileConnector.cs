using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileConnector : ListComponent<TileConnector> {

	[SerializeField] Renderer visibleRenderer;
	[SerializeField] Material materialConnected, materialDisconnected;

	Level.Node node;
	TileConnector neighbor;
	new Collider collider;
	bool connected = false;

	void Awake () {
		gameObject.layer = Constants.tileTrianglesLayer;
		node = GetComponentInParent<Level.Node> ();
		collider = GetComponent<Collider> ();
	}

	void Start () {
		if (neighbor) {
			return;
		}
		foreach (TileConnector other in TileConnector.InstanceList) {
			if (!neighbor && node != other.node && Vector3.SqrMagnitude(transform.position - other.transform.position) < 0.001f) {
				other.neighbor = this;
				neighbor = other;
				connected = other.connected;
				other.Refresh ();
			}
		}
		Refresh ();
	}

	void Refresh () {
		if (neighbor) {
			visibleRenderer.enabled = true;
			visibleRenderer.material = Connected ? materialConnected : materialDisconnected;
			collider.enabled = true;
		} else {
			visibleRenderer.enabled = false;
			collider.enabled = false;
		}
	}

	void OnDestroy () {
		if (neighbor) {
			neighbor.neighbor = null;
			neighbor.Connected = false;
		}
	}

	public bool TryConnectToNode (Level.Node otherNode) {
		Start ();
		if (otherNode == node || !neighbor) {
			return false;
		}
		foreach (TileConnector other in otherNode.GetComponentsInChildren<TileConnector> ()) {
			if (other == neighbor) {
				Connected = true;
				return true;
			}
		}
		return false;
	}

	public bool Connected {
		get {
			return neighbor && connected;
		}
		set {
			if (neighbor) {
				connected = value;
				neighbor.connected = value;
				Refresh ();
				neighbor.Refresh ();
			}
		}
	}

}
