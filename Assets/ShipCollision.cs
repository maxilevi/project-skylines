using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCollision : MonoBehaviour {

	public TimeControl Control;
	public GameObject Model;
	public Movement Controls;
	private bool _lock = false;

	public void Reset(){
		_lock = false;
	}

	void OnCollisionEnter(Collision col){
		if (_lock)
			return;

		Controls.Lock ();
		Control.Lose ();
		Model.AddComponent<Explode> ();
		_lock = true;
	}
}
