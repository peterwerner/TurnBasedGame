using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorMove : Actor {

	[SerializeField] float moveSpeed = 5f;

	protected virtual void Update () {
		// TODO: mechanim ?
		transform.position = Vector3.MoveTowards (transform.position, this.Node.transform.position, moveSpeed * Time.deltaTime);
		if (MovePath.Count > 1) {
			lookDir = MovePath [1].transform.position - MovePath [0].transform.position;
		}
		transform.LookAt (transform.position + lookDir);
		if (!IsTurnEnded () && Mathf.Approximately(Vector3.Distance(transform.position, this.Node.transform.position), 0f)) {
			EndTurn ();
		}	
	}

}
