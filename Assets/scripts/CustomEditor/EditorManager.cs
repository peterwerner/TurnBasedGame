using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : SingletonComponent<EditorManager> {

	public enum Modes { TILES, CONNECTIONS };

	public bool enabled3d = true;
	public Modes mode;

	[SerializeField] float cameraMoveSpeed = 10f;
	[SerializeField] float cameraFocusSize = 2f;
	[SerializeField] float cameraFocusSpeed = 1f;

	[SerializeField] TileGhostSet ghostSetPrefabHorizontal = null, ghostSetPrefabVertical = null;
	[SerializeField] Level.Node nodePrefab;
	[SerializeField] Material materialSelected, materialDefault;
	TileGhostSet ghostSetHorizontal, ghostSetVertical;
	Level.Node nodeSelected;


	void Start () {
		ghostSetHorizontal = Instantiate (ghostSetPrefabHorizontal);
		ghostSetVertical = Instantiate (ghostSetPrefabVertical);
		Select (Instantiate (nodePrefab));
		Camera.main.transform.position = nodeSelected.transform.position - 10 * Camera.main.transform.forward;
	}

	void Update () {
		
		// Handle input
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			// Tile mode
			if (mode == Modes.TILES) {
				TileGhost ghost = null;
				Level.Node node = null;
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
						node = hit.collider.GetComponent<Level.Node> ();
						if (node) {
							Select (node);
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
		if (nodeSelected) {
			Vector3 targetPos = nodeSelected.transform.position - 10 * Camera.main.transform.forward;
			Camera.main.transform.position = Vector3.MoveTowards (Camera.main.transform.position, targetPos, cameraMoveSpeed * Time.deltaTime);
			Camera.main.orthographicSize = Mathf.MoveTowards (Camera.main.orthographicSize, cameraFocusSize, cameraFocusSpeed * Time.deltaTime);
		}
	}

	void Select (Level.Node node) {
		Deselect ();
		nodeSelected = node;
		node.UpdateDirection ();
		TileGhostSet ghostSet = node.IsWall () ? ghostSetVertical : ghostSetHorizontal;
		ghostSet.transform.position = node.transform.position;
		ghostSet.transform.rotation = node.transform.rotation;
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
				}
				ghost.gameObject.SetActive (!occupied);
			}
		}
		nodeSelected.GetComponent<Tile> ().enabled = true;
		nodeSelected.GetComponentInChildren<Renderer> ().material = materialSelected;
	}

	void Deselect () {
		ghostSetHorizontal.gameObject.SetActive (false);
		ghostSetVertical.gameObject.SetActive (false);
		if (nodeSelected) {
			nodeSelected.GetComponentInChildren<Renderer> ().material = materialDefault;
			nodeSelected.GetComponent<Tile> ().enabled = false;
		}
		nodeSelected = null;
	}

	void CreateTile (TileGhost ghost) {
		Level.Node node = Instantiate (nodePrefab);
		node.transform.position = ghost.transform.position;
		node.transform.rotation = ghost.transform.rotation;
		node.transform.RotateAround (node.transform.position, node.transform.right, -90);
		node.UpdateDirection ();
		if (node.IsWall ()) {
			for (int i = 0; i < 2; i++) {
				float angle = Vector3.Angle (node.transform.forward, Vector3.down);
				node.transform.RotateAround (node.transform.position, node.transform.up, angle);
			}
		}
		foreach (TileConnector connector in node.GetComponentsInChildren<TileConnector> ()) {
			connector.TryConnectToNode (nodeSelected);
		}
		Select (node);
	}
		
}
