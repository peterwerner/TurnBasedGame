using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : SingletonComponent<EditorManager> {

	[SerializeField] TileGhostSet ghostSetPrefabHorizontal = null, ghostSetPrefabVertical = null;
	[SerializeField] Level.Node nodePrefab;
	TileGhostSet ghostSetHorizontal, ghostSetVertical;
	Level.Node nodeSelected;

	void Start () {
		ghostSetHorizontal = Instantiate (ghostSetPrefabHorizontal);
		ghostSetVertical = Instantiate (ghostSetPrefabVertical);
		Deselect ();
	}

	void Update () {
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
	}

	void Select (Level.Node node) {
		Deselect ();
		node.UpdateDirection ();
		nodeSelected = node;
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
		EditorUI.Instance.SetNodeSelected (true);
	}

	void Deselect () {
		ghostSetHorizontal.gameObject.SetActive (false);
		ghostSetVertical.gameObject.SetActive (false);
		nodeSelected = null;
		EditorUI.Instance.SetNodeSelected (false);
	}

	void CreateTile (TileGhost ghost) {
		Level.Node node = Instantiate (nodePrefab);
		node.transform.position = ghost.transform.position;
		node.transform.up = - ghost.transform.forward;
		Select (node);
	}
		
	/* Events / actions triggered by UI */

	public void ToggleGhostDirection () {
		if (nodeSelected) {
			if (nodeSelected.IsWall ()) {
				TileGhost.modeVertical = TileGhost.modeVertical == TileGhost.Mode.FACE
					? TileGhost.Mode.NONFACE
					: TileGhost.Mode.FACE;
			} else {
				TileGhost.modeHorizontal = TileGhost.modeHorizontal == TileGhost.Mode.FACE
					? TileGhost.Mode.NONFACE
					: TileGhost.Mode.FACE;
			}
			Select (nodeSelected);
		}
	}

}
