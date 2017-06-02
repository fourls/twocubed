using UnityEngine;
using System.Collections;

public class Resource : MonoBehaviour {

	public int durability = 2;
	public int type = 1;
	public int amount = 1;
	public Sprite dmgSprite;
	private bool deadeding = false;

	public bool TakeDamage (int damage) {
		if (!deadeding) {
			durability -= damage;
			if (durability <= 0) {
				enabled = false;
				Invoke ("Destruct", 0.2f);
				deadeding = true;
				return true;
			}
		}
		return false;
	}

	void Destruct () {
		GetComponent<BoxCollider2D> ().enabled = false;
		gameObject.layer = LayerMask.NameToLayer ("Default");
		GetComponent<SpriteRenderer> ().sprite = dmgSprite;
	}
}

/* 

red ore = 1
green ore = 2
blue ore = 3
yellow ore = 4

*/