/*
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions{

	public static Vector3 Xyz(this Vector4 Input){
		return new Vector3 (Input.x, Input.y, Input.z);
	}

	public static Vector2 Xy(this Vector3 Input){
		return new Vector2 (Input.x, Input.y);
	}

	public static Vector2 Xz(this Vector3 Input){
		return new Vector2 (Input.x, Input.z);
	}
}
