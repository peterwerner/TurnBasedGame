using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileConnector : ListComponent<TileConnector> {

	[SerializeField] Renderer visibleRenderer;
	[SerializeField] Material materialConnected, materialDisconnected;

	Tile tile;
	TileConnector partner;
	new Collider collider;
	bool connected = false;

	void Awake () {
		gameObject.layer = Constants.tileTrianglesLayer;
		tile = GetComponentInParent<Tile> ();
		collider = GetComponent<Collider> ();
	}

	void Start () {
		UpdatePartner ();
	}
		
	void UpdatePartner () {
		partner = null;
		// Try to connect to all other connectors at the same position
		foreach (TileConnector other in TileConnector.InstanceList) {
			if (!partner && tile != other.tile && Vector3.SqrMagnitude(transform.position - other.transform.position) < 0.001f) {
				// Invalid - tile's backface points to other tile's face
				Vector3 tileBackPos = tile.transform.position - tile.transform.up * Tile.size * 0.5f;
				Vector3 othertilePos = other.tile.transform.position + other.tile.transform.up * Tile.size * 0.5f;
				if (Vector3.SqrMagnitude (tileBackPos - othertilePos) < 0.001f) {
					continue;
				}
				// Invalid - tile's face points to other tile's backface
				Vector3 tilePos = tile.transform.position + tile.transform.up * Tile.size * 0.5f;
				Vector3 othertileBackPos = other.tile.transform.position - other.tile.transform.up * Tile.size * 0.5f;
				if (Vector3.SqrMagnitude (tilePos - othertileBackPos) < 0.001f) {
					continue;
				}
				// Invalid - blocking tile in the way
				bool blocked = false;
				foreach (Tile blockerTile in Tile.InstanceList) {
					if (blockerTile != tile && blockerTile != other.tile) {
						Vector3 blockPos = blockerTile.transform.position;
						if (Vector3.SqrMagnitude (blockPos - (transform.position + tile.transform.up * Tile.size * 0.5f)) < 0.001f
						    || Vector3.SqrMagnitude (blockPos - (other.transform.position + other.tile.transform.up * Tile.size * 0.5f)) < 0.001f
						) {
							blocked = true;
							break;
						}
					}
				}
				if (blocked) {
					continue;
				}
				// Valid connection
				if (other.partner) {
					other.partner.partner = null;
					other.partner.Refresh ();
				}
				other.partner = this;
				connected = other.connected;
				partner = other;
				partner.Refresh ();
			}
		}
		Refresh ();
	}

	void Refresh () {
		if (partner) {
			visibleRenderer.enabled = true;
			visibleRenderer.material = Connected ? materialConnected : materialDisconnected;
			collider.enabled = true;
		} else {
			visibleRenderer.enabled = false;
			collider.enabled = false;
		}
	}

	void OnDestroy () {
		if (partner) {
			partner.partner = null;
			partner.Connected = false;
		}
	}

	public bool TryConnectToTile (Tile otherTile) {
		UpdatePartner ();
		if (otherTile == tile || !partner) {
			return false;
		}
		foreach (TileConnector other in otherTile.TileConnectors) {
			if (other == partner) {
				Connected = true;
				return true;
			}
		}
		return false;
	}

	public bool Connected {
		get {
			return partner && connected;
		}
		set {
			if (partner) {
				connected = value;
				partner.connected = value;
				Refresh ();
				partner.Refresh ();
			}
		}
	}

	void OnDrawGizmos () {
		if (partner) {
			Gizmos.color = Connected ? Color.cyan : Color.yellow;
			Gizmos.DrawLine (tile.transform.position + tile.transform.up * 0.1f, partner.tile.transform.position + partner.tile.transform.up * 0.1f);
		}
	}

}
