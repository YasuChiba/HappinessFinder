using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioClip shutterSound;
    public AudioClip foundSound;

    AudioSource audioSource;

	// Use this for initialization
	void Start () {
        audioSource = this.gameObject.AddComponent<AudioSource>();

	}
	
	public void cloverFound()
    {
        this.audioSource.PlayOneShot(foundSound);
    }

    public void shutter()
    {
        this.audioSource.PlayOneShot(shutterSound);

    }
}
