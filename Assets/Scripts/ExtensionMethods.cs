using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
	public static int ReMap(this int i, int fromLow, int fromHigh, int toLow, int toHigh)
	{
		return (i - fromLow) / (toLow - fromLow) * (toHigh - fromHigh) + toHigh;
	}

	public static float ReMap(this float f, float fromLow, float fromHigh, float toLow, float toHigh)
	{
		return (f - fromLow) / (toLow - fromLow) * (toHigh - fromHigh) + toHigh;
	}

	public static Component SearchHierarchy<T>(this Transform t, string component) where T : Component
	{
		Transform toReturn = t;
		bool componentFound = true;
		while (!toReturn.GetComponent(component))
		{
			if (toReturn.parent == null)
			{
				componentFound = false;
				break;
			}
			toReturn = toReturn.parent;
		}
		return componentFound ? toReturn.GetComponent(component) : null;
	}

	public static Vector3Int ToVector3Int(this Vector3 v)
	{
		return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
	}

	public static Vector3 ToVector3(this Vector3Int v)
	{
		return new Vector3(v.x, v.y, v.z);
	}
}