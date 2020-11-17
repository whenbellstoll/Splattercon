using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

enum GameState { BoothPlacing, Playing, RoundEnd}

public class GameManager : MonoBehaviour
{
    [Header("Booth Stuff")]
    [SerializeField]
    private TextMeshProUGUI _boothPlacingText;
    [SerializeField]
    private GameObject _boothPrefab;
    [SerializeField]
    private GameObject _boothContainer;

    [Header("Attendee Stuff")]
    [SerializeField]
    private GameObject _attendeePrefab;
    [SerializeField]
    private GameObject _attendeeContainer;
    [SerializeField]
    private GameObject _startNode;
    [SerializeField]
    private GameObject _endNode;
    private List<GameObject> _attendeePath;
    private int _attendeeCount;
    private int _attendeesSpawned;
    private int _requiredAttendees;
    private int _attendeesPassed;
    private float _attendeeSpawnDelay;
    private float _attendeeSpawnTimer;

    [Header("Enemy Stuff")]
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject _spawnNode;
    private int _enemyCount;
    private int _enemiesSpawned;
    private float _enemySpawnDelay;
    private float _enemySpawnTimer;

    [Header("Spell Stuff")]
    [SerializeField]
    private GameObject _damageSpell;
    [SerializeField]
    private GameObject _slowSpell;

    [Header("Other stuff")]
    [SerializeField]
    private TextMeshProUGUI _roundText;
    [SerializeField]
    private TextMeshProUGUI _moneyText;
    [SerializeField]
    private TextMeshProUGUI _totalAttendeesText;
    [SerializeField]
    private TextMeshProUGUI _attendeesNeededText;
    [SerializeField]
    private TextMeshProUGUI _attendeesPassedText;

    private int _money = 0;

    private GameState _gameState;
    private Placing _placing;
    private int _round = 0;
    private int _boothsRemaining = 0;

    // Start is called before the first frame update
    void Start()
    {
        _placing = GetComponent<Placing>();
        SetGameState(GameState.BoothPlacing);
    }

    // Update is called once per frame
    void Update()
    {
        switch(_gameState)
        {
            case GameState.BoothPlacing:
                //Keep placing booths until all booths are placed
                if(!_placing.IsPlacing)
                {
                    _boothsRemaining--;
                    if(_boothsRemaining == 0)
                    {
                        _boothPlacingText.gameObject.SetActive(false);
                        SetGameState(GameState.Playing);
                    }
                    else
                    {
                        _boothPlacingText.text = "Booths: " + _boothsRemaining;
                        _placing.StartPlacing(_boothPrefab, _boothContainer);
                    }
                }
                break;
            case GameState.Playing:
                if(_attendeesSpawned < _attendeeCount)
                {
                    if(_attendeeSpawnTimer > _attendeeSpawnDelay)
                    {
                        //Spawn an attendee
                        GameObject attendee = Instantiate(_attendeePrefab, _attendeePath[0].transform.position, Quaternion.identity, _attendeeContainer.transform);
                        attendee.GetComponent<FollowPath>().SetNodes(_attendeePath);
                        attendee.GetComponent<FollowPath>().FinalNodeReached = AttendeePassed;
                        _attendeeSpawnTimer = 0;
                        _attendeesSpawned++;
                    }

                    _attendeeSpawnTimer += Time.deltaTime;
                }
                else if(_attendeeContainer.transform.childCount == 0 )
                {
                    SetGameState(GameState.BoothPlacing);
                }

                if (_enemiesSpawned < _enemyCount)
                {
                    if (_enemySpawnTimer > _enemySpawnDelay)
                    {
                        //Spawn an attendee
                        GameObject enemy = Instantiate(_enemyPrefab, _spawnNode.transform.position, Quaternion.identity, _enemyContainer.transform);

                        _enemySpawnTimer = 0;
                        _enemiesSpawned++;
                    }
                    
                    _enemySpawnTimer += Time.deltaTime;
                }
                break;
        }
    }

