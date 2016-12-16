using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class ActorMove : Actor {

	[SerializeField] float moveSpeed = 5f;
	int index = 0;

	protected override void OnTurnStart () {}

	protected override void Update () {
		base.Update ();
		// TODO: mechanim ?
		transform.position = Vector3.MoveTowards (transform.position, MovePath[index].transform.position, moveSpeed * Time.deltaTime);
		if (MovePath.Count > 1) {
			lookDir = MovePath [1].transform.position - MovePath [0].transform.position;
		}
		transform.LookAt (transform.position + lookDir);
		if (!IsTurnEnded () && Mathf.Approximately(Vector3.Distance(transform.position, MovePath[index].transform.position), 0f)) {
			if (index < MovePath.Count - 1) {
				index++;
			} else {
				index = 0;
				EndTurn ();
			}
		}
	}

}
