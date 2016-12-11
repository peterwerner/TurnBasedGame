using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonComponent<GameManager> {

	[SerializeField] int nodeLayer = 4;
	float prevTurnStartTime = Mathf.NegativeInfinity;
	bool isInTurn = false;

	public static bool StartTurn() {
		if (Instance.isInTurn) {
			return false;
		}
		Instance.isInTurn = true;
		Instance.prevTurnStartTime = Time.time;
		Actor.StartTurns ();
		return true;
	}

	public static bool IsInTurn () {
		return Instance.isInTurn;
	}

	void Start () {
		foreach (Level.Node node in Level.Node.InstanceList) {
			node.gameObject.layer = nodeLayer;
		}
		Level.NodeAStar.Init ();
	}

	void Update () {
		if (isInTurn && Actor.AllActorsFinished()) {
			isInTurn = false;
			Actor.EndTurns ();
		}
	}

}
