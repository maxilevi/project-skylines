/*
 * Written by Maxi Levi <maxilevi@live.com>, November 2017
 */


using UnityEngine;
using System.Collections;
using Assets;
using Assets.Generation;

public class Movement : MonoBehaviour {

	public float Speed = 16;
	public float TurnSpeed = 3f;
	public TrailRenderer LeftTrail, RightTrail;
	public Color TrailColor;
	public Vector3 LeftPosition, RightPosition;
	public Material TrailMaterial;
	public AudioSource LeftSource, RightSource;
	public AudioClip SwooshClip;
	private GameObject Debris;
	private bool _lock;
	private float _leftTargetVolume = 1, _rightTargetVolume = 1;
	private float _originalVolume;
	private float _speed = 0;

	public bool IsInSpawn{
		get{ return (transform.parent.position - WorldGenerator.SpawnPosition).sqrMagnitude < WorldGenerator.SpawnRadius * WorldGenerator.SpawnRadius; }
	}

	void Start(){
		Debris = GameObject.FindGameObjectWithTag ("Debris");
		LeftSource = GameObject.FindGameObjectWithTag ("LeftSource").GetComponent<AudioSource>();
		RightSource = GameObject.FindGameObjectWithTag ("RightSource").GetComponent<AudioSource>();
		_originalVolume = (RightSource.volume + LeftSource.volume) * .5f;
	}

	public void Lock(){
		_lock = true;
	}

	public void Unlock(){
		_lock = false;
	}

	// Update is called once per frame
	void Update () {

		LeftSource.volume = Mathf.Lerp (LeftSource.volume, _originalVolume * _leftTargetVolume, Time.deltaTime * 2f);
		RightSource.volume = Mathf.Lerp (RightSource.volume, _originalVolume * _rightTargetVolume, Time.deltaTime * 2f);

		if (LeftSource.volume < 0.05f)
			LeftSource.Stop ();

		if (RightSource.volume < 0.05f)
			RightSource.Stop ();

		if (_lock)
			return;

		_speed = Mathf.Lerp (_speed, Speed, Time.deltaTime * .25f);
		transform.parent.position += transform.forward * Time.deltaTime * 4 * _speed;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * 2.5f);

		float zAngle = transform.localRotation.eulerAngles.z;
		float xAngle = transform.localRotation.eulerAngles.x;
		if (zAngle > 45 && zAngle < 135 || zAngle > 225 && zAngle < 315 || xAngle > 45 && xAngle < 90 || xAngle > 270 && xAngle < 315) {
			StartTrail (ref LeftTrail, LeftPosition);
			LeftSource.transform.position = LeftPosition;
			LeftSource.clip = SwooshClip;
			_leftTargetVolume = 1;
			if(!LeftSource.isPlaying)
				LeftSource.Play ();
		} else {
			StopTrail (ref LeftTrail);
			_leftTargetVolume = 0;
		}
		
		if (zAngle > 45 && zAngle < 135 || zAngle > 225 && zAngle < 315 || xAngle > 45 && xAngle < 90 || xAngle > 270 && xAngle < 315) {
			StartTrail (ref RightTrail, RightPosition);
			RightSource.transform.position = RightPosition;
			RightSource.clip = SwooshClip;
			_rightTargetVolume = 1;
			if(!RightSource.isPlaying)
				RightSource.Play ();
		} else {
			StopTrail (ref RightTrail);
			_rightTargetVolume = 0;
		}
		
		float scale = (Time.timeScale != 1) ? (1 / Time.timeScale) * .5f : 1;
		float hAxis = Input.GetAxisRaw("Horizontal");
		float vAxis = Input.GetAxisRaw("Vertical");

		if(Options.Invert)
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.right * Time.deltaTime * 64f * TurnSpeed * scale * vAxis);
		else
			transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + Vector3.right * Time.deltaTime * 64f * TurnSpeed * scale * -vAxis);


		transform.localRotation = Quaternion.Euler (transform.localRotation.eulerAngles + Vector3.forward * Time.deltaTime * 64f * TurnSpeed * scale * -hAxis);
		transform.parent.Rotate (-Vector3.up * Time.deltaTime * 64f * TurnSpeed * scale * -hAxis);

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
		Trail.time = 1.5f;
	}

	void StopTrail(ref TrailRenderer Trail){
		if (Trail == null)
			return;

		Trail.transform.parent = (Debris != null) ?  Debris.transform : null;
		Destroy (Trail.gameObject, Trail.time+1);
		Trail = null;
	}
}
