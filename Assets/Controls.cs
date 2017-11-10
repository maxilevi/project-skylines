
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour {

	public float Speed = 8f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.W))
			transform.Rotate(transform.right, Time.deltaTime * 64f);

		if(Input.GetKey(KeyCode.S))
			transform.Rotate(-transform.right, Time.deltaTime * 64f);

		if(Input.GetKey(KeyCode.D))
			transform.Rotate(transform.forward, Time.deltaTime * 64f);

		if(Input.GetKey(KeyCode.A))
			transform.Rotate(-transform.forward, Time.deltaTime * 64f);

		transform.position += Time.deltaTime * transform.forward * Speed;
	}

	void OnCollisionEnter(){
		Debug.Log ("Explosion");
	}
}
