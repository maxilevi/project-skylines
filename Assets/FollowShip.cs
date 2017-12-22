/*
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

using UnityEngine;
using System.Collections;

public class FollowShip : MonoBehaviour {

	public GameObject TargetShip;
	public Vector3 Offset;
	private Vector3 _lerpPosition;

	void LateUpdate () {
		if (TargetShip == null)
			return;
		_lerpPosition = Lerp (_lerpPosition, TargetShip.transform.position, Time.deltaTime * 32f);
		this.transform.position = Lerp (this.transform.position, TargetShip.transform.position + TargetShip.transform.forward * Offset.z + TargetShip.transform.up * Offset.y, Time.deltaTime * 8f);
		this.transform.LookAt( _lerpPosition, Vector3.up);
    }

	public Vector3 Lerp(Vector3 A, Vector3 B, float C){
		return new Vector3 (Mathf.Lerp(A.x, B.x, C), Mathf.Lerp(A.y, B.y, C), Mathf.Lerp(A.z, B.z, C)  );
	}
}
