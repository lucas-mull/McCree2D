using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour {

    public GameObject _mccree;
    public int _speed;

    private float _xOffset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        _xOffset = _mccree.transform.position.x / _speed;
        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(_xOffset, 0));
	}
}
