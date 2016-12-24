using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

	[SerializeField] float emissionFadeRate = 1f;
	[SerializeField] float emissionMin = 0f, emissionMax = 1f;
	[SerializeField] new Renderer renderer;
	[SerializeField] Color emissionColor;
	float time;

	void Update () {
		float scale = Mathf.GammaToLinearSpace (Mathf.PingPong (time, emissionMax - emissionMin) - emissionMin);
		renderer.material.SetColor ("_EmissionColor", emissionColor * scale);
		time += emissionFadeRate * Time.deltaTime;
	}

	void OnEnable () {
		time = 0;
	}

	void OnDisable () {
		renderer.material.SetColor ("_EmissionColor", emissionColor * 0);
	}
}
