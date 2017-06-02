using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

	public AudioClip endMusic;

	public float levelStartDelay;
	public float turnDelay;
	public BoardManager boardScript;
	public static GameManager instance = null;
	public int playerHealth = 10;
	[HideInInspector] public List<int> playerInventory = new List<int>();
	[HideInInspector] public List<int> playerPowerups = new List<int>();
	[HideInInspector] public bool playersTurn = true;

	public string[] maleNames;
	public string[] femaleNames;
	[TextArea(1,5)]
	public string[] NPCSentences;

	private int level = 1;
	private List<Enemy> enemies;
	private bool enemiesMoving = false;
	[HideInInspector] public bool settingUp = false;
	[HideInInspector] public int enemiesFinishedMoving;
	private PlayerData lastSave;

	private List<int> talkedNpcs;
	private List<int> readSigns;

	private Text levelText;
	private GameObject levelImage;
	private Text deathText;
	private GameObject deathImage;
	private Text titleText;
	private Text descText;
	private GameObject dialoguePanel;
	private GameObject menuPanel;
	private Text endScoreText;
	private Text highScoreText;
	private Button pauseButton;

	// Use this for initialization
	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
		DontDestroyOnLoad (gameObject);
		enemies = new List<Enemy> ();
		talkedNpcs = new List<int> ();
		readSigns = new List<int> ();
		InitInventory ();
		boardScript = GetComponent<BoardManager> ();
		InitGame ();
		lastSave = Load ();
	}

	void OnEnable () {}

	private void OnLevelWasLoaded (int index) {
		if(level != 1)
			InitGame ();
	}

	void InitInventory () {
		playerInventory.Add (0);
		playerInventory.Add (0); // red
		playerInventory.Add (0); // green
		playerInventory.Add (0); // blue
		playerInventory.Add (0); // yellow
		playerPowerups.Add (0);
		playerPowerups.Add (0); // sword
		playerPowerups.Add (0); // mask
		playerPowerups.Add (0); // spike box
	}

	void Update () {
		if (playersTurn || settingUp)
			return;
		if (enemiesMoving) {
			Debug.Log ("Player has moved before enemies have finished moving!");
			playersTurn = true;
		}
		StartCoroutine (MoveEnemies ());
	}
	IEnumerator MoveEnemies () {
		enemiesFinishedMoving = 0;
		enemiesMoving = true;
		//yield return new WaitForSeconds (turnDelay);
		
		for (int i = 0; i < enemies.Count; i++) {
			if (GameObject.FindGameObjectWithTag ("Player").GetComponent<Player> ().wearingMask == 0) {
				enemies [i].MoveEnemy ();
			} else {
				enemiesFinishedMoving++;
			}
			//yield return new WaitForSeconds(enemies[i].moveTime);
		}

		//yield return new WaitForSeconds (turnDelay);

		playersTurn = true;

		while (enemiesFinishedMoving != enemies.Count)
			yield return null;
		enemiesMoving = false;
	}

	// Update is called once per frame
	void InitGame () {
		foreach (RectTransform kiddy in GameObject.Find("Canvas").GetComponentInChildren<RectTransform>(true)) {
			kiddy.gameObject.SetActive (true);
		}

		settingUp = true;
		levelText = GameObject.Find ("Level Text").GetComponent<Text>();
		levelImage = GameObject.Find ("Level Image");
		deathText = GameObject.Find ("Death Text").GetComponent<Text>();
		deathImage = GameObject.Find ("Death Image");
		titleText = GameObject.Find ("Title Text").GetComponent<Text>();
		descText = GameObject.Find ("Description Text").GetComponent<Text>();
		endScoreText = GameObject.Find ("End Score Text").GetComponent<Text> ();
		highScoreText = GameObject.Find ("High Score Text").GetComponent<Text> ();
		dialoguePanel = GameObject.Find ("Dialogue Panel");
		menuPanel = GameObject.Find ("Pause Menu");
		menuPanel.SetActive (false);
		deathImage.SetActive (false);
		dialoguePanel.SetActive (false);
		levelText.text = "World " + level;
		levelImage.SetActive (true);
		enemies.Clear ();
		boardScript.SetupScene (level);
		Invoke ("EndSetup", levelStartDelay);
	}

	void EndSetup () {
		levelImage.SetActive (false);
		GameObject.FindGameObjectWithTag ("Player").GetComponent<Player> ().MoveIntoBoard ();
	}

	public void AddEnemiesToList (Enemy en) {
		enemies.Add (en);
	}

	public void NextLevel () {
		level++;
	}

	public void GameOver () {
		settingUp = true;
		Invoke ("ShowDeathText", 0.5f);
	}

	int CalcScores () {
		playerInventory = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>().inventory;

		int score = 0;

		score += level * 100;
		AddToEndScore (playerInventory [1], "inventory_contents", 20);
		AddToEndScore (playerInventory [2], "inventory_contents", 50);
		AddToEndScore (playerInventory [3], "inventory_contents", 100);
		AddToEndScore (playerInventory [4], "inventory_contents", 200);
		score += talkedNpcs.Count * 10;
		score += readSigns.Count * 5;

		return score;
	}

	public void AddToEndScore (int num, string type) {
		if (type == "npc_talk") {
			if (!talkedNpcs.Contains(num)) {
				talkedNpcs.Add (num);
			}
		} else if (type == "sign_read") {
			if (!readSigns.Contains (num)) {
				readSigns.Add (num);
			}
		}
	}

	int AddToEndScore (int num, string type, int spec) {
		if (type == "inventory_contents") {
			return num * spec;
		}
		return 0;
	}

	void ShowDeathText () {
		int tempScore = CalcScores ();
		deathText.text = "You perished in world " + level + "...";
		endScoreText.text = "Score: " + tempScore;
		if (lastSave != null) {
			highScoreText.text = "High score: " + lastSave.highScore;
			if (tempScore > lastSave.highScore) {
				highScoreText.text = "You set a new high score!";
				Save (tempScore, lastSave.lowScore, "None");
			} else if (tempScore < lastSave.lowScore || lastSave.lowScore == 0) {
				highScoreText.text = "You set a record low score!";
				Save (lastSave.highScore, tempScore, "None");
			}
		} else {
			highScoreText.text = "You set a new high score!";
			Save (tempScore, tempScore, "None");
		}
		deathImage.SetActive (true);
		enabled = false;
		SoundManager.instance.PlaySingle (endMusic,false);
		SoundManager.instance.musicAudio.Stop ();
	}

	public void ShowDialogue (string title, string description) {
		titleText.text = title;
		descText.text = description;
		dialoguePanel.SetActive (true); 
	}

	public void HideDialogue () {
		dialoguePanel.SetActive (false);
	}

	void Save (int highScore, int lowScore, string playerName) {
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + "/highScore.dat", FileMode.OpenOrCreate);

		PlayerData data = new PlayerData (highScore,lowScore,playerName);

		bf.Serialize (file, data);
		file.Close ();
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

	public void PauseGame () {
		Time.timeScale = 0;
		menuPanel.SetActive (true);
	}

	public void ResumeGame () {
		Time.timeScale = 1;
		menuPanel.SetActive (false);
	}

	public void QuitGame () {
		Application.Quit ();
	}

	public void ReturnToMenu () {
		SceneManager.LoadScene ("Start");
		Destroy (instance.gameObject);
	}
}

[Serializable]
public class PlayerData {
	public int highScore;
	public int lowScore;
	public string name;

	public PlayerData (int hs,int ls,string n) {
		highScore = hs;
		lowScore = ls;
		name = n;
	}
}
