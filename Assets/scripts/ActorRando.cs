using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorRando : ActorMove {

	protected override void OnTurnStart () {
		Level.Node[] options = node.GetNeighbors();
		Level.Node randomNode = options[Random.Range(0, options.Length)];
		TryMoveTo (randomNode);
	}
		
}
