using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXInstance : MonoBehaviour {
    AudioSource audioSource;
    void Start() {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    // Update is called once per frame
    void Update() {  
        audioSource.mute = SoundController.soundMuted;
        if (!audioSource.isPlaying) {
            Destroy(gameObject, 0.5f);
        }
    }
}