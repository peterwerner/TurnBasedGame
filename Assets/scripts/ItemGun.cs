using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGun : Inventory.Item {

	[SerializeField] Projectile projectilePrefab;
	Actor target;

	protected override void OnUse () { 
		// TODO dont just auto shoot at the first target
		Dictionary<Vector3, List<Actor> > actorsInLOS = node.GetActorsInLOS();
		foreach (Vector3 direction in actorsInLOS.Keys) {
			foreach (Actor actorInLOS in actorsInLOS [direction]) {
				if (Owner.GetCharacter().IsEnemy(actorInLOS.GetCharacter())) {
					target = actorInLOS;
					Shoot();
					return;
				}
			}
		}
	}

	void Shoot () {
		Instantiate<Projectile> (projectilePrefab, transform.position, transform.rotation).Init (target.transform, OnHit);
	}

	void OnHit () {
		if (target.GetCharacter ()) {
			target.GetCharacter ().Alive = false;
		}
		Destroy ();
	}
}
