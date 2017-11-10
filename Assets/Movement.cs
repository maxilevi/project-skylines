using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	public float Speed = 16;
	public float TurnSpeed = 3f;
	public TrailRenderer LeftTrail, RightTrail;
	public Color TrailColor;
	public Vector3 LeftPosition, RightPosition;
	public Material TrailMaterial;
	private bool _lock;

	public void Lock(){
		_lock = true;
	}

	public void Unlock(){
		_lock = false;
	}

	// Update is called once per frame
	void Update () {
		if (_lock)
			return;
		
		transform.parent.position += transform.forward * Time.deltaTime * 4 * Speed;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 1.5f);

		float zAngle = transform.localRotation.eulerAngles.z;
		float xAngle = transform.localRotation.eulerAngles.x;
		if (zAngle > 45 && zAngle < 135 || zAngle > 225 && zAngle < 315 || xAngle > 45 && xAngle < 90 || xAngle > 270 && xAngle < 315 )
			StartTrail (ref LeftTrail, LeftPosition);
		else
			StopTrail (ref LeftTrail);
		
		if(zAngle > 45 && zAngle < 135 || zAngle > 225 && zAngle < 315 || xAngle > 45 && xAngle < 90 || xAngle > 270 && xAngle < 315 )
			StartTrail (ref RightTrail, RightPosition);
		else
			StopTrail (ref RightTrail);
		
		float scale = (Time.timeScale != 1) ? (1 / Time.timeScale) * .5f : 1;

        if (Input.GetKey(KeyCode.W))
        {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.right * Time.deltaTime * 64f * TurnSpeed * scale);
        }

        if (Input.GetKey(KeyCode.S))
        {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles - Vector3.right * Time.deltaTime * 64f * TurnSpeed * scale);
        }

        if (Input.GetKey (KeyCode.A)) {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.forward * Time.deltaTime * 64f * TurnSpeed * scale);
			transform.parent.Rotate(- Vector3.up * Time.deltaTime * 64f * TurnSpeed * (1/Time.timeScale));
        }

		if (Input.GetKey (KeyCode.D)) {
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles - Vector3.forward * Time.deltaTime * 64f * TurnSpeed * scale);
			transform.parent.Rotate( Vector3.up * Time.deltaTime * 64f * TurnSpeed * scale );

        }
	}

	void StartTrail(ref TrailRenderer Trail, Vector3 Position){
		if (Trail != null)
			return;
		GameObject go = new GameObject ("Trail");
		go.transform.parent = this.gameObject.transform;
		Trail = go.AddComponent<TrailRenderer> ();
		Trail.widthMultiplier = .25f;
		Trail.endColor = new Color (0, 0, 0, 0);
		Trail.startColor = TrailColor;
		Trail.transform.localPosition = Position;
		Trail.material = TrailMaterial;
	}

	void StopTrail(ref TrailRenderer Trail){
		if (Trail == null)
			return;
		Trail.transform.parent = null;
		Destroy (Trail.gameObject, Trail.time+1);
		Trail = null;
	}
}
