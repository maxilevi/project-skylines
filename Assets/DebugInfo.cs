using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour {

	public Text DebugText;
	public World World;

	void Update () {
		DebugText.gameObject.SetActive (true);
		DebugText.text = "FPS = " + 1.0f / Time.deltaTime + Environment.NewLine
		+ "GQueue = " + World.GenQueue + Environment.NewLine
		+ "MQueue = " + World.MeshQueue + Environment.NewLine;
	}
}
