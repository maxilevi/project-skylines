using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {

	public Material Terrain;
	public Color[] PossibleColors;
	public Color CurrentColor;
	public float MaxThickness = 0.3f, MinThickness = 0.15f;

	void Update(){
		Terrain.SetColor("_Color", Lerp(Terrain.GetColor("_Color"), CurrentColor, Time.deltaTime * 2f));
		Terrain.SetColor("_GColor", Terrain.GetColor("_Color"));
		Terrain.SetColor("_WColor", Terrain.GetColor("_Color"));
		Terrain.SetColor("_EmissionColor", Terrain.GetColor("_Color"));
	}

	Color Lerp(Color c1, Color c2, float delta){
		return new Color (Mathf.Lerp (c1.r, c2.r, delta), Mathf.Lerp (c1.g, c2.g, delta), Mathf.Lerp (c1.b, c2.b, delta), Mathf.Lerp (c1.a, c2.a, delta));
	}
			
}
