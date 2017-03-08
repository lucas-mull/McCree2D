﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHit : MonoBehaviour {

    public AudioClip _soundHit;
    public int _scoreValue = 10;    
    public Sprite _sprite10;
    public Sprite _sprite50;
    public GameObject _mccreeGO;
    public SpawnTargets _targetsController;

    private Animator _animator;
    private AudioSource _audioSource;
    private bool _enabled = true;
    private int _targetId = 0;
    private Mccree _mccree;

    private void Start()
    {
        _enabled = true;

        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _mccree = _mccreeGO.GetComponent<Mccree>();
        switch(_scoreValue)
        {
            case 50:
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite50;
                break;
            default:
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite10;
                break;
        }
    }

    private void OnMouseDown()
    {
        if (_enabled && !_mccree.IsReloading())
        {
            GetComponent<CircleCollider2D>().enabled = false;
            _enabled = false;            

            _mccree.UpdateScore(_scoreValue);

            _mccree.Hit();
            _animator.SetTrigger("hit");
            _audioSource.PlayOneShot(_soundHit);           

            _targetsController.TargetDestroyed(this);
        }
        else
        {
            Debug.Log("Hit but not registered - _enabled = " + _enabled + " , reloading = " + _mccree.IsReloading());
        }
        
    }

    private void DestroyInstance()
    {        
        Destroy(gameObject);
    }

    public void AssignId(int id)
    {
        _targetId = id;
    }

    public void Disappear()
    {
        _animator.SetTrigger("disappear");
        _enabled = false;
    }

    public bool IsColliding(TargetHit other)
    {
        return GetComponent<CircleCollider2D>().IsTouching(other.GetComponent<CircleCollider2D>());
    }
}
