using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnTargets : MonoBehaviour {

    private const int MIN_SPACE = 3;

    public GameObject _prefabTarget10;
    public GameObject _prefabTarget50;
    public Canvas _canvasEnd;
    public Text _hint;
    public AudioClip _soundWhistle;

    public int _maxPerWave = 3, _minPerWave = 1, _targetLifeTime = 3, _timeInSeconds = 60, _minSpacing = 1;
    public int _50sFrequency = 3;   // signifie que chaque cible a 1 chance sur 3 d'être bleue (valeur de 50 pts)
    public float xMin = -40, xMax = -5, yMin = -10, yMax = 5;
    public GameObject _mccree;

    private bool _nextWave = true, _nextSecond = true, _gameOver = false, _gameEnded = false;
    private int _timeLeftInSeconds;
    private int _targetsLeftInScene = 0;
    private Text _timer;
    private List<TargetHit> _targets = new List<TargetHit>();
    private Coroutine _lastWaveTimer = null;
    private AudioSource _audioSource;

    // Use this for initialization
    void Start () {
        _timeLeftInSeconds = _timeInSeconds;
        _timer = _mccree.transform.GetChild(0).transform.GetChild(3).GetComponent<Text>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // If timer is out we end the game
        if (_gameOver)
        {
            if (!_gameEnded)
            {
                _gameEnded = true;
                _mccree.GetComponent<Mccree>().ReportScoringData();
                _mccree.GetComponent<Mccree>().enabled = false;
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
            _targetsLeftInScene = Random.Range(_minPerWave, _maxPerWave);
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

                TargetHit targetScript = GenerateTarget(prefabUsed);

                while (!IsTargetPositionValid(targetScript))
                {
                    Debug.Log("Overlapping ! Regenerating...");
                    targetScript = GenerateTarget(prefabUsed);
                }

                targetScript._mccreeGO = _mccree;
                targetScript._targetsController = this;

                _targets.Add(targetScript);
            }

            _lastWaveTimer = StartCoroutine(WaveTimer());
        }
    }

    private bool IsTargetPositionValid(TargetHit target)
    {
        foreach (TargetHit t in _targets)
        {
            if (target.IsColliding(t))
                return false;
        }

        return true;
    }

    private TargetHit GenerateTarget(GameObject prefab)
    {
        float randX = Random.Range(xMin, xMax);
        float randY = Random.Range(yMin, yMax);

        GameObject instance = Instantiate(prefab, new Vector3(randX, randY, -15), Quaternion.identity);
        return instance.GetComponent<TargetHit>();
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1);
        _timeLeftInSeconds--;

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

        foreach(TargetHit target in _targets)
        {
            target.Disappear();            
        }

        _targets.Clear();

        _nextWave = true;
    }

    IEnumerator EndGame()
    {
        _hint.text = "All done !";
        _hint.GetComponent<Animator>().SetTrigger("appear");
        _audioSource.PlayOneShot(_soundWhistle); 

        yield return new WaitForSeconds(1.5f);
        _canvasEnd.enabled = true;
    }

    public void TargetDestroyed(TargetHit target)
    {
        _targets.Remove(target);
        _targetsLeftInScene--;
        if (_targetsLeftInScene == 0)
            _nextWave = true;
    }
}
