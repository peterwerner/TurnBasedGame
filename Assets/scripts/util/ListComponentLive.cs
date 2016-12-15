using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public abstract class ListComponentLive<T> : MonoBehaviour where T : MonoBehaviour
{
	public static List<T> InstanceList = new List<T>();

	protected virtual void OnEnable()
	{
		InstanceList.Add( this as T );
	}

	protected virtual void OnDisable()
	{
		InstanceList.Remove( this as T );
	}

	public static void DestroyAll()
	{
		T[] ListClone = new T[InstanceList.Count];
		InstanceList.CopyTo(ListClone);
		foreach (T instance in ListClone)
			Destroy(instance.gameObject);
	}

	public static T ClosestTo(Vector3 pos)
	{
		T closest = null;
		float closestDistSqrd = Mathf.Infinity;
		foreach (T instance in InstanceList) {
			float distSqrd = Vector3.SqrMagnitude (pos - instance.transform.position);
			if (distSqrd < closestDistSqrd) {
				closestDistSqrd = distSqrd;
				closest = instance;
			}
		}
		return closest;
	}

}