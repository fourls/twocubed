using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public AudioSource sfxAudio1;
	public AudioSource sfxAudio2;
	public AudioSource sfxAudio3;
	public AudioSource sfxAudio4;
	public AudioSource musicAudio;
	public static SoundManager instance;


	// Use this for initialization
	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
	}

	public void PlaySingle (AudioClip clip, bool withPitch) {
		AudioSource sfxAudio = sfxAudio1;
		if (sfxAudio1.isPlaying) {
			sfxAudio = sfxAudio2;
			if (sfxAudio2.isPlaying) {
				sfxAudio = sfxAudio3;
				if (sfxAudio3.isPlaying) {
					sfxAudio = sfxAudio4;
				}
			}
		}
			
		float pitchAlter = Random.Range (0.95f, 1.05f);

		if(!withPitch)
			pitchAlter = 1f;

		sfxAudio.pitch = pitchAlter;
		sfxAudio.clip = clip;
		sfxAudio.Play ();
	}
}
