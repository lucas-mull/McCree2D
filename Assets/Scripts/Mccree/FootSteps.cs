using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour {

    public AudioClip _soundStep;
    public float _pitchMin = 0.9f, _pitchMax = 1.2f, _volumeMin = 0.8f, _volumeMax = 1.2f;

    private AudioSource _audioSource;

	// Use this for initialization
	void Start () {
        _audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetAxis("Horizontal") != 0 && !_audioSource.isPlaying)
        {
            _audioSource.pitch = Random.Range(_pitchMin, _pitchMax);
            _audioSource.volume = Random.Range(_volumeMin, _volumeMax);
            _audioSource.PlayOneShot(_soundStep);
        }
	}
}
