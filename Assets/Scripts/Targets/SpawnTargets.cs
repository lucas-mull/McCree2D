using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnTargets : MonoBehaviour {

    private const int MIN_SPACE = 3;
    private static Color HIGH_NOON_COLOR = new Color(0.925f, 0.71f, 0.573f);

    public GameObject _prefabTarget10;
    public GameObject _prefabTarget50;
    public Canvas _canvasEnd;
    public Text _hint;
    public AudioClip _soundWhistle, _voiceDeadeye, _soundDeadeye;
    public Light _ambientLight;

    public int _maxPerWave = 3, _minPerWave = 1, _targetLifeTime = 3, _timeInSeconds = 60, _minSpacing = 1;
    public int _50sFrequency = 3;   // signifie que chaque cible a 1 chance sur 3 d'être bleue (valeur de 50 pts)
    public float xMin = -40, xMax = -5, yMin = -10, yMax = 5;
    public GameObject _mccreeGO;

    private bool _nextWave = true, _nextSecond = true, _gameOver = false, _gameEnded = false;
    private int _timeLeftInSeconds;
    private int _targetsLeftInScene = 0;
    
    private Text _timer;
    private List<Target> _targets = new List<Target>();
    private Coroutine _lastWaveTimer = null;
    private AudioSource _audioSource;
    private Mccree _mccree;

    // Use this for initialization
    void Start () {
        _timeLeftInSeconds = _timeInSeconds;
        _timer = GameObject.Find("Timer").GetComponent<Text>();
        _timer.text = "" + _timeInSeconds;
        _audioSource = GetComponent<AudioSource>();
        _mccree = _mccreeGO.GetComponent<Mccree>();
    }

    private void Update()
    {
        // If timer is out we end the game
        if (_gameOver)
        {
            if (!_gameEnded)
            {
                _gameEnded = true;                
                StartCoroutine(EndGame());
            }

            return;
        }

        if (_nextSecond)
        {
            _nextSecond = false;
            StartCoroutine(Timer());
        }            
        

        if (_nextWave)
        {
            if (_lastWaveTimer != null)
                StopCoroutine(_lastWaveTimer);

            _nextWave = false;
            _targetsLeftInScene = Random.Range(_minPerWave, _maxPerWave + 1);
            for (int i = 0; i < _targetsLeftInScene; i++)
            {
                GameObject prefabUsed = _prefabTarget10;
                int valueFor50 = _50sFrequency - 1;
                if (valueFor50 == 0)
                    valueFor50 = 1;

                if (Random.Range(1, _50sFrequency) == valueFor50)
                {
                    prefabUsed = _prefabTarget50;
                }

                Target targetScript = GenerateTarget(prefabUsed);

                while (!IsTargetPositionValid(targetScript))
                {
                    Debug.Log("Overlapping ! Regenerating...");
                    targetScript = GenerateTarget(prefabUsed);
                }

                targetScript._mccreeGO = _mccreeGO;
                targetScript._targetsController = this;

                _targets.Add(targetScript);
            }

            _lastWaveTimer = StartCoroutine(WaveTimer());
        }
        else if (Input.GetKeyDown(KeyCode.A) && _mccree.IsDeadeyeReady() && _targets.Count > 0)
        {
            StopCoroutine(_lastWaveTimer);
            StartCoroutine(Deadeye());
        }
        
    }

    private bool IsTargetPositionValid(Target target)
    {
        foreach (Target t in _targets)
        {
            if (target.IsColliding(t))
                return false;
        }

        return true;
    }

    private Target GenerateTarget(GameObject prefab)
    {
        float randX = Random.Range(xMin, xMax);
        float randY = Random.Range(yMin, yMax);

        GameObject instance = Instantiate(prefab, new Vector3(randX, randY, -15), Quaternion.identity);
        return instance.GetComponent<Target>();
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1);
        _timeLeftInSeconds--;

        _mccree.IncrementDeadeyePercent();

        if (_timeLeftInSeconds <= 10)
        {
            _timer.color = Color.red;
        }

        _timer.text = "" + _timeLeftInSeconds;
        _nextSecond = true;
        _gameOver = _timeLeftInSeconds == 0;
    }

    IEnumerator WaveTimer()
    {
        yield return new WaitForSeconds(_targetLifeTime);

        foreach (Target target in _targets)
        {
            target.Disappear();
        }

        _targets.Clear();

        //Debug.Log("Targets have been cleared !");

        _nextWave = true;
    }

    IEnumerator EndGame()
    {
        _mccree.ReportScoringData();
        _mccree.enabled = false;
        foreach (Target target in _targets)
        {
            target.enabled = false;
        }

        _mccree.ShowHint("ALL DONE !", Color.white, "appear", true);
        _audioSource.PlayOneShot(_soundWhistle); 

        yield return new WaitForSeconds(1.5f);
        _canvasEnd.enabled = true;
    }

    IEnumerator Deadeye()
    {
        _mccree.NotifyDeadeyeHasStarted();

        _ambientLight.color = HIGH_NOON_COLOR;        

        // Make sure no new wave starts
        _nextWave = false;

        int toKill = _targets.Count;

        //Debug.Log(toKill + " targets for deadeye");

        foreach(Target t in _targets)
        {
            t.Deadeye();
        }

        _audioSource.PlayOneShot(_soundDeadeye);
        yield return new WaitForSeconds(0.3f);

        _audioSource.PlayOneShot(_voiceDeadeye);
        yield return new WaitForSeconds(_voiceDeadeye.length);

        _mccree.Deadeye(toKill);

        for(int i = 0; i < toKill; i++)
        {
            Target target = _targets[0]; // Next target is always first index (since last has been destroyed --> everything shifts to left)

            if (target.IsEnabled())
            {
                target.Destroy();
                yield return new WaitForSeconds(0.1f);
            }
        }

        _ambientLight.color = Color.white;

        yield return new WaitForSeconds(1.0f);

        _nextWave = true;
    }

    public void TargetDestroyed(Target target)
    {
        _targets.Remove(target);
    }
}
