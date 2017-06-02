using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MenuPlayer : MonoBehaviour {

	public string[] animations;
	public GameObject playButton;
	public GameObject statsButton;
	public GameObject quitButton;
	public Text highScoreText;
	public Text lowScoreText;
	public GameObject darkPanel;
	public GameObject optionsPanel;
	public GameObject statsPanel;
	public Toggle fullscreenToggle;
	public Dropdown resDropdown;

	private int XRes;
	private int YRes;
	private bool isFullscreen;

	private bool runningRandom;
	private Rigidbody2D rb2D;
	private Animator animator;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1;
		rb2D = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
		runningRandom = true;
		StartCoroutine (RandomAnimations());
	}
	
	IEnumerator RandomAnimations () {
		yield return new WaitForSeconds (5f);
		while (runningRandom) {
			if(Mathf.RoundToInt(Random.Range(0f,1f)) == 1) {
				animator.SetTrigger(animations[Random.Range(0,animations.Length)]);
			}
			yield return new WaitForSeconds (2f);
		}
	}

	public void BeginGame () {
		runningRandom = false;
		animator.SetBool ("walking", true);
		StartCoroutine (SmoothMovement (transform.position + new Vector3 (0f, 4f, 0f)));
		Invoke ("LoadGame", 1f);
	}

	protected IEnumerator SmoothMovement (Vector3 end) {
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		while (sqrRemainingDistance > float.Epsilon) {
			Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, 5 * Time.deltaTime);
			rb2D.MovePosition(newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
	}

	public void ShowStats () {
		Time.timeScale = 0;
		PlayerData data = Load ();
		darkPanel.SetActive (true);
		statsPanel.SetActive (true);
		optionsPanel.SetActive (false);
		if(data == null) {
			highScoreText.text = "High Score:<color=#9FFFB3FF>-</color>";
			lowScoreText.text = "Low Score:<color=#FFACACFF>-</color>";
			return;
		}
		highScoreText.text = "High Score:<color=#9FFFB3FF>" + data.highScore + "</color>";
		lowScoreText.text = "Low Score:<color=#FFACACFF>" + data.lowScore + "</color>";
	}

	public void HideStats () {
		Time.timeScale = 1;
		darkPanel.SetActive(false);
	}

	/*public void ShowOptions () {
		Debug.LogError (Screen.ToString());
		darkPanel.SetActive(true);
		statsPanel.SetActive (false);
		optionsPanel.SetActive (true);
		fullscreenToggle.isOn = isFullscreen;

		Resolution[] resi = Screen.resolutions;

		List<string> dropdownOptions = new List<string> ();

		resDropdown.ClearOptions ();

		int currentResVal = 0;
		int counter = 0;

		foreach (Resolution resolution in resi) {
			dropdownOptions.Add (resolution.ToString());
			if (resolution.ToString () == Screen.currentResolution.ToString ()) {
				currentResVal = counter;
			}
			counter ++;
		}

		resDropdown.AddOptions (dropdownOptions);
		resDropdown.value = currentResVal;
	}

	public void HideOptions (bool saving) {
		Time.timeScale = 1;
		if (saving) {
			string resolution = resDropdown.captionText.text;
			string[] splitRes = resolution.Split (new string[] { " " }, StringSplitOptions.None);
			foreach (string s in splitRes) {
				Debug.Log (s);
			}
			isFullscreen = fullscreenToggle.isOn;
			int.TryParse (splitRes [0], out XRes);
			int.TryParse (splitRes [2], out YRes);
			Screen.SetResolution (XRes, YRes, isFullscreen);
		}
		darkPanel.SetActive (false);
	}*/

	void LoadGame () {
		SceneManager.LoadScene ("Main");
	}

	PlayerData Load () {
		if (File.Exists (Application.persistentDataPath + "/highScore.dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/highScore.dat", FileMode.Open);

			PlayerData data = (PlayerData)bf.Deserialize (file);
			file.Close ();

			return data;
		}
		return null;
	}
}
