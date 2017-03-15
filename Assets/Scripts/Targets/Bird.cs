using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bird : MonoBehaviour {

    public Camera _mainCamera;
    public AudioClip _soundVulture, _soundVultureDead, _soundVultureAttack;
    public float _speed = 5f;
    public Mccree _mccree;
    public TargetsController _birdController;
    public int _scoreValue = 30, _headMultiplicator = 2;

    private Animator _animator;
    private AudioSource _audioSource;
    private bool _move = false, _attack = false, _hasAlreadyAttacked = false;
    private Transform _destination;
    private Text _score;
    private BoxCollider2D _headHitBox, _bodyHitBox;
    private Quaternion _originTextRotation;

	// Use this for initialization
	void Start () {
        _animator = transform.parent.GetComponent<Animator>();
        _audioSource = transform.parent.GetComponent<AudioSource>();
        _mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();

        _audioSource.loop = true;
        _audioSource.clip = _soundVulture;
        _audioSource.Play();

        _score = transform.parent.FindChild("Canvas").FindChild("scoreText").GetComponent<Text>();
        _score.text = "";

        _originTextRotation = _score.transform.rotation;

        _headHitBox = transform.parent.FindChild("head").GetComponent<BoxCollider2D>();
        _bodyHitBox = transform.parent.FindChild("bodyNormal").GetComponent<BoxCollider2D>();
    }

    private void LateUpdate()
    {
        _score.transform.rotation = _originTextRotation;
    }

    // Update is called once per frame
    void Update () {

		if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit;
            hit = Physics2D.Raycast(_mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && !_mccree.IsReloading() && (hit.collider.tag == "Head" || hit.collider.tag == "Body"))
            {
                _mccree.Hit();

                SetEnabled(false);

                _move = false;

                int score = _scoreValue;

                if (_audioSource.isPlaying)
                    _audioSource.Stop();

                _audioSource.loop = false;
                _audioSource.PlayOneShot(_soundVultureDead);

                switch (hit.collider.tag)
                {
                    case "Head":
                        score *= _headMultiplicator;
                        _score.text = "" + score;
                        _animator.SetTrigger("deadHeadshot");                        
                        break;
                    case "Body":
                        _score.text = "" + score;
                        _animator.SetTrigger("dead");
                        break;
                    default:
                        break;
                }

                _mccree.UpdateScore(score);
                _birdController.BirdKilled(this);
            }
        }

        if (_move)
        {
            Transform target = _destination;
            if (_attack)
                target = _mccree.transform;

            transform.parent.position = Vector2.MoveTowards(transform.parent.position, target.position, _speed * Time.deltaTime);
        }
	}

    public void Kill()
    {
        _mccree.Hit();

        SetEnabled(false);

        _move = false;

        if (_audioSource.isPlaying)
            _audioSource.Stop();

        _audioSource.loop = false;
        _audioSource.PlayOneShot(_soundVultureDead);

        _score.text = "" + _scoreValue;
        _animator.SetTrigger("dead");
        
        _mccree.UpdateScore(_scoreValue);
        _birdController.BirdKilled(this);

    }

    public void Attack()
    {
        Vector3 vectorToTarget = _mccree.transform.position - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + 90;
        transform.parent.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        _animator.SetTrigger("attack");

        _audioSource.loop = false;
        _audioSource.Stop();
        _audioSource.PlayOneShot(_soundVultureAttack);

        _attack = true;
    }

    public void SetEnabled(bool enabled)
    {
        _headHitBox.enabled = enabled;
        _bodyHitBox.enabled = enabled;
    }
    
    public void Flip(bool flip)
    {
        if (flip)
        {
            transform.parent.localScale = new Vector3(-1, 1, 1);
            transform.parent.FindChild("Canvas").FindChild("scoreText").localScale = new Vector3(-1, 1, 1);
        }            
        else
        {
            transform.parent.localScale = new Vector3(1, 1, 1);
        }            
    }

    public void SetDestination(Transform destination)
    {
        _destination = destination;
        _move = true;
    }

    public void Deadeye()
    {
        _animator.SetTrigger("deadeye");
    }

    public bool HasAttacked()
    {
        return _hasAlreadyAttacked;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == _destination.name)
        {
            Destroy(gameObject);
            _audioSource.Stop();
            GetComponent<Bird>().enabled = false;

            _birdController.BirdKilled(this);
        }
        else if (collision.tag == "Player")
        {
            _attack = false;

            Vector3 vectorToTarget = _destination.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg + 90;
            transform.parent.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            _hasAlreadyAttacked = true;
        }
    }
}
