using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

	public enum Factions { NONE, PLAYER, BADGUY };

	[SerializeField] Factions faction;
	[SerializeField] bool alive = true;

	public bool Alive {
		get {
			return alive;
		}
		set {
			alive = value;
		}
	}

	public Factions Faction {
		get { return faction; }
	}

	public bool IsEnemy (Character other) {
		if (!other) {
			return false;
		} else if (other.Faction == Factions.PLAYER) {
			return Faction == Factions.BADGUY;
		} else if (other.Faction == Factions.BADGUY) {
			return Faction == Factions.PLAYER;
		}
		return false;
	}

}
