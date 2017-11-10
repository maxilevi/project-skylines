using UnityEngine;
using System.Collections;

public class FollowShip : MonoBehaviour {

	public GameObject TargetShip;
	public Vector3 Offset;

	void Update () {
		this.transform.position = Lerp (this.transform.position, TargetShip.transform.position + TargetShip.transform.forward * Offset.z + TargetShip.transform.up * Offset.y, Time.deltaTime * 16f);
		this.transform.LookAt(TargetShip.transform.position, Vector3.up);
        //this.transform.rotation = Quaternion.Slerp(this.transform.rotation, TargetShip.transform.rotation, Time.deltaTime * 32f);
    }

	public Vector3 Lerp(Vector3 A, Vector3 B, float C){
		return new Vector3 (Mathf.Lerp(A.x, B.x, C), Mathf.Lerp(A.y, B.y, C), Mathf.Lerp(A.z, B.z, C)  );
	}
}
