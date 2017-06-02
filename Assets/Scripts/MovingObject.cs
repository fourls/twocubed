using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour {

	public float moveTime = 0.1f;
	public float maxDist = 10f;
	public LayerMask blockingLayer;

	public GameObject boxCollider;
	private Rigidbody2D rb2D;
	private float inverseMoveTime;

	// Use this for initialization
	protected virtual void Start () {
		rb2D = GetComponent<Rigidbody2D> ();
		inverseMoveTime = 1f / moveTime;
	}
	
	protected bool Move (int xDir, int yDir, out RaycastHit2D hit) {
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2 (xDir, yDir);
		
		boxCollider.GetComponent<BoxCollider2D>().enabled = false;
		hit = Physics2D.Linecast (start, end, blockingLayer);
		boxCollider.GetComponent<BoxCollider2D>().enabled = true;
		
		if (hit.transform == null) {
			boxCollider.transform.position = end;
			StartCoroutine (SmoothMovement (end));
			return true;
		}
		return false;
	}
	
	protected IEnumerator SmoothMovement (Vector3 end) {
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
		while (sqrRemainingDistance > float.Epsilon) {
			Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition(newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;

		}

		OnCompleteMove ();
	}

	protected virtual bool AttemptMove (int xDir, int yDir) {
		RaycastHit2D hit;
		bool canMove = Move (xDir, yDir, out hit);

		if (hit.transform != null) {
			if (!canMove)
				OnCantMove (hit.transform.gameObject, xDir, yDir);
		}
		return canMove;
	}

	protected abstract void OnCompleteMove ();

	protected abstract void OnCantMove (GameObject go, int xDir, int yDir);
}
