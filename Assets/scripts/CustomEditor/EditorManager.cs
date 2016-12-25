using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : SingletonComponent<EditorManager> {

	public enum Modes { TILES, CONNECTIONS };

	public bool enabled3d = true;
	Modes mode;

	[SerializeField] float cameraMoveSpeed = 10f;
	[SerializeField] float cameraFocusSize = 2f;
	[SerializeField] float cameraFocusSpeed = 1f;

	[SerializeField] TileGhostSet ghostSetPrefabHorizontal = null, ghostSetPrefabVertical = null;
	[SerializeField] Tile tilePrefab;
	TileGhostSet ghostSetHorizontal, ghostSetVertical;
	Tile tileSelected;


	void Start () {
		ghostSetHorizontal = Instantiate (ghostSetPrefabHorizontal);
		ghostSetVertical = Instantiate (ghostSetPrefabVertical);
		Select (Instantiate (tilePrefab));
		Camera.main.transform.position = tileSelected.transform.position - 10 * Camera.main.transform.forward;
	}

	void Update () {
		
		// Handle input
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			// Tile mode
			if (mode == Modes.TILES) {
				TileGhost ghost = null;
				Tile tile = null;
				LayerMask layerMask = 1 << Constants.tileGhostLayer;
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
					ghost = hit.collider.GetComponent<TileGhost> ();
					if (ghost) {
						CreateTile (ghost);
					}
				}
				if (!ghost) {
					layerMask = layerMask | (1 << Constants.nodeLayer);
					if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
						tile = hit.collider.GetComponent<Tile> ();
						if (tile) {
							Select (tile);
						}
					}
				}
			}
			// Connection mode
			else if (mode == Modes.CONNECTIONS) {
				LayerMask layerMask = 1 << Constants.tileTrianglesLayer;
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
					TileConnector connector = hit.collider.GetComponent<TileConnector> ();
					if (connector) {
						connector.Connected = !connector.Connected;
					}
				}
			}
		}

		// Handle camera movement
		if (tileSelected) {
			Vector3 targetPos = tileSelected.transform.position - 10 * Camera.main.transform.forward;
			Camera.main.transform.position = Vector3.MoveTowards (Camera.main.transform.position, targetPos, cameraMoveSpeed * Time.deltaTime);
			Camera.main.orthographicSize = Mathf.MoveTowards (Camera.main.orthographicSize, cameraFocusSize, cameraFocusSpeed * Time.deltaTime);
		}
	}

	void Select (Tile tile) {
		Deselect ();
		tileSelected = tile;
		TileGhostSet ghostSet = tile.IsVertical () ? ghostSetVertical : ghostSetHorizontal;
		ghostSet.transform.position = tile.transform.position;
		ghostSet.transform.rotation = tile.transform.rotation;
		ghostSet.gameObject.SetActive (true);
		foreach (TileGhost ghost in ghostSet.Ghosts) {
			if (!enabled3d && ghost.IsVertical()) {
				ghost.gameObject.SetActive (false);
			} else {
				bool occupied = false;
				foreach (Level.Node otherNode in Level.Node.InstanceList) {
					// Ghost is at a node that already exists
					if (Vector3.SqrMagnitude (ghost.transform.position - otherNode.transform.position) < 0.001f) {
						occupied = true;
						break;
					}
					/*
					// Ghost's backface points to an existing node's face
					Vector3 ghostBackPos = ghost.transform.position + ghost.transform.forward * Level.Node.size * 0.5f;
					Vector3 otherNodePos = otherNode.transform.position + otherNode.transform.up * Level.Node.size * 0.5f;
					if (Vector3.SqrMagnitude (ghostBackPos - otherNodePos) < 0.001f) {
						occupied = true;
						break;
					}
					// Ghost points to an existing node's backface
					ghostBackPos = ghost.transform.position - ghost.transform.forward * Level.Node.size * 0.5f;
					otherNodePos = otherNode.transform.position - otherNode.transform.up * Level.Node.size * 0.5f;
					if (Vector3.SqrMagnitude (ghostBackPos - otherNodePos) < 0.001f) {
						occupied = true;
						break;
					}
					*/
				}
				ghost.gameObject.SetActive (!occupied);
			}
		}
		tileSelected.GetComponent<Tile> ().Pulse (true);
	}

	void Deselect () {
		ghostSetHorizontal.gameObject.SetActive (false);
		ghostSetVertical.gameObject.SetActive (false);
		if (tileSelected) {
			tileSelected.GetComponent<Tile> ().Pulse (false);
		}
		tileSelected = null;
	}

	void CreateTile (TileGhost ghost) {
		Tile tile = Instantiate (tilePrefab);
		tile.transform.position = ghost.transform.position;
		tile.transform.rotation = ghost.transform.rotation;
		tile.transform.RotateAround (tile.transform.position, tile.transform.right, -90);
		if (tile.IsVertical ()) {
			for (int i = 0; i < 2; i++) {
				float angle = Vector3.Angle (tile.transform.forward, Vector3.down);
				tile.transform.RotateAround (tile.transform.position, tile.transform.up, angle);
			}
		}
		foreach (TileConnector connector in tile.GetComponentsInChildren<TileConnector> ()) {
			connector.TryConnectToTile (tileSelected);
		}
		Select (tile);
	}

	public Modes Mode {
		set {
			if (value != Modes.TILES && mode == Modes.TILES) {
				Deselect ();
			}
			mode = value;
		}
	}
		
}
