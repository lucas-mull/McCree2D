using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mccree : MonoBehaviour {

    const int CLIP_SIZE = 6;
    const int MAX_MULTIPLICATOR = 32;

    public AudioClip _soundGun, _soundStep, _soundReload, _voiceDeadeyeReady;
    public AudioClip[] _voiceLines;
    public Text _scoreText, _hintText, _comboText;
    public Image _clipBar, _deadeyeBar, _deadeyeFlames;
    public GameObject _mccreeFlames;
    public float _reloadSpeed = 1.0f, _hurtTime = 1f;
    public int _comboTreshold = 3;

    private Animator _animator;
    private AudioSource _audioSource, _voiceAudioSource;
    private float _speed = 7.0f, _reloadTime, _timeSpentReloading = 0.0f, _deadeyePercent = 0.0f;
    private int _score = 0, _bullets = CLIP_SIZE, _shotsTaken = 0, _totalHits = 0, _totalMissed = 0, _deadeyeKills = 0;
    private bool _updateScore = false, _facingLeft = true, _isShooting = false, _isMoving = false, _isReloading = false, _deadeyeReady = false, _inDeadeye = false, _isHurting = false;
    private int _shotsWithoutMissing = 0, _combo = 1, _maxCombo = 1;
    private int _indexOfLastVoiceClip = 0;

	// Use this for initialization
	void Start () {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _voiceAudioSource = GameObject.Find("Interface").GetComponent<AudioSource>();

        _reloadTime = _soundReload.length;
    }
	
	// Update is called once per frame
	void Update () {

        _comboText.text = _combo + "x";
        _deadeyeBar.fillAmount = _deadeyePercent;

        if (IsDeadeyeReady() && !_deadeyeReady)
        {            
            _voiceAudioSource.PlayOneShot(_voiceDeadeyeReady);

            Debug.Log("Deadeye is ready !");
            _deadeyeReady = true;
            _deadeyeFlames.GetComponent<Animator>().SetBool("ult", true);
            //_mccreeFlames.SetActive(true);
        }

        if (_updateScore)
        {
            _updateScore = false;
            _scoreText.text = "" + _score;         
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

        if (Input.GetMouseButtonDown(0) && !_isShooting && !_isReloading && !_inDeadeye)
        {            
            StartCoroutine(Shoot());
        }
        else if (Input.GetKeyDown(KeyCode.R) && _bullets < CLIP_SIZE)
        {
            StartCoroutine(Reload());
        }

        // Character faces the half of the screen where mouse cursor (crosshair) is located
        Vector2 mccreePosition = Camera.main.WorldToScreenPoint(transform.position);        
        GetComponent<SpriteRenderer>().flipX = (Input.mousePosition.x > mccreePosition.x);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Hit - name = " + collision.name + " - tag = " + collision.transform.parent.tag);
        if (collision.transform.parent.tag == "Enemy" && !_isHurting)
        {
            _isHurting = true;
            StartCoroutine(Hurt());
        }
    }

    IEnumerator Hurt()
    {
        // Combo falls back to 1 when hit
        // TODO - cancels high noon if started ?
        ResetCombo();
        ShowHint("Combo lost !", Color.red, "appear");

        // Make character blink for _hurtTime seconds
        _animator.SetBool("hurt", true);
        yield return new WaitForSeconds(_hurtTime);
        _animator.SetBool("hurt", false);

        // Character can be hurt again
        _isHurting = false;
    }


    IEnumerator Shoot()
    {
        _isShooting = true;

        _bullets--;

        // Needs this in order to prevent player from shooting one last bullet before reloading
        if (_bullets == 0)
            _isReloading = true;
        
        _shotsTaken++;        

        _clipBar.fillAmount -= (1.0f / 6.0f);               

        _animator.SetTrigger("shoot");
        _audioSource.PlayOneShot(_soundGun);
        yield return new WaitForSeconds(0.1f);

        // Player missed
        if (_shotsTaken - _totalHits > _totalMissed)
        {
            _totalMissed++;

            ShowHint("MISS !", Color.white, "appear");

            _shotsWithoutMissing = 0;
            ResetCombo();
        }

        // Reload
        if (_bullets == 0)
        {
            yield return StartCoroutine(Reload());
        }
                
        _isShooting = false;        
    }

    private void ResetCombo()
    {
        _combo = 1;
    }

    IEnumerator Reload()
    {
        _isReloading = true;

        _timeSpentReloading += _reloadTime / _reloadSpeed;

        ShowHint("RELOADING", Color.white, "blink");
        Animator hintAnimator = _hintText.GetComponent<Animator>();
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
        if (_shotsWithoutMissing % _comboTreshold == 0 && _combo < MAX_MULTIPLICATOR)
        {
            _combo *= 2;

            if (_combo > _maxCombo)
                _maxCombo = _combo;

            ShowHint(_combo + "X", Color.yellow, "appear");

            if (!_voiceAudioSource.isPlaying && Random.Range(0, 2) == 1)
            {
                int voiceClipIndex = Random.Range(0, _voiceLines.Length);
                if (voiceClipIndex != _indexOfLastVoiceClip)
                {
                    _voiceAudioSource.PlayOneShot(_voiceLines[voiceClipIndex]);
                    _indexOfLastVoiceClip = voiceClipIndex;
                }                    
            }                
        }
    }

    public void IncrementDeadeyePercent()
    {
        _deadeyePercent += (0.01f * _combo);
        _deadeyePercent = Mathf.Clamp(_deadeyePercent, 0.0f, 1.0f);
    }

    public void NotifyDeadeyeHasStarted()
    {
        _inDeadeye = true;
    }

    public void Deadeye(int targets)
    {
        StartCoroutine(Ult(targets));
    }

    public bool IsDeadeyeReady()
    {
        return _deadeyePercent == 1.0f;
    }

    private IEnumerator Ult(int targets)
    {
        _inDeadeye = true;
        for (int i = 0; i < targets; i++, _shotsTaken++)
        {
            _animator.SetTrigger("shoot");
            _audioSource.PlayOneShot(_soundGun);
            yield return new WaitForSeconds(0.1f);
        }

        _deadeyePercent = 0.0f;
        //_mccreeFlames.SetActive(false);
        _deadeyeFlames.GetComponent<Animator>().SetBool("ult", false);
        _deadeyeReady = false;
        _inDeadeye = false;
        _deadeyeKills += targets;
    }

    public void UpdateScore(int toAdd)
    {
        // Les cibles touchées par l'ult ne comptent pas pour remplir la jauge du prochain ult.
        if (!_deadeyeReady)
        {
            _deadeyePercent += (0.01f * toAdd / 10.0f);
            _deadeyePercent = Mathf.Clamp(_deadeyePercent, 0.0f, 1.0f);
        }

        _score += (_combo * toAdd);
        _updateScore = true;
    }

    public bool IsReloading()
    {
        return _isReloading;
    }

    public bool IsInDeadeye()
    {
        return _inDeadeye;
    }

    public void ReportScoringData()
    {
        EndLevel endingScreen = GetComponent<EndLevel>();
        endingScreen.TotalHits = _totalHits;
        endingScreen.TotalMissed = _totalMissed;
        endingScreen.MaxCombo = _maxCombo;
        endingScreen.TimeSpentReloading = _timeSpentReloading;
        endingScreen.TotalScore = _score;
        endingScreen.DeadeyeKills = _deadeyeKills;
    }

    public void ShowHint(string text, Color color, string motion, bool stay = false)
    {
        _hintText.text = text;
        _hintText.color = color;
        _hintText.GetComponent<Animator>().SetTrigger(motion);
        if (stay)
        {
            _hintText.GetComponent<Animator>().SetTrigger("stay");
        }
    }
}
