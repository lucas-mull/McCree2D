using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetsController : MonoBehaviour {

    private static Color HIGH_NOON_COLOR = new Color(0.925f, 0.71f, 0.573f);
    const int MAX_BIRDS_ON_SCENE = 2;

    public GameObject _prefabTarget10, _prefabTarget50, _prefabBird;
    public GameObject _player;
    public Canvas _canvasEnd;
    public Text _hint;
    public AudioClip _soundWhistle, _voiceDeadeye, _soundDeadeye;
    public Light _ambientLight;    

    public int _maxPerWave = 3, _minPerWave = 1, _targetLifeTime = 3, _timeInSeconds = 60;
    public int _50sFrequency = 3;   // signifie que chaque cible a 1 chance sur 3 d'être bleue (valeur de 50 pts)   
    public float _minSpacing = 1.0f;    

    private bool _nextWave = false, _nextSecond = false, _gameOver = false, _gameEnded = false;
    private int _timeLeftInSeconds;
    private int _targetsLeftInScene = 0;
    
    private Text _timer;
    private List<Target> _targets = new List<Target>();
    private List<Bird> _birds = new List<Bird>();
    private Transform _boundLeft, _boundRight;
    private Coroutine _lastWaveTimer = null;
    private AudioSource _audioSource;
    private Mccree _mccree;
    private float _xMin, _xMax, _yMin, _yMax;

    // Use this for initialization
    void Start () {
        _timeLeftInSeconds = _timeInSeconds;
        _timer = GameObject.Find("Timer").GetComponent<Text>();
        _timer.text = string.Format("{0}:{1:00}", _timeInSeconds / 60, _timeInSeconds % 60);
        _audioSource = GetComponent<AudioSource>();
        _mccree = _player.GetComponent<Mccree>();

        // On assigne les limites de la zone de spawn des cibles
        BoxCollider2D spawnZone = GetComponent<BoxCollider2D>();
        Vector3 minPoint = spawnZone.bounds.min;
        Vector3 maxPoint = spawnZone.bounds.max;

        _boundLeft = transform.FindChild("boundL");
        _boundRight = transform.FindChild("boundR");

        _xMin = minPoint.x;
        _yMin = minPoint.y;
        _xMax = maxPoint.x;
        _yMax = maxPoint.y;     

        // On lance le compte à rebours
        StartCoroutine(Countdown());
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
            NextWave();
        }
        else if (Input.GetKeyDown(KeyCode.A) && _mccree.IsDeadeyeReady() && _targets.Count > 0)
        {
            StopCoroutine(_lastWaveTimer);
            StartCoroutine(Deadeye());
        }
        else if (Input.GetKeyDown(KeyCode.T) && _targets.Count > 0)
        {
            StopCoroutine(_lastWaveTimer);
            StartCoroutine(Deadeye());
        }

    }

    private void NextWave()
    {
        if (_lastWaveTimer != null)
            StopCoroutine(_lastWaveTimer);

        _nextWave = false;

        SpawnBird();
        SpawnTargets();
    }

    private void SpawnBird()
    {
        // We make the already on scene birds attack if they're in range of the player
        foreach (Bird bird in _birds)
        {
            float distance = Vector3.Distance(bird.transform.position, _mccree.transform.position);
            Debug.Log("Distance : " + distance);
            if (distance <= 15 && !bird.HasAttacked())
            {
                bird.Attack();
            }
        }

        // Bird spawn
        if (_birds.Count < MAX_BIRDS_ON_SCENE && _timeLeftInSeconds % 10 <= 3 && Random.Range(0, 3) == 2)
        {
            Debug.Log("Spawning bird !");

            Transform start, finish;
            Bird bird;
            float randY = Random.Range(_yMin, _yMax);

            if (Random.Range(0, 2) == 0)
            {
                start = _boundLeft;
                finish = _boundRight;

                start.position = new Vector3(start.position.x, randY);
                finish.position = new Vector3(finish.position.x, randY);

                bird = Instantiate(_prefabBird, start.position, Quaternion.identity).GetComponentInChildren<Bird>();
                bird.Flip(true);
                bird.SetDestination(finish);
            }
            else
            {
                start = _boundRight;
                finish = _boundLeft;

                start.position = new Vector3(start.position.x, randY);
                finish.position = new Vector3(finish.position.x, randY);

                bird = Instantiate(_prefabBird, _boundRight.position, Quaternion.identity).GetComponentInChildren<Bird>();
                bird.SetDestination(_boundLeft);
            }

            bird._mccree = _mccree;
            bird._birdController = this;

            _birds.Add(bird);
        }        
    }

    private void SpawnTargets()
    {
        // Targets spawn
        _targetsLeftInScene = Random.Range(_minPerWave, _maxPerWave + 1);
        for (int i = 0; i < _targetsLeftInScene; i++)
        {
            GameObject prefabUsed = _prefabTarget10;
            int valueFor50 = _50sFrequency - 1;
            if (valueFor50 <= 0)
                valueFor50 = 1;

            if (Random.Range(1, _50sFrequency) == valueFor50)
            {
                prefabUsed = _prefabTarget50;
            }

            float randX = Random.Range(_xMin, _xMax);
            float randY = Random.Range(_yMin, _yMax);

            while (!IsPositionValid(randX, randY))
            {
                Debug.Log("Overlapping ! Regenerating...");
                randX = Random.Range(_xMin, _xMax);
                randY = Random.Range(_yMin, _yMax);

                if (Input.GetKeyDown(KeyCode.Space))
                    break;
            }

            Target targetScript = GenerateTarget(prefabUsed, randX, randY);

            targetScript._player = _player;
            targetScript._targetsController = this;

            _targets.Add(targetScript);
        }

        if (_timeLeftInSeconds % 5 <= 3)
            _targets[Random.Range(0, _targets.Count)].PerformMovement(Target.MOVEMENT_VERTICAL);

        _lastWaveTimer = StartCoroutine(WaveTimer());
    }

    private bool IsPositionValid(float x, float y)
    {
        bool xValid = false, yValid = false;
        foreach (Target t in _targets)
        {
            xValid = Mathf.Abs(t.transform.position.x - x) > _minSpacing;
            yValid = Mathf.Abs(t.transform.position.y - y) > _minSpacing;

            if (!xValid && !yValid)
                return false;
        }

        return true;
    }

    private Target GenerateTarget(GameObject prefab, float x, float y)
    { 
        GameObject instance = Instantiate(prefab, new Vector3(x, y, -15), Quaternion.identity);
        return instance.GetComponentInChildren<Target>();
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

        _timer.text = string.Format("{0}:{1:00}", _timeLeftInSeconds / 60, _timeLeftInSeconds % 60);
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

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 5; i++)
        {
            _mccree.ShowHint("" + (5 - i), Color.white, "appear");
            yield return new WaitForSeconds(1.0f);
        }

        _mccree.ShowHint("GO !", Color.white, "appear");

        _nextSecond = true;
        _nextWave = true;
    }

    IEnumerator EndGame()
    {
        _mccree.ReportScoringData();
        _mccree.enabled = false;
        foreach (Target target in _targets)
        {
            target.SetEnabled(false);
        }

        foreach(Bird bird in _birds)
        {
            bird.SetEnabled(false);
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

        int targetsCount = _targets.Count, birdsCount = _birds.Count;

        //Debug.Log(toKill + " targets for deadeye");

        foreach(Target t in _targets)
        {
            t.Deadeye();
        }

        foreach(Bird bird in _birds)
        {
            bird.Deadeye();
        }

        _audioSource.PlayOneShot(_soundDeadeye);
        yield return new WaitForSeconds(0.3f);

        _audioSource.PlayOneShot(_voiceDeadeye);
        yield return new WaitForSeconds(_voiceDeadeye.length);

        _mccree.Deadeye(targetsCount + birdsCount);

        for(int i = 0; i < targetsCount; i++)
        {
            Target target = _targets[0]; // Next target is always first index (since last has been destroyed --> everything shifts to left)

            if (target.IsEnabled())
            {
                target.Destroy();
                yield return new WaitForSeconds(0.1f);
            }
        }

        for (int i = 0; i < birdsCount; i++)
        {
            Bird bird = _birds[0];

            bird.Kill();
            yield return new WaitForSeconds(0.1f);
        }
        

        _ambientLight.color = Color.white;

        yield return new WaitForSeconds(1.0f);

        _nextWave = true;
    }

    public void TargetDestroyed(Target target)
    {
        _targets.Remove(target);
    }

    public void BirdKilled(Bird bird)
    {
        _birds.Remove(bird);
    }
}
