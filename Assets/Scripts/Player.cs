using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject {

	public class Item {
		public int amount;
		public int type;

		public Item (int amt, int tp) {
			amount = amt;
			type = tp;
		}
	}

	public GameObject spikePrefab;

	public AudioClip footStepSound;
	public AudioClip mineSound;
	public AudioClip pickUpHeartSound;
	public AudioClip pickUpSound;

	public float mineTime;
	public int playerDamage = 1;
	public int pointsPerHeart;
	public float restartLevelDelay;

	public RuntimeAnimatorController maskController;
	public RuntimeAnimatorController defaultController;
	private Animator animator;
	private int health;
	private bool currentlyMoving = false;
	private bool movingIntoBoard = false;
	private Text healthText;
	public Text redText;
	public Text greenText;
	public Text blueText;
	public Text yellowText;
	public Text swordText;
	public Text maskText;
	public Text spikeText;
	public List<int> inventory = new List<int> ();

	[HideInInspector] public int wearingMask = 0;

	private GameObject lastReadSign;

	public List<int> powerups = new List<int> ();

	public void MoveIntoBoard () {
		movingIntoBoard = true;
		StartCoroutine (SmoothMovement (transform.position + Vector3.up));
	}

	protected override void Start () {
		animator = GetComponent<Animator> ();
		health = GameManager.instance.playerHealth;
		inventory = GameManager.instance.playerInventory;
		powerups = GameManager.instance.playerPowerups;
		healthText = GameObject.Find ("Health Text").GetComponent<Text>();
		UpdateText ();
		base.Start ();
	}

	void OnApplicationFocus (bool focus) {
		if (!focus && !GameManager.instance.settingUp)
			GameManager.instance.PauseGame ();
	}

	void OnDisable () {
		GameManager.instance.playerHealth = health;
		GameManager.instance.playerInventory = inventory;
		GameManager.instance.playerPowerups = powerups;
	}

	public void UpdateText () {
		int showHealth = health;
		if (health < 0)
			showHealth = 0;
		healthText.text = "" + showHealth;
		redText.text = "x" + inventory [1];
		greenText.text = "x" + inventory [2];
		blueText.text = "x" + inventory [3];
		yellowText.text = "x" + inventory [4];
		swordText.text = "x" + powerups [1];
		maskText.text = "x" + powerups [2];
		spikeText.text = "x" + powerups [3];
	}

	public void UpdateText (int addition) {
		if (addition >= 0) {
			UpdateText ();
			healthText.text += " +" + addition;
		} else {
			UpdateText ();
			healthText.text += " -" + addition * -1;
		}
		redText.text = "x" + inventory [1];
		greenText.text = "x" + inventory [2];
		blueText.text = "x" + inventory [3];
		yellowText.text = "x" + inventory [4];
		swordText.text = "x" + powerups [1];
		maskText.text = "x" + powerups [2];
		spikeText.text = "x" + powerups [3];
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
			PauseButtonClicked();

		if (!GameManager.instance.playersTurn || GameManager.instance.settingUp || Time.timeScale == 0)
			return;
		
		if (Input.GetKeyDown(KeyCode.M) && powerups[2] > 0 && wearingMask == 0) {
			powerups [2]--;
			wearingMask = 10;
			animator.runtimeAnimatorController = maskController;
		}

		if (Input.GetKeyDown(KeyCode.K) && powerups[3] > 0) {
			powerups [3]--;
			Instantiate (spikePrefab, transform.position, Quaternion.identity);
		}

		int horizontal = 0;
		int vertical = 0;

		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		if (horizontal != 0)
			vertical = 0;

		if ((horizontal != 0 || vertical != 0) && !currentlyMoving) {
			animator.SetBool ("walking", true);
			AttemptMove (horizontal, vertical);
		}
	}

	protected override bool AttemptMove (int xDir, int yDir) {
		currentlyMoving = true;
		UpdateText ();
		if (wearingMask > 0) {
			wearingMask--;
			if (wearingMask == 0) {
				animator.runtimeAnimatorController = defaultController;
			}
		}

		if(base.AttemptMove (xDir, yDir))
			SoundManager.instance.PlaySingle (footStepSound,true);


		return true;
	}

	protected override void OnCompleteMove () {
		if (enabled) {
			animator.SetBool ("walking", false);
		}
		if (!movingIntoBoard) {
			EndPlayersTurn ();
		} else {
			movingIntoBoard = false;
			GameManager.instance.settingUp = false;
			currentlyMoving = false;
		}
		if (lastReadSign != null) {
			if (!GetComponent<BoxCollider2D> ().IsTouching (lastReadSign.GetComponent<BoxCollider2D> ())) {
				GameManager.instance.HideDialogue ();
			}
		}
	}

	protected override void OnCantMove (GameObject go, int xDir, int yDir) {
		if (go.tag == "Resource") {
			Resource hitResource = go.GetComponent<Resource> ();
			animator.SetBool ("walking", false);
			animator.SetTrigger ("mining");
			Invoke ("EndPlayersTurn", mineTime);
			SoundManager.instance.PlaySingle (mineSound, true);
			if (hitResource.TakeDamage (playerDamage)) {
				inventory [hitResource.type]++;
				UpdateText ();
			}
			return;
		} else if (go.transform.parent != null) {
			if (go.transform.parent.name != "Item Container" && go.transform.parent.name != "Board Container") {
				if (go.transform.parent.GetComponentInChildren<Enemy> () != null) {
					Enemy en = go.transform.parent.GetComponentInChildren<Enemy> ();
					if (powerups [1] != 0) {
						en.Die ();
						powerups [1]--;
						UpdateText ();
						animator.SetTrigger ("swording");
					}
					animator.SetBool ("walking", false);
					Invoke ("EndPlayersTurn", 0.2f);
					return;
				}
			}
		}
		animator.SetBool ("walking", false);
		Invoke("EndPlayersTurn",0.2f);
	}

	void EndPlayersTurn () {
		GameManager.instance.playersTurn = false;
		currentlyMoving = false;
		Debug.Log ("End of player's turn");
	}

	void IsGameOver () {
		if (health <= 0) {
			enabled = false;
			GameManager.instance.GameOver();
		}
	}

	public void LoseHealth (int damage) {
		health -= damage;
		animator.SetTrigger ("hit");
		UpdateText (-damage);

		IsGameOver ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Exit") {
			Invoke ("Restart", restartLevelDelay);
			animator.SetBool ("walking", true);
			StartCoroutine(SmoothMovement(new Vector3(other.transform.position.x,other.transform.position.y + 2,0f)));
			enabled = false;
		} else if (other.tag == "Heart") {
			health += pointsPerHeart;
			if (health > 10)
				health = 10;
			UpdateText (pointsPerHeart);
			SoundManager.instance.PlaySingle (pickUpHeartSound, false);
			other.gameObject.SetActive (false);
		} else if (other.tag == "Sign") {
			Sign sn = other.GetComponent<Sign> ();
			GameManager.instance.ShowDialogue (sn.title, sn.description);
			GameManager.instance.AddToEndScore (sn.GetInstanceID (), "sign_read");
			lastReadSign = other.gameObject;
		} else if (other.tag == "NPC") {
			NPC npc = other.GetComponent<NPC> ();
			GameManager.instance.ShowDialogue (npc.npcName, npc.sentence);
			GameManager.instance.AddToEndScore (npc.GetInstanceID (), "npc_talk");
			lastReadSign = other.gameObject;
		} else if (other.tag == "Sword") {
			powerups [1]++;
			UpdateText ();
			other.gameObject.SetActive (false);
			SoundManager.instance.PlaySingle (pickUpSound, false);
		} else if (other.tag == "Mask") {
			powerups [2]++;
			UpdateText ();
			other.gameObject.SetActive (false);
			SoundManager.instance.PlaySingle (pickUpSound, false);
		} else if (other.tag == "Spike Box") {
			powerups [3]++;
			UpdateText ();
			other.gameObject.SetActive (false);
			SoundManager.instance.PlaySingle (pickUpSound, false);
		}
	}

	public void PauseButtonClicked () {
		if (!GameManager.instance.settingUp) {
			if (Time.timeScale == 1)
				GameManager.instance.PauseGame ();
			else
				GameManager.instance.ResumeGame ();
		}
	}

	public void Restart () {
		GameManager.instance.NextLevel();
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}
}
