using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonUtil {
	/// <summary>
	/// Check if the given point is within the bound
	/// </summary>
	/// <param name="checkPoint">World position of the point we want to check</param>
	/// <param name="bound">The bound that is checked against. Also in world frame</param>
	/// <returns></returns>
	static public bool IsPosInsideBound2D(Vector3 checkPoint, Bounds bound) {
		if (checkPoint.x >= bound.min.x && checkPoint.x < bound.max.x &&
			checkPoint.y >= bound.min.y && checkPoint.y < bound.max.y) {
			return true;
		}
		return false;
	}
}