    private void SetGameState(GameState state)
    {
        _gameState = state;
        switch(state)
        {
            case GameState.BoothPlacing:
                ClearBooths();
                ClearEnemies();
                _round++;
                _boothsRemaining = 3 + _round / 2;
                _boothPlacingText.gameObject.SetActive(true);
                _boothPlacingText.text = "Booths: " + _boothsRemaining;
                _roundText.text = "Round: " + _round;
                _placing.StartPlacing(_boothPrefab, _boothContainer);
                //Set Attendee values
                _attendeeSpawnDelay = 0.5f;
                _attendeeSpawnTimer = _attendeeSpawnDelay;
                _attendeeCount = 8 + 2 * _round;
                _attendeesSpawned = 0;
                _requiredAttendees = (int)(_attendeeCount * 0.5f);
                _attendeesPassed = 0;

                //Set up enemy values
                _enemySpawnDelay = 4f - 3f * (1 / Mathf.Sqrt(_round));
                _enemySpawnTimer = _enemySpawnDelay - 2.0f;
                _enemyCount = 3 + 1 * (_round / 2);
                _enemiesSpawned = 0;

                UpdateAttendeeText();
                RandomizeSpawns();
                break;
            case GameState.Playing:
                Debug.Log("Now Playing");
                //Set up attendee values
                SetAttendeePath();

                break;
        }
    }

