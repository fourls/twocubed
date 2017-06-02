using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour {

	public char gender;
	[HideInInspector] public string npcName;
	[HideInInspector] public string sentence;

	// Use this for initialization
	void Start () {
		if(gender == 'f')
			npcName = GameManager.instance.femaleNames [Random.Range (0, GameManager.instance.femaleNames.Length)];
		else
			npcName = GameManager.instance.maleNames [Random.Range (0, GameManager.instance.maleNames.Length)];
		sentence = GameManager.instance.NPCSentences [Random.Range (0, GameManager.instance.NPCSentences.Length)];
	}

	public void AnimHit () {
		GetComponent<Animator> ().SetTrigger ("hit");
	}
}
