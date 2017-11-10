using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class TimeControl : MonoBehaviour {

	public RectTransform TimeBar;
	public float EnergyLeft = 100;
	public float EnergyUsage = 8;
	public bool Using;
	private bool WasPressed;
	public Camera View;
	public Text Score;
	private float _score;
	public bool Lost = false;
	public RectTransform GameOver;
	private Vector2 _targetGameOver;

	public void Lose(){
		Lost = true;
		Time.timeScale = .25f;
		_targetGameOver = Vector2.one * .5f;
	}

	void Update(){
		if (Lost) {
			GameOver.localScale = Lerp (GameOver.localScale, _targetGameOver, Time.deltaTime * 4f * (1 / Time.timeScale));
		}
		if (Lost)
			return;

		if(Input.GetKey(KeyCode.Space) && EnergyLeft > 0 && !WasPressed){
			EnergyLeft -= Time.deltaTime * EnergyUsage * (1/Time.timeScale);
			EnergyLeft = Mathf.Clamp (EnergyLeft, 0, 100);
			Using = true;
		}else{
			EnergyLeft += Time.deltaTime * EnergyUsage * .5f;
			EnergyLeft = Mathf.Clamp (EnergyLeft, 0, 100);
			Using = false;
		}
		TimeBar.sizeDelta = Lerp(TimeBar.sizeDelta, new Vector2 (EnergyLeft-.5f, TimeBar.sizeDelta.y), Time.deltaTime * 6f);

		if (Using) {
			Time.timeScale = .35f;
			View.GetComponent<MotionBlur>().enabled = true;
			View.GetComponent<VignetteAndChromaticAberration>().enabled = false;
		} else {
			Time.timeScale = 1f;
			View.GetComponent<MotionBlur>().enabled = false;
			View.GetComponent<VignetteAndChromaticAberration>().enabled = true;
		}

		if(!Using)
			WasPressed = Input.GetKey(KeyCode.Space);

		_score += Time.deltaTime * 8;
		Score.text = ((int) _score).ToString();
	}

	Vector2 Lerp(Vector2 a, Vector2 b, float d){
		return new Vector2 ( Mathf.Lerp(a.x,b.x,d), Mathf.Lerp(b.x,b.y,d) );
	}
}
