using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public delegate void OnHitFunction();

	[SerializeField] float speed = 20;
	Transform target;
	OnHitFunction onHit;

	public void Init (Transform target, OnHitFunction onHit) {
		this.target = target;
		this.onHit = onHit;
		this.enabled = true;
	}

	void Update () {
		float moveDelta = speed * Time.deltaTime;
		if (Vector3.Distance (transform.position, target.position) < moveDelta) {
			onHit ();
			enabled = false;
			gameObject.SetActive (false);
			Destroy (gameObject);
		}
		transform.position = Vector3.MoveTowards (transform.position, target.position, moveDelta);
		transform.LookAt (target.position);
	}

}
