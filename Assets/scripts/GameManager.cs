using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManager : SingletonComponent<GameManager> {

	[CustomEditor(typeof(GameManager))]
	public class GameManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			GameManager myScript = (GameManager)target;
			if (GUILayout.Button ("Force Refresh Node Relationships")) {
				myScript.Setup ();
				Level.Node.InitAll();
			}
		}
	}

	[SerializeField] int nodeLayer = 4;
	[SerializeField] LayerMask nodeLineOfSightLayers;
	bool isInTurn = false;

	public static bool StartTurn() {
		if (Instance.isInTurn) {
			return false;
		}
		Instance.isInTurn = true;
		Actor.StartTurns ();
		Actor.HandleInteractions ();
		return true;
	}

	public static bool IsInTurn {
		get { return Instance.isInTurn; }
	}

	public static bool PlayerCanMove {
		get { return !IsInTurn && !Inventory.Item.IsAnyInstanceBeingUsed(); }
	}

	public static LayerMask GetNodeLOSLayers () {
		return Instance.nodeLineOfSightLayers;
	}

	public static int GetNodeLayer () {
		return Instance.nodeLayer;
	}

	void Start () {
		foreach (Level.Node node in Level.Node.InstanceList) {
			node.gameObject.layer = nodeLayer;
		}
		Level.NodeAStar.Init ();
		Level.Node.InitAll ();
	}

	void Update () {
		if (isInTurn && Actor.AllActorsFinished()) {
			isInTurn = false;
			Actor.EndTurns ();
		}
	}

}
