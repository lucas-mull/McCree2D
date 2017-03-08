using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mccree : MonoBehaviour {

    const int CLIP_SIZE = 6;

    public AudioClip _soundGun, _soundStep, _soundReload;
    public Text _scoreText, _hintText, _comboText;
    public Image _clipBar;
    public float _reloadSpeed = 1.0f;
    public int _comboTreshold = 3;

    private Animator _animator;
    private AudioSource _audioSource;
    private float _speed = 7.0f, _reloadTime, _timeSpentReloading = 0.0f;
    private int _score = 0, _bullets = CLIP_SIZE, _shotsTaken = 0, _totalHits = 0, _totalMissed = 0;
    private bool _updateScore = false, _facingLeft = true, _isShooting = false, _isMoving = false, _isReloading = false;
    private int _shotsWithoutMissing = 0, _combo = 1, _maxCombo = 1;

	// Use this for initialization
	void Start () {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _reloadTime = _soundReload.length;
    }
	
	// Update is called once per frame
	void Update () {

        _comboText.text = _combo + "x";

        if (_updateScore)
        {
            _updateScore = false;
            _scoreText.text = "" + _score;
            if (_score < 0)
                _scoreText.color = Color.red;
            else
                _scoreText.color = Color.green;            
        }

        //float h = Input.GetAxis("Horizontal");
        //float movementValue = h * _speed;

        //if (movementValue != 0)
        //{
        //    _isMoving = true;
        //    if (movementValue > 0 && _facingLeft)
        //    {
        //        _facingLeft = false;
        //        GetComponent<SpriteRenderer>().flipX = true;
        //    }
        //    else if (movementValue < 0 && !_facingLeft)
        //    {
        //        _facingLeft = true;
        //        GetComponent<SpriteRenderer>().flipX = false;
        //    }

        //    transform.Translate(Vector3.right * movementValue * Time.deltaTime);
        //    _animator.SetFloat("speed", _speed);
        //}
        //else
        //{            
        //    _isMoving = false;
        //    _animator.SetFloat("speed", -1.0f);
        //}

        if (Input.GetMouseButton(0) && !_isShooting)
        {            
            StartCoroutine(Shoot());
        }
        else if (Input.GetKeyDown(KeyCode.R) && _bullets < CLIP_SIZE)
        {
            StartCoroutine(Reload());
        }

        // Personnage se tourne vers l'hémisphère de l'écran où se situe la souris
        Vector2 mccreePosition = Camera.main.WorldToScreenPoint(transform.position);        
        GetComponent<SpriteRenderer>().flipX = (Input.mousePosition.x > mccreePosition.x);

    }


    IEnumerator Shoot()
    {
        if (_isReloading)
            yield break;

        _isShooting = true;

        _bullets--;

        // Needs this in order to prevent player from shooting one last bullet before reloading
        if (_bullets == 0)
            _isReloading = true;
        
        _shotsTaken++;

        // Player missed
        if (_shotsTaken - _totalHits > _totalMissed)
        {
            _totalMissed++;
            _hintText.text = "MISS !";
            _hintText.GetComponent<Animator>().SetTrigger("appear");

            _shotsWithoutMissing = 0;
            _combo = 1;
        }

        _clipBar.fillAmount -= (1.0f / 6.0f);               

        _animator.SetTrigger("shoot");
        _audioSource.PlayOneShot(_soundGun);
        yield return new WaitForSeconds(0.1f);

        // Reload
        if (_bullets == 0)
        {
            yield return StartCoroutine(Reload());
        }
                
        _isShooting = false;        
    }

    IEnumerator Reload()
    {
        _isReloading = true;

        _timeSpentReloading += _reloadTime / _reloadSpeed;

        _hintText.text = "RELOADING";
        Animator hintAnimator = _hintText.GetComponent<Animator>();
        hintAnimator.SetTrigger("blink");
        hintAnimator.speed = _reloadSpeed;

        _bullets = CLIP_SIZE;
        _audioSource.pitch = _reloadSpeed;
        _audioSource.PlayOneShot(_soundReload);
        yield return new WaitForSeconds(_reloadTime/_reloadSpeed);

        _isReloading = false;
        _clipBar.fillAmount = 1.0f;

        hintAnimator.speed = _reloadSpeed;
        _audioSource.pitch = 1.0f;
    }

    public void Hit()
    {
        _totalHits++;
        _shotsWithoutMissing++;
        if (_shotsWithoutMissing % _comboTreshold == 0)
        {
            _combo *= 2;

            if (_combo > _maxCombo)
                _maxCombo = _combo;
        }            
    }

    public void UpdateScore(int toAdd)
    {
        _score += (_combo * toAdd);
        _updateScore = true;
    }

    public bool IsReloading()
    {
        return _isReloading;
    }

    public void ReportScoringData()
    {
        EndLevel endingScreen = GetComponent<EndLevel>();
        endingScreen.TotalHits = _totalHits;
        endingScreen.TotalMissed = _totalMissed;
        endingScreen.MaxCombo = _maxCombo;
        endingScreen.TimeSpentReloading = _timeSpentReloading;
        endingScreen.TotalScore = _score;
    }
}
