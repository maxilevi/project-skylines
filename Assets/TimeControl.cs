/*
 *Written by Maxi Levi <maxilevi@live.com>, November 2017
*/

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using Assets.Generation;
using Assets;

public class TimeControl : MonoBehaviour {

	public RectTransform TimeBar;
	public float EnergyLeft = 100;
	public float EnergyUsage = 8;
	public bool Using;
	private bool WasPressed;
	public Camera View;
	public Text Score, ScoreCenter;
	private float _score;
	public bool Lost = true;//To simulate the start menu
	public Text GameOver;
	public RawImage Title;
	public Text RestartBtn, StartBtn;
	public GameObject PlayerPrefab;
	public AudioSource Sound;
	public Text InvertTxt;
	public Image InvertCheck;
	public Toggle Invert;
	private Movement _movement;
	private float _targetGameOver;
	private float _targetRestart;
	private float _targetScore;
	private float _targetStart;
	private float _targetTitle;
	private float _targetPitch = 1;
	private float _targetInvert;

	void Start(){
		Lost = true;
		Time.timeScale = .25f;
		_targetStart = 1;
		_targetTitle = 1;
		StartCoroutine (PlayAnim());
	}

	public void Lose(){
		Lost = true;
		Time.timeScale = .25f;
		_targetGameOver = 1f;
		_targetScore = 1f;
		StartCoroutine (LostCoroutine());
	}

	IEnumerator LostCoroutine (){
		yield return new WaitForSeconds (3 / (1/Time.timeScale) );
		_targetGameOver = 0;
		while (Lost) {
			yield return new WaitForSeconds (1 / (1/Time.timeScale) );
			_targetRestart = 1;
			yield return new WaitForSeconds (1 / (1/Time.timeScale) );
			_targetRestart = 0;

		}
	}

	IEnumerator PlayAnim (){
		while (Lost) {
			yield return new WaitForSeconds (.5f / (1/Time.timeScale) );
			_targetStart = 1;
			yield return new WaitForSeconds (.5f / (1/Time.timeScale) );
			_targetStart = 0;

		}
	}

	public void StartGame(){
		Restart ();
	}

	public void Restart(){
		if (!Lost)
			return;
		StartCoroutine (RestartCoroutine());
	}

	IEnumerator RestartCoroutine(){
		_targetStart = 0;
		_targetRestart = 0;
		_targetTitle = 0;
		Lost = false;
		Time.timeScale = 1f;
		_score = 0;
		_targetScore = 0;
		EnergyLeft = 100;
		Destroy (GameObject.FindGameObjectWithTag("Player"));
		Destroy (GameObject.FindGameObjectWithTag("Debris"));

		GameObject Debris = new GameObject ("Debris");
		Debris.tag = "Debris";
		OpenSimplexNoise.Load (Random.Range(int.MinValue, int.MaxValue));

		World world = GameObject.FindGameObjectWithTag ("World").GetComponent<World>();
		Chunk[] chunks = null;
		lock(world.Chunks) 
			chunks =  world.Chunks.Values.ToList().ToArray();
		
		for (int i = 0; i < chunks.Length; i++)
			world.RemoveChunk (chunks[i]);
	
		GameObject go = Instantiate<GameObject>(PlayerPrefab, Vector3.zero, Quaternion.identity);
		world.Player = go;
		_movement = go.GetComponentInChildren<Movement> ();
		go.GetComponent<ShipCollision> ().Control = this.GetComponent<TimeControl> ();
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FollowShip>().TargetShip = go;

		yield return null;
	}

	void Update(){

		#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.F2))
			ScreenCapture.CaptureScreenshot("C:/Users/maxi/Desktop/Neon/"+System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")+".png");
		#endif

		if ( Lost && Input.GetKeyDown(KeyCode.Space))//Input.anyKeyDown)
			Restart();

		Score.text = ((int) _score).ToString();
		ScoreCenter.text = Score.text;

		_targetInvert = Mathf.Min (1, _targetScore + _targetTitle);
		Sound.pitch = Mathf.Lerp (Sound.pitch, _targetPitch, Time.deltaTime * 8f);
		Title.color = new Color(Title.color.r, Title.color.g, Title.color.b, Mathf.Lerp (Title.color.a, _targetTitle, Time.deltaTime * 4f * (1/Time.timeScale)));
		StartBtn.color = new Color(StartBtn.color.r, StartBtn.color.g, StartBtn.color.b, Mathf.Lerp (StartBtn.color.a, _targetStart, Time.deltaTime * 4f * (1/Time.timeScale)));
		GameOver.color = new Color(GameOver.color.r, GameOver.color.g, GameOver.color.b, Mathf.Lerp (GameOver.color.a, _targetGameOver, Time.deltaTime * 4f * (1/Time.timeScale)));
		RestartBtn.color = new Color(RestartBtn.color.r, RestartBtn.color.g, RestartBtn.color.b, Mathf.Lerp (RestartBtn.color.a, _targetRestart, Time.deltaTime * 4f * (1/Time.timeScale)));
		Invert.targetGraphic.color = new Color (Invert.targetGraphic.color.r, Invert.targetGraphic.color.g, Invert.targetGraphic.color.b, Mathf.Lerp(Invert.targetGraphic.color.a, _targetInvert, Time.deltaTime * 4f * (1/Time.timeScale)));
		InvertTxt.color = new Color(InvertTxt.color.r, InvertTxt.color.g, InvertTxt.color.b, Mathf.Lerp (InvertTxt.color.a, _targetInvert, Time.deltaTime * 4f * (1/Time.timeScale)));
		InvertCheck.color = new Color(InvertCheck.color.r, InvertCheck.color.g, InvertCheck.color.b, Mathf.Lerp (InvertCheck.color.a, _targetInvert, Time.deltaTime * 4f * (1/Time.timeScale)));
		if (_targetTitle != 1) {
			Score.color = new Color (Score.color.r, Score.color.g, Score.color.b, Mathf.Lerp (Score.color.a, 1 - _targetScore, Time.deltaTime * 2f * (1 / Time.timeScale)));
			ScoreCenter.color = new Color (ScoreCenter.color.r, ScoreCenter.color.g, ScoreCenter.color.b, Mathf.Lerp (ScoreCenter.color.a, _targetScore, Time.deltaTime * 2f * (1 / Time.timeScale)));
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
			_targetPitch = .5f;
			View.GetComponent<MotionBlur>().enabled = true;
			View.GetComponent<VignetteAndChromaticAberration>().enabled = false;
		} else {
			Time.timeScale = 1f;
			_targetPitch = 1f;
			View.GetComponent<MotionBlur>().enabled = false;
			View.GetComponent<VignetteAndChromaticAberration>().enabled = true;
		}

		if(!Using)
			WasPressed = Input.GetKey(KeyCode.Space);

		if (!_movement.IsInSpawn)
			_score += Time.deltaTime * 8;
	
		if (_score < 125)
			_movement.Speed = 12;
		else if (_score < 275)
			_movement.Speed = 14;
		else if(_score < 500)
			_movement.Speed = 16;
		else if(_score < 1000)
			_movement.Speed = 18;
			
	}

	public void InvertControls(){
		Options.Invert = !Options.Invert;
		Invert.isOn = Options.Invert;
	}

	Vector2 Lerp(Vector2 a, Vector2 b, float d){
		return new Vector2 ( Mathf.Lerp(a.x,b.x,d), Mathf.Lerp(b.x,b.y,d) );
	}
}
