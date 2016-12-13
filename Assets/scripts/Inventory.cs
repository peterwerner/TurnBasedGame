using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Inventory : MonoBehaviour {

	public class Item : Actor {
		Inventory inventory;
		bool isBeingUsed = false;
		
		public void PickUp (Inventory inventory) {
			this.inventory = inventory;
			this.inventory.items.Add (this);
			gameObject.SetActive (false);
			// TODO this may need to change
			transform.parent = inventory.transform;
			transform.localPosition = Vector3.zero;
			OnPickUp ();
		}

		public void Use () {
			isBeingUsed = true;
			OnUse ();
		}

		public void CancelUse () {
			isBeingUsed = false;
			OnCancelUse ();
		}

		public void Destroy () {
			isBeingUsed = false;
			inventory.items.Remove (this);
			inventory = null;
			enabled = false;
			GameObject.Destroy (gameObject);
		}

		public void Drop () {
			throw new System.Exception ("not implemented!!!");
			/*
			this.inventory.items.Remove (this);
			this.inventory = null;
			this.gameObject.SetActive (true);
			OnDrop ();
			*/
		}

		public Actor Owner {
			get { return inventory.owner; }
		}

		public bool IsBeingUsed {
			get { return isBeingUsed; }
		}

		protected virtual void OnUse () { print ("use"); }
		protected virtual void OnCancelUse () { print ("cancel use"); }
		protected virtual void OnDestroy () { print ("destroy"); }
		protected virtual void OnPickUp () { print ("pickup"); }
		protected virtual void OnDrop () { print ("drop"); }
	}

	List<Item> items = new List<Item> ();
	Actor owner;

	void Awake () {
		owner = GetComponent<Actor> ();
	}
}
