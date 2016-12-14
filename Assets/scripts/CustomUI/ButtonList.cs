using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUI {

	public class ButtonList {

		List<Button> buttons = new List<Button> ();
		float padding = 0.1f;

		public void Show (Button button) {
			if (!buttons.Contains (button)) {
				float offset = buttons.Count + padding * (buttons.Count + 1);
				button.SetPosition (new Vector3 (padding * -1, offset, 0), true);
				buttons.Add (button);
			}
		}

		public void Hide (Button button) {
			int index = buttons.FindIndex (o => o == button);
			if (index >= 0) {
				buttons.RemoveAt (index);
				for (int i = index; i < buttons.Count; i++) {
					float offset = i + padding * (i + 1);
					buttons [i].SetPosition (new Vector3 (padding * -1, offset, 0));
				}
			}
		}
	}

}