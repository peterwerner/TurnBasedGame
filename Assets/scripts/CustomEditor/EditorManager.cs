using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : SingletonComponent<EditorManager> {

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
		Camera.main.transform.position = nodeSelected.transform.position - 100 * Camera.main.transform.forward;
	}

	void Update () {
		// Handle input
		if (Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			LayerMask layerMask = (1 << Constants.tileGhostLayer) | (1 << Constants.nodeLayer);
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {
				TileGhost ghost = hit.collider.GetComponent<TileGhost> ();
				if (ghost) {
					CreateTile (ghost);
				} else {
					Level.Node node = hit.collider.GetComponent<Level.Node> ();
					if (node) {
						Select (node);
					} else {
						// Deselect ();
					}
				}
			} else {
				// Deselect ();
			}
		}
		// Handle camera movement
		if (nodeSelected) {
			Vector3 targetPos = nodeSelected.transform.position - 100 * Camera.main.transform.forward;
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
		nodeSelected.GetComponentInChildren<Renderer> ().material = materialSelected;
	}

	void Deselect () {
		ghostSetHorizontal.gameObject.SetActive (false);
		ghostSetVertical.gameObject.SetActive (false);
		if (nodeSelected) {
			nodeSelected.GetComponentInChildren<Renderer> ().material = materialDefault;
		}
		nodeSelected = null;
	}

	void CreateTile (TileGhost ghost) {
		Level.Node node = Instantiate (nodePrefab);
		node.transform.position = ghost.transform.position;
		node.transform.rotation = ghost.transform.rotation;
		node.transform.RotateAround (node.transform.position, node.transform.right, -90);
		Select (node);
	}
		
	/* Events / actions triggered by UI */

}
