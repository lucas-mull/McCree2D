using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {

    public const string MOVEMENT_VERTICAL = "vertical";
    public const string MOVEMENT_HORIZONTAL = "horizontal";
    public const string MOVEMENT_NONE = "none";

    public AudioClip _soundHit;
    public int _scoreValue = 10;    
    public Sprite _sprite10;
    public Sprite _sprite50;
    public GameObject _player;
    public TargetsController _targetsController;

    private Animator _animator;
    private AudioSource _audioSource;
    private bool _enabled = true, _back = false;
    private Mccree _mccree;
    private string _movement = MOVEMENT_NONE;
    private float _speed = 7;
    private Transform _origin, _destination, _deathIconTransform;

    private void Start()
    {
        _enabled = true;

        _origin = transform.parent.FindChild("A");
        _destination = transform.parent.FindChild("B");
        _deathIconTransform = transform.parent.FindChild("death");

        _animator = transform.parent.GetComponent<Animator>();
        _audioSource = transform.parent.GetComponent<AudioSource>();
        _mccree = _player.GetComponent<Mccree>();
        switch(_scoreValue)
        {
            case 50:
                transform.parent.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite50;
                break;
            default:
                transform.parent.GetChild(0).GetComponent<SpriteRenderer>().sprite = _sprite10;
                break;
        }
    }

    private void Update()
    {
        //transform.position = new Vector2(0f, 20f);
        switch (_movement)
        {
            case MOVEMENT_VERTICAL:

                if (!_back)
                {
                    transform.localPosition = Vector2.MoveTowards(transform.localPosition, _destination.localPosition, _speed * Time.deltaTime);
                    _deathIconTransform.localPosition = Vector2.MoveTowards(_deathIconTransform.localPosition, _destination.localPosition, _speed * Time.deltaTime);
                }
                else
                {
                    transform.localPosition = Vector2.MoveTowards(transform.localPosition, _origin.localPosition, _speed * Time.deltaTime);
                    _deathIconTransform.localPosition = Vector2.MoveTowards(_deathIconTransform.localPosition, _origin.localPosition, _speed * Time.deltaTime);
                }

                break;
            default:
                break;
        }
    }

    private void OnMouseDown()
    {
        if (_enabled && !_mccree.IsReloading() && !_mccree.IsInDeadeye())
        {
            Destroy();
        }
        else
        {
            Debug.Log("Hit but not registered - _enabled = " + _enabled + " , reloading = " + _mccree.IsReloading());
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "A")
        {
            _back = false;
        }
        else if (other.gameObject.name == "B")
        {
            _back = true;
        }
    }

    public void Deadeye()
    {
        transform.parent.GetComponent<Animator>().SetTrigger("deadeye");
    }

    public void Destroy()
    {
        PerformMovement(MOVEMENT_NONE);

        GetComponent<CircleCollider2D>().enabled = false;
        _enabled = false;

        _mccree.UpdateScore(_scoreValue);

        _mccree.Hit();
        _animator.SetTrigger("hit");
        _audioSource.PlayOneShot(_soundHit);

        _targetsController.TargetDestroyed(this);
    }

    public void PerformMovement(string movement)
    {
        //transform.parent.GetComponent<Animator>().SetTrigger(movement);

        _movement = movement;
    }

    public void DestroyInstance()
    {        
        Destroy(transform.parent.gameObject);
    }

    public void Disappear()
    {
        _animator.SetTrigger("disappear");
        _enabled = false;
    }

    public bool IsEnabled()
    {
        return _enabled;
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;        
    }
}
