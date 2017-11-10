using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	public float Speed = 16;
	public float TurnSpeed = 3f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.parent.position += transform.forward * Time.deltaTime * 4 * Speed;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 1.5f);

        if (Input.GetKey(KeyCode.W))
        {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.right * Time.deltaTime * 64f * TurnSpeed);
        }

        if (Input.GetKey(KeyCode.S))
        {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles - Vector3.right * Time.deltaTime * 64f * TurnSpeed);
        }

        if (Input.GetKey (KeyCode.A)) {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.forward * Time.deltaTime * 64f * TurnSpeed);
			transform.parent.Rotate(- Vector3.up * Time.deltaTime * 64f * TurnSpeed );
        }

		if (Input.GetKey (KeyCode.D)) {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles - Vector3.forward * Time.deltaTime * 64f * TurnSpeed);
			transform.parent.Rotate( Vector3.up * Time.deltaTime * 64f * TurnSpeed );

        }
	}
}