    //Gets the nearest booth to a position
    public Vector2 GetNearestBooth(Vector2 pos)
    {
        float closestDist = float.MaxValue;
        Vector2 closestBooth = Vector2.zero;
        for (int i = 0; i < _boothContainer.transform.childCount; i++)
        {
            Vector2 boothpos = _boothContainer.transform.GetChild(i).position;
            if (Vector2.SqrMagnitude(pos - boothpos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude(pos - boothpos);
                closestBooth = boothpos;
            }
        }
        if (closestBooth == Vector2.zero)
        {
            closestBooth = transform.position;
        }

        return closestBooth;
    }

    //Gets the nearest attendee to a position
    public Vector2 GetNearestAttendee(Vector2 pos)
    {
        float closestDist = float.MaxValue;
        Vector2 closestAttendee = Vector2.zero;
        for (int i = 0; i < _attendeeContainer.transform.childCount; i++)
        {
            Vector2 attendeepos = _attendeeContainer.transform.GetChild(i).position;
            if (Vector2.SqrMagnitude(pos - attendeepos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude(pos - attendeepos);
                closestAttendee = attendeepos;
            }
        }
        if (closestAttendee == Vector2.zero)
        {
            closestAttendee = transform.position;
        }

        return closestAttendee;
    }

    //Gets the nearest booth skipping a certain booth (used for finding the nearest booth to another booth
    public GameObject GetNearestBooth(Vector2 pos, List<GameObject> _vistedBooths)
    {
        float closestDist = float.MaxValue;
        Vector2 closestBooth = Vector2.zero;
        GameObject foundBooth = _boothContainer.transform.GetChild(0).gameObject;

        for (int i = 0; i < _boothContainer.transform.childCount; i++)
        {
            //Make sure booth hasn't been visited before
            if (_vistedBooths.Contains(_boothContainer.transform.GetChild(i).gameObject)) continue;

            Vector2 boothpos = _boothContainer.transform.GetChild(i).position;
            if (Vector2.SqrMagnitude(pos - boothpos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude(pos - boothpos);
                closestBooth = boothpos;
                foundBooth = _boothContainer.transform.GetChild(i).gameObject;
            }
        }
        if (closestBooth == Vector2.zero)
        {
            closestBooth = transform.position;
        }

        return foundBooth;
    }

    //Applies damage to all enemies in a range of a spell
    public void ApplyDamage(Vector2 pos, float dam)
    {
        HealthBar health;
        foreach (Transform enemy in _enemyContainer.transform)
        {
            if (Vector2.Distance(enemy.position, pos) < 1)
            {
                health = enemy.GetComponent<HealthBar>();
                health.TakeDamage(dam);
            }
        }
    }

    //Applies slow to all enemies in a range of a spell
    public void ApplySlow(Vector2 pos, float speed, float time)
    {
        Enemy movement;
        foreach (Transform enemy in _enemyContainer.transform)
        {
            if (Vector2.Distance(enemy.position, pos) < 1)
            {
                movement = enemy.GetComponent<Enemy>();
                movement.ChangeSpeed(speed, time);

            }
        }
    }

    //Sets the path for attendees
    private void SetAttendeePath()
    {
        _attendeePath = new List<GameObject>();
        List<int> _visted = new List<int>();
        _attendeePath.Add(_startNode);
        for (int i = 0; i < _boothContainer.transform.childCount; i++)
        {
            _attendeePath.Add(GetNearestBooth(_attendeePath[_attendeePath.Count - 1].transform.position, _attendeePath));
        }
        _attendeePath.Add(_endNode);
    }

    private void OnDrawGizmos()
    {
        if (_attendeePath != null && _attendeePath.Count > 1)
        {
            for(int i = 0; i < _attendeePath.Count - 1; i++)
            {
                Debug.DrawLine(_attendeePath[i].transform.position, _attendeePath[i + 1].transform.position);
            }
        }
    }

    //Called when attendee makes it to the end of the path
    private void AttendeePassed(GameObject attendee)
    {
        _money += 50;
        _moneyText.text = "Money: " + _money;
        _attendeesPassed++;
        Destroy(attendee);
        UpdateAttendeeText();
    }

    //Clears booths
    private void ClearBooths()
    {
        foreach(Transform booth in _boothContainer.transform)
        {
            Destroy(booth.gameObject);
        }
    }

    //Clears enemies
    private void ClearEnemies()
    {
        foreach(Transform enemy in _enemyContainer.transform)
        {
            Destroy(enemy.gameObject);
        }
    }

    //Update attendee text
    private void UpdateAttendeeText()
    {
        _totalAttendeesText.text = "Total Attendees: " + _attendeeCount;
        _attendeesNeededText.text = "Attendees Needed: " + _requiredAttendees;
        _attendeesPassedText.text = "Attendees Passed: " + _attendeesPassed;
        if(_attendeesPassed >= _requiredAttendees)
        {
            _attendeesPassedText.color = Color.green;
        }
        else
        {
            _attendeesPassedText.color = Color.red;
        }
    }


    //Randomize spawns
    private void RandomizeSpawns()
    {
        //Randomize start node
        if (Random.Range(0, 2) == 0)
        {
            if (Random.Range(0, 2) == 0)
                _startNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.max.y + 2);
            else
                _startNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.min.y - 2);
        }
        else
        {
            if (Random.Range(0, 2) == 0)
                _startNode.transform.position = new Vector2(_placing.ScreenBounds.min.x - 2, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
            else
                _startNode.transform.position = new Vector2(_placing.ScreenBounds.max.x + 2, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
        }

        //Randomize end node
        if (Random.Range(0, 2) == 0)
        {
            if (Random.Range(0, 2) == 0)
                _endNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.max.y + 2);
            else
                _endNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.min.y - 2);
        }
        else
        {
            if (Random.Range(0, 2) == 0)
                _endNode.transform.position = new Vector2(_placing.ScreenBounds.min.x - 2, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
            else
                _endNode.transform.position = new Vector2(_placing.ScreenBounds.max.x + 2, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
        }

        //Randomize enemy spawn node
        if (Random.Range(0, 2) == 0)
        {
            if (Random.Range(0, 2) == 0)
                _spawnNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.max.y + 2);
            else
                _spawnNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.min.y - 2);
        }
        else
        {
            if (Random.Range(0, 2) == 0)
                _spawnNode.transform.position = new Vector2(_placing.ScreenBounds.min.x - 2, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
            else
                _spawnNode.transform.position = new Vector2(_placing.ScreenBounds.max.x + 2, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
        }
    }
}
