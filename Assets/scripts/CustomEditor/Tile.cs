using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : ListComponent<Tile> {

	public static readonly float size = 1f;

	[SerializeField] float emissionFadeRate = 1f;
	[SerializeField] float emissionMin = 0f, emissionMax = 1f;
	[SerializeField] new Renderer renderer;
	[SerializeField] Color emissionColor;
	TileConnector[] connectors;
	float time;
	bool isPulsing = false;

	void Awake () {
		connectors = GetComponentsInChildren<TileConnector> ();
	}

	void Update () {
		if (isPulsing) {
			float scale = Mathf.GammaToLinearSpace (Mathf.PingPong (time, emissionMax - emissionMin) - emissionMin);
			renderer.material.SetColor ("_EmissionColor", emissionColor * scale);
			time += emissionFadeRate * Time.deltaTime;
		}
	}
		
	public void Pulse (bool on) {
		isPulsing = on;
		if (!isPulsing) {
			renderer.material.SetColor ("_EmissionColor", emissionColor * 0);
		}
	}

	public bool IsVertical () {
		Vector3 upApprox = VectorUtil.ClosestCardinalDirection (transform.up);
		return upApprox != Vector3.up && upApprox != Vector3.down;
	}

	public TileConnector[] TileConnectors { get { return connectors; } }
}
