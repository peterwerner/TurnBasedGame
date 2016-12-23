using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorManager : SingletonComponent<EditorManager> {

	[SerializeField] TileGhostSet ghostSetPrefabHorizontal = null, ghostSetPrefabVertical = null;
	[SerializeField] Level.Node nodePrefab;
	TileGhostSet ghostSetHorizontal, ghostSetVertical;
	Level.Node nodeSelected;

	void Awake () {
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
						Deselect ();
					}
				}
			} else {
				Deselect ();
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
				if (Vector3.SqrMagnitude (ghost.transform.position - otherNode.transform.position) < 0.001f) {
					occupied = true;
					break;
				}
			}
			ghost.gameObject.SetActive (!occupied);
		}
	}

	void Deselect () {
		ghostSetHorizontal.gameObject.SetActive (false);
		ghostSetVertical.gameObject.SetActive (false);
		nodeSelected = null;
	}

	void CreateTile (TileGhost ghost) {
		Level.Node node = Instantiate (nodePrefab);
		node.transform.position = ghost.transform.position;
		node.transform.up = - ghost.transform.forward;
		Select (node);
	}
}
