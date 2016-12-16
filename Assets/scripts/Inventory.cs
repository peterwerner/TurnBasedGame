using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Inventory : MonoBehaviour {

	public class Item : Actor {

		static bool isAnyInstanceBeingUsed = false;

		[SerializeField] string label;
		Inventory inventory;
		bool isBeingUsed = false;
		// UI
		CustomUI.Button button;

		public void PickUp (Inventory inventory) {
			if (this.Node) {
				this.Node.RemoveActor (this);
			}
			this.inventory = inventory;
			this.inventory.items.Add (this);
			gameObject.SetActive (false);
			// TODO this may need to change
			transform.parent = inventory.transform;
			transform.localPosition = Vector3.zero;
			OnPickUp ();
			// UI
			button = Instantiate (this.inventory.buttonPrefab, this.inventory.itemButtonParent);
			this.inventory.buttonListUI.Show (button);
			button.Show (Use, CancelUse, IsUseable);
			button.GetComponentInChildren<UnityEngine.UI.Text> ().text = label;
		}

		public void Use () {
			if (IsUseable()) {
				isBeingUsed = true;
				isAnyInstanceBeingUsed = isBeingUsed;
				OnUse ();
			}
		}

		public void CancelUse () {
			if (isBeingUsed) {
				isBeingUsed = false;
				isAnyInstanceBeingUsed = isBeingUsed;
				OnCancelUse ();
				button.Cancel ();
			}
		}

		public void DestroyButton () {
			if (button) {
				this.inventory.buttonListUI.Hide (button);
				button.Hide ();
				button = null;
			}
		}

		public void Destroy () {
			if (button) {
				DestroyButton ();
			}
			isBeingUsed = false;
			isAnyInstanceBeingUsed = isBeingUsed;
			inventory.items.Remove (this);
			inventory = null;
			enabled = false;
			GameObject.Destroy (gameObject);
		}
			
		public Actor Owner {
			get { return inventory.owner; }
		}

		public bool IsBeingUsed {
			get { return isBeingUsed; }
		}

		public bool IsUseable () {
			return !GameManager.IsInTurn && !isAnyInstanceBeingUsed;
		}

		public static bool IsAnyInstanceBeingUsed () {
			return isAnyInstanceBeingUsed;
		}

		protected virtual void OnUse () {}
		protected virtual void OnCancelUse () {}
		protected virtual void OnDestroy () {}
		protected virtual void OnPickUp () {}
	}

	List<Item> items = new List<Item> ();
	Actor owner;
	// UI
	[SerializeField] CustomUI.Button buttonPrefab;
	[SerializeField] Transform itemButtonParent;
	CustomUI.ButtonList buttonListUI = new CustomUI.ButtonList ();

	void Awake () {
		owner = GetComponent<Actor> ();
	}

}
