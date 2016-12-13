using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Inventory : MonoBehaviour {

	public class Item : Actor {
		Inventory inventory;
		
		public void PickUp (Inventory inventory) {
			this.inventory = inventory;
			this.inventory.items.Add (this);
			this.gameObject.SetActive (false);
			OnPickUp ();
		}

		public void Use () {
			this.inventory.items.Remove (this);
			this.inventory = null;
			OnUse ();
		}

		public void Drop () {
			throw new System.Exception ("not implemented!!!");
			this.inventory.items.Remove (this);
			this.inventory = null;
			this.gameObject.SetActive (true);
			OnDrop ();
		}

		protected virtual void OnUse () { print ("use"); }
		protected virtual void OnDrop () { print ("drop"); }
		protected virtual void OnPickUp () { print ("pickup"); }
	}

	Actor owner;
	List<Item> items = new List<Item> ();

	void Awake () {
		owner = GetComponent<Actor> ();
	}
}
