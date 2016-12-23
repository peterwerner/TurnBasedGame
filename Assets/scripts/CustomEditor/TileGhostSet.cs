using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGhostSet : MonoBehaviour {

	TileGhost[] ghosts;

	void Awake () {
		ghosts = GetComponentsInChildren<TileGhost> ();
	}

	public TileGhost[] Ghosts {
		get { return ghosts; }
	}
}
