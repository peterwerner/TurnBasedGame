using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActorMove : Actor {

	[SerializeField] float moveSpeed = 5f;

	protected virtual void Update () {
		// TODO: mechanim ?
		transform.position = Vector3.MoveTowards (transform.position, node.transform.position, moveSpeed * Time.deltaTime);
		transform.LookAt (transform.position + lookDir);
		if (!IsTurnEnded () && Mathf.Approximately(Vector3.Distance(transform.position, node.transform.position), 0f)) {
			EndTurn ();
		}	
	}

}
