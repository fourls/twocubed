using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {

	public AudioClip enemySound;
	public AudioClip dieSound;
	public AudioClip spikeSound;

	public int damage = 10;
	public bool useSkipMove = true;

	private bool finishAcknol;
	private bool moving;

	private Animator animator;
	[HideInInspector] public Transform target;
	private int timesMovedInFrame;
	private bool skipMove = false;
	protected override void Start () {
		GameManager.instance.AddEnemiesToList (this);
		animator = GetComponent<Animator> ();
		target = GameObject.FindGameObjectWithTag ("Player").transform;
		base.Start ();
	}
		
	protected override bool AttemptMove(int xDir, int yDir) {
		moving = true;
		base.AttemptMove (xDir,yDir);
		return true;
	}

	public void MoveEnemy() {
		finishAcknol = false;

		if (!enabled) {
			AcknowledgeFinish ();
			return;
		}

		if (useSkipMove) {
			if (skipMove) {
				AcknowledgeFinish ();
				skipMove = false;
				return;
			}
		}

		int xDir = 0;
		int yDir = 0;
		timesMovedInFrame = 0;

		if (Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon)
			yDir = target.position.y > transform.position.y ? 1 : -1;
		else 
			xDir = target.position.x > transform.position.x ? 1 : -1;

		animator.SetBool ("walking", true);
		skipMove = true;
		AttemptMove (xDir, yDir);
	}

	protected override void OnCantMove (GameObject go, int xDir, int yDir) {
		moving = false;
		if (!enabled) {
			AcknowledgeFinish ();
			return;
		}
		animator.SetBool ("walking", false);
		if (go == null) {
			AcknowledgeFinish ();
			return;
		}
		if (go.transform.parent != null) {
			if (go.transform.parent.name != "Item Container" && go.transform.parent.name != "Board Container") {
				if (go.transform.parent.GetComponentInChildren<Player> () != null) {
					Player pl = go.transform.parent.GetComponentInChildren<Player> ();
					if (pl.powerups [1] != 0) {
						pl.powerups [1]--;
						pl.UpdateText ();
						pl.GetComponent<Animator> ().SetTrigger ("swording");
						AcknowledgeFinish ();
						Die ();
					} else {
						pl.LoseHealth (damage);
						animator.SetTrigger ("attack");
						Invoke ("AcknowledgeFinish", 0.5f);
						SoundManager.instance.PlaySingle (enemySound, true);
					}
					return;
				} else if (go.transform.parent.GetComponent<NPC> () != null) {
					target = go.transform.parent.transform;
					animator.SetTrigger ("attack");
					AcknowledgeFinish ();
					go.transform.parent.GetComponent<NPC> ().AnimHit ();
					return;
				}
			}
		}

		timesMovedInFrame++;

		if (timesMovedInFrame > 1 || !enabled) {
			AcknowledgeFinish ();
			return;
		}
		
		if (xDir != 0) {
			yDir = target.position.y > transform.position.y ? 1 : -1;
			AttemptMove (0, yDir);
		} else if (yDir != 0) {
			xDir = target.position.x > transform.position.x ? 1 : -1;
			AttemptMove (xDir, 0);
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == "Spike") {
			other.GetComponent<Animator> ().SetTrigger ("attack");
			SoundManager.instance.PlaySingle (spikeSound,true);
			Die ();
		}
	}

	public void Die () {
		if (moving) {
			AcknowledgeFinish ();
		}
		boxCollider.SetActive (false);
		animator.SetTrigger ("dead");
		Invoke ("ByeBye", 0.5f);
		enabled = false;
		SoundManager.instance.PlaySingle (dieSound,true);
	}

	void ByeBye () {
		transform.parent.gameObject.SetActive (false);
	}

	void AcknowledgeFinish () {
		if (finishAcknol) {
			return;
		}
		moving = false;
		GameManager.instance.enemiesFinishedMoving ++;
		finishAcknol = true;
	}

	protected override void OnCompleteMove () {
		animator.SetBool ("walking", false);
		AcknowledgeFinish ();
	}
}
