﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

enum GameState { BoothPlacing, Playing, RoundEnd}

public class GameManager : MonoBehaviour
{
	
	public static bool PAUSED = false;
	
    [Header("Booth Stuff")]
    [SerializeField]
    private GameObject _vampireBoothPrefab;
    [SerializeField]
    private GameObject _vampireBoothContainer;
    [SerializeField]
    private GameObject _ghostBoothPrefab;
    [SerializeField]
    private GameObject _ghostBoothContainer;

    [Header("Attendee Stuff")]
    [SerializeField]
    private GameObject _vampireAttendeePrefab;
    [SerializeField]
    private GameObject _vampireAttendeeContainer;
    public GameObject VampireAttendeeContainer => _vampireAttendeeContainer;
    [SerializeField]
    private GameObject _ghostAttendeePrefab;
    [SerializeField]
    private GameObject _ghostAttendeeContainer;
    public GameObject GhostAttendeeContainer => _ghostAttendeeContainer;
    [SerializeField]
    private GameObject _startNode;
    [SerializeField]
    private GameObject _endNode;
    private List<GameObject> _vampireAttendeePath;
    private List<GameObject> _ghostAttendeePath;
    private int _attendeeCount;
    private int _attendeesSpawned;
    private int _requiredAttendees;
    private int _attendeesPassed;
    private float _attendeeSpawnDelay;
    private float _attendeeSpawnTimer;
    private int _ghostAttendees;
    private int _vampireAttendees;

    [Header("Enemy Stuff")]
    [SerializeField]
    private GameObject _vampireEnemyPrefab;
    [SerializeField]
    private GameObject _ghostEnemyPrefab;
    [SerializeField]
    private GameObject _enemyContainer;
    [SerializeField]
    private GameObject _spawnNode;
    private int _enemyCount;
    private int _enemiesSpawned;
    private float _enemySpawnDelay;
    private float _enemySpawnTimer;
    private int _vampireEnemies;
    private int _ghostEnemies;
    private float _spellTime;

    [Header("Trap Stuff")]
    [SerializeField]
    private GameObject _bearTrapPrefab;
    [SerializeField]
    private GameObject _cannonTrapPrefab;
    [SerializeField]
    private GameObject _trapContainer;


    //[Header("Spell Stuff")]
    //[SerializeField]
    //private GameObject _damageSpell;
    //[SerializeField]
    //private GameObject _slowSpell;

    [Header("Tutorial Stuff")]
    [SerializeField]
    private GameObject _tutorial;
    [SerializeField]
    private GameObject _tutorialTextOne;
    [SerializeField]
    private GameObject _tutorialTextTwo;


    [Header("Other stuff")]
    [SerializeField]
    private Text _roundText;
    [SerializeField]
    private Text _attendeeText;
	[SerializeField]
    private RectTransform _attendeeBar;
	[SerializeField]
    private int _maxAttendeeBarSize;
	[SerializeField]
	private float _attendeeBarInitialX; 
    //[SerializeField]
    //private Text _attendeesNeededText;
    //[SerializeField]
    //private Text _attendeesPassedText;
	[SerializeField]
    private GameObject _pauseMenu;
    [SerializeField]
    private SceneData _sceneDataObject;

    private AudioSource _audioSource;

    private GameState _gameState;
    private Placing _placing;
    private Selection _select;
    private CastSpell _castSpell;
    private int _round = -2;
    private int _boothsRemaining = 0;
    private int _trapsRemaining = 0;

    private const int MAX_BOOTHS = 7;

    private const float MIN_SPAWN_NODE_DISTANCE = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        _placing = GetComponent<Placing>();
        _select = GetComponent<Selection>();
        _castSpell = GetComponent<CastSpell>();
        _select.OnSelectionChange = OnSelectionChange;
        _audioSource = GetComponent<AudioSource>();

        SetGameState(GameState.BoothPlacing);

        _spellTime = 3;
        _audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PAUSED)
        {
            switch (_gameState)
            {
                case GameState.BoothPlacing:

                    _select.UpdatePlayMask();

                    
                    if( _round < 1 && _select.AllZero(SelectionGroups.BOOTH) && _select.PlayButtonClicked() )
                    {
                        _tutorialTextOne.SetActive(false);
                        _tutorialTextTwo.SetActive(false);
                    }

                    if(_select.PlayButtonClicked() && _select.AllZero(SelectionGroups.BOOTH))
                    {
                        SetGameState(GameState.Playing);

                        _roundText.transform.parent.gameObject.SetActive(false);

                        _placing.CancelPlacing();

                        _select.HideButtons();
                    }
                    
                    

                    break;
                case GameState.Playing:
                    //Checks if you can cast a spell
                    if (Input.GetMouseButtonDown(1) && !_select.IsZero())
                    {
                        _castSpell.Cast(_select.GetCurrentSelectionName());
                        _spellTime = 0;
                        _select.DecrementCurrentSelection();
                    }
                    _spellTime += Time.deltaTime;

                    if (_attendeesSpawned < _attendeeCount)
                    {
                        if (_attendeeSpawnTimer > _attendeeSpawnDelay)
                        {
                            //Spawn an attendee
                            if (_vampireAttendees <= 0)
                            {
                                GameObject attendee = Instantiate(_ghostAttendeePrefab, _ghostAttendeePath[0].transform.position, Quaternion.identity, _ghostAttendeeContainer.transform);
                                attendee.GetComponent<FollowPath>().SetNodes(_ghostAttendeePath);
                                attendee.GetComponent<FollowPath>().FinalNodeReached = AttendeePassed;
                                _ghostAttendees--;
                            }
                            else if (_ghostAttendees <= 0 || Random.Range(0, 2) == 0)
                            {
                                GameObject attendee = Instantiate(_vampireAttendeePrefab, _vampireAttendeePath[0].transform.position, Quaternion.identity, _vampireAttendeeContainer.transform);
                                attendee.GetComponent<FollowPath>().SetNodes(_vampireAttendeePath);
                                attendee.GetComponent<FollowPath>().FinalNodeReached = AttendeePassed;
                                _vampireAttendees--;
                            }
                            else
                            {
                                GameObject attendee = Instantiate(_ghostAttendeePrefab, _ghostAttendeePath[0].transform.position, Quaternion.identity, _ghostAttendeeContainer.transform);
                                attendee.GetComponent<FollowPath>().SetNodes(_ghostAttendeePath);
                                attendee.GetComponent<FollowPath>().FinalNodeReached = AttendeePassed;
                                _ghostAttendees--;
                            }
                            _attendeeSpawnTimer = 0;
                            _attendeesSpawned++;
                        }

                        _attendeeSpawnTimer += Time.deltaTime;
                    }
                    else if (!AttendeesActive())
                    {

                        if(_attendeesPassed < _requiredAttendees)
                        {

                            _sceneDataObject.SetData(_round);
                            SceneManager.LoadScene("EndMenu", LoadSceneMode.Single);

                        }
                        else
                        {
                            SetGameState(GameState.BoothPlacing);

                            _roundText.transform.parent.gameObject.SetActive(true);
                        }

                    }

                    if (_enemiesSpawned < _enemyCount)
                    {
                        if (_enemySpawnTimer > _enemySpawnDelay)
                        {
                            //Spawn an enemy
                            //Spawn an attendee
                            if (_vampireEnemies <= 0)
                            {
                                GameObject enemy = Instantiate(_ghostEnemyPrefab, _spawnNode.transform.position, Quaternion.identity, _enemyContainer.transform);
                                enemy.GetComponent<Enemy>().SetProgressionValues(Mathf.Max(0,_round) * 0.3f, Mathf.Max(0, _round) * 5);
                                if( _round < 1 )
                                {
                                    enemy.GetComponent<Enemy>().SetProgressionValues( 0.3f, 5);
                                }
                                _ghostEnemies--;
                            }
                            else if (_ghostEnemies <= 0 || Random.Range(0, 2) == 0)
                            {
                                GameObject enemy = Instantiate(_vampireEnemyPrefab, _spawnNode.transform.position, Quaternion.identity, _enemyContainer.transform);
                                enemy.GetComponent<Enemy>().SetProgressionValues(Mathf.Max(0, _round) * 0.2f, Mathf.Max(0, _round) * 8);
                                if (_round < 1)
                                {
                                    enemy.GetComponent<Enemy>().SetProgressionValues(0.2f, 8);
                                }
                                _vampireEnemies--;
                            }
                            else
                            {
                                GameObject enemy = Instantiate(_ghostEnemyPrefab, _spawnNode.transform.position, Quaternion.identity, _enemyContainer.transform);
                                enemy.GetComponent<Enemy>().SetProgressionValues(Mathf.Max(0, _round) * 0.3f, Mathf.Max(0, _round) * 5);
                                if (_round < 1)
                                {
                                    enemy.GetComponent<Enemy>().SetProgressionValues(0.3f, 5);
                                }
                                _ghostEnemies--;
                            }

                            _enemySpawnTimer = 0;
                            _enemiesSpawned++;
                        }

                        _enemySpawnTimer += Time.deltaTime;
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !PAUSED)
        {
            PauseGame();
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && PAUSED)
        {
            ResumeGame();
        }

    }

    private bool AttendeesActive()
    {
        return _vampireAttendeeContainer.transform.childCount > 0 || _ghostAttendeeContainer.transform.childCount > 0;
    }

    private void ObjectPlaced()
    {
        _select.DecrementCurrentSelection();

        if (!_select.IsZero())
        {
            switch (_select.GetCurrentSelectionName())
            {
                case "Vampire Booth":
                    _placing.StartPlacing(_vampireBoothPrefab, _vampireBoothContainer, ObjectPlaced, CanPlaceBooth);
                    break;
                case "Ghost Booth":
                    _placing.StartPlacing(_ghostBoothPrefab, _ghostBoothContainer, ObjectPlaced, CanPlaceBooth);
                    break;
                case "Bear Trap":
                    if (!_select.IsZero())
                    {
                        _placing.StartPlacing(_bearTrapPrefab, _trapContainer, ObjectPlaced, CanPlaceTrap);
                    }
                    //Currently can not stop placing traps without clicking a button so you can place infinite traps
                    else
                    {
                        _placing.StartPlacing(_bearTrapPrefab, _trapContainer, ObjectPlaced, CanPlaceTrap);
                    }
                    break;
                case "Cannon Trap":
                    _placing.StartPlacing(_cannonTrapPrefab, _trapContainer, ObjectPlaced, CanPlaceTrap);
                    break;
                default:
                    //_placing.StartPlacing(_boothPrefab, _boothContainer, CanPlaceBooth);
                    break;
            }
        }
    }

    private void OnSelectionChange()
    {
        if(_gameState == GameState.BoothPlacing)
        {
            if (!_select.IsZero() && _select.GetCurrentSelectionGroup() != SelectionGroups.SPELL)
            {
                switch (_select.GetCurrentSelectionName())
                {
                    case "Vampire Booth":
                        _placing.StartPlacing(_vampireBoothPrefab, _vampireBoothContainer, ObjectPlaced, CanPlaceBooth);
                        break;
                    case "Ghost Booth":
                        _placing.StartPlacing(_ghostBoothPrefab, _ghostBoothContainer, ObjectPlaced, CanPlaceBooth);
                        break;
                    case "Bear Trap":
                        if (!_select.IsZero())
                        {
                            _placing.StartPlacing(_bearTrapPrefab, _trapContainer, ObjectPlaced, CanPlaceTrap);
                        }
                        //Currently can not stop placing traps without clicking a button so you can place infinite traps
                        else
                        {
                            _placing.StartPlacing(_bearTrapPrefab, _trapContainer, ObjectPlaced, CanPlaceTrap);
                        }
                        break;
                    case "Cannon Trap":
                        _placing.StartPlacing(_cannonTrapPrefab, _trapContainer, ObjectPlaced, CanPlaceTrap);
                        break;
                    default:
                        //_placing.StartPlacing(_boothPrefab, _boothContainer, CanPlaceBooth);
                        break;
                }
            }
            else
            {
                _placing.CancelPlacing();
            }
        }
    }

    public void PauseGame(){
        _audioSource.volume = .01f;
		_pauseMenu.SetActive(true);
        _placing.CancelPlacing();
        PAUSED = true;
		Time.timeScale = 0f;
	}


    public void ResumeGame(){
        _audioSource.volume = .05f;
        _pauseMenu.SetActive(false);
        _placing.ResumePlacing();
        PAUSED = false;
		Time.timeScale = 1.0f;
	}

    public void SkipDemo(){
        _tutorial.SetActive(false);
        if (_round == -1)
            _select.AddMoney(150);
        else
            _select.AddMoney(75);
        _round = 0;
        
        _tutorialTextOne.SetActive(false);
        _tutorialTextTwo.SetActive(false);
        
        SetGameState(GameState.BoothPlacing);
    }

    public int GetRound(){
        return _round;
    }


    private bool CanPlaceBooth(Vector2 pos)
    {
        for(int i = 0; i < _vampireBoothContainer.transform.childCount; i++)
        {
            if(Vector2.SqrMagnitude(pos - (Vector2)_vampireBoothContainer.transform.GetChild(i).transform.position) <= 1.0f)
            {
                return true;
            }
        }
        for (int i = 0; i < _ghostBoothContainer.transform.childCount; i++)
        {
            if (Vector2.SqrMagnitude(pos - (Vector2)_ghostBoothContainer.transform.GetChild(i).transform.position) <= 1.0f)
            {
                return true;
            }
        }
        for (int i = 0; i < _trapContainer.transform.childCount; i++)
        {
            if (Vector2.SqrMagnitude(pos - (Vector2)_trapContainer.transform.GetChild(i).transform.position) <= 0.1f * 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    private bool CanPlaceTrap(Vector2 pos)
    {
        for (int i = 0; i < _vampireBoothContainer.transform.childCount; i++)
        {
            if (Vector2.SqrMagnitude(pos - (Vector2)_vampireBoothContainer.transform.GetChild(i).transform.position) <= 0.1f * 0.1f)
            {
                return true;
            }
        }
        for (int i = 0; i < _ghostBoothContainer.transform.childCount; i++)
        {
            if (Vector2.SqrMagnitude(pos - (Vector2)_ghostBoothContainer.transform.GetChild(i).transform.position) <= 0.1f * 0.1f)
            {
                return true;
            }
        }
        return false;
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
                
                if( _round == 1)
                {
                    _tutorial.SetActive(false);
                }
                
                if( _round == 0 )
                {
                    _tutorialTextTwo.SetActive(true);
                }

                if (_round <= 0)
                    _tutorial.SetActive(true);

                //Set booth values
                if (_round > 0){
                    _select.SetAmount(SelectionGroups.BOOTH, "Vampire Booth", Mathf.Min(MAX_BOOTHS, 1 + _round / 2));
                    _select.SetAmount(SelectionGroups.BOOTH, "Ghost Booth", Mathf.Min(MAX_BOOTHS, 1 + _round / 3));
                }
                else{
                    _select.SetAmount(SelectionGroups.BOOTH, "Vampire Booth", Mathf.Min(MAX_BOOTHS, 1));
                    _select.SetAmount(SelectionGroups.BOOTH, "Ghost Booth", Mathf.Min(MAX_BOOTHS, 1));
                }
                _select.SetAmount(SelectionGroups.SPELL, "Fire Spell", 20);
                _select.SetAmount(SelectionGroups.SPELL, "Snow Spell", 15);

                _select.ShowButtons();

                _roundText.text = "Round " + _round;
                if (_round <= 0)
                    _roundText.text = "Tutorial";
                OnSelectionChange();
                //Set Attendee values
                _attendeeSpawnDelay = 0.5f;
                _attendeeSpawnTimer = _attendeeSpawnDelay;
                // conditional for tutorial
                if (_round > 0){
                    _attendeeCount = 8 + 2 * Mathf.Max(0, _round);
                }
                else{ //tutorial
                    _attendeeCount = 8;
                }

                _vampireAttendees = _attendeeCount / 2;
                _ghostAttendees = _attendeeCount - _vampireAttendees;
                _attendeesSpawned = 0;
                _requiredAttendees = (int)(_attendeeCount * 0.5f);
                _attendeesPassed = 0;

                //Set up enemy values
                if (_round > 0)
                {
                    _enemySpawnDelay = 2f * (1 / Mathf.Sqrt(_round));
                    _enemySpawnTimer = _enemySpawnDelay - 2.0f;
                    _enemyCount = 3 + 1 * Mathf.Max(0, _round);
                    _enemiesSpawned = 0;
                    _vampireEnemies = _enemyCount / 2;
                    _ghostEnemies = _enemyCount - _vampireAttendees;
                }
                else if( _round == -1 )
                {
                    _enemySpawnDelay = 2f;
                    _enemySpawnTimer = _enemySpawnDelay - 2.0f;
                    _enemyCount = 3;
                    _enemiesSpawned = 0;
                    _vampireEnemies = _enemyCount;
                    _ghostEnemies = 0;
                }
                else if( _round == 0 )
                {
                    _enemySpawnDelay = 2f;
                    _enemySpawnTimer = _enemySpawnDelay - 2.0f;
                    _enemyCount = 4;
                    _enemiesSpawned = 0;
                    _vampireEnemies = _enemyCount / 2;
                    _ghostEnemies = _enemyCount - _vampireAttendees;
                }
                //Set up Trap values
                _select.SetAmount(SelectionGroups.TRAP, "Bear Trap", 100);
                _select.SetAmount(SelectionGroups.TRAP, "Cannon Trap", 250);

                UpdateAttendeeText();
                RandomizeSpawns();

                break;
            case GameState.Playing:
                Debug.Log("Now Playing");
                //Set up attendee values
                if(_round <= 0)
                    _tutorial.SetActive(false);
                SetAttendeePath();

                break;
        }
    }

    //Gets the nearest booth to a position
    public Vector2 GetNearestBooth(Vector2 pos)
    {
        float closestDist = float.MaxValue;
        Vector2 closestBooth = Vector2.zero;
        for (int i = 0; i < _vampireBoothContainer.transform.childCount; i++)
        {
            Vector2 boothpos = _vampireBoothContainer.transform.GetChild(i).position;
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
    public Vector2 GetNearestAttendee(Vector2 pos, GameObject container)
    {
        float closestDist = float.MaxValue;
        Vector2 closestAttendee = Vector2.zero;
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Vector2 attendeepos = container.transform.GetChild(i).position;
            if (!_placing.ScreenBounds.Contains(attendeepos)) continue;
            if (Vector2.SqrMagnitude(pos - attendeepos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude(pos - attendeepos);
                closestAttendee = attendeepos;
            }
        }
        if (closestAttendee == Vector2.zero)
        {
            closestAttendee = _spawnNode.transform.position;
        }

        return closestAttendee;
    }

    //Gets the nearest booth skipping a certain booth (used for finding the nearest booth to another booth
    public GameObject GetNearestBooth(Vector2 pos, GameObject container, List<GameObject> _vistedBooths)
    {
        float closestDist = float.MaxValue;
        Vector2 closestBooth = Vector2.zero;
        GameObject foundBooth = container.transform.GetChild(0).gameObject;

        for (int i = 0; i < container.transform.childCount; i++)
        {
            //Make sure booth hasn't been visited before
            if (_vistedBooths.Contains(container.transform.GetChild(i).gameObject)) continue;

            Vector2 boothpos = container.transform.GetChild(i).position;
            if (Vector2.SqrMagnitude(pos - boothpos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude(pos - boothpos);
                closestBooth = boothpos;
                foundBooth = container.transform.GetChild(i).gameObject;
            }
        }
        if (closestBooth == Vector2.zero)
        {
            closestBooth = transform.position;
        }

        return foundBooth;
    }

    public Vector2 GetNearestEnemy(Vector2 pos)
    {
        float closestDist = float.MaxValue;
        Vector2 closestEnemy = Vector2.zero;
        for (int i = 0; i < _enemyContainer.transform.childCount; i++)
        {
            Vector2 enemypos = _enemyContainer.transform.GetChild(i).position;
            if (Vector2.SqrMagnitude(pos - enemypos) < closestDist)
            {
                closestDist = Vector2.SqrMagnitude(pos - enemypos);
                closestEnemy = enemypos;
            }
        }
        if (closestEnemy == Vector2.zero)
        {
            closestEnemy = Vector2.zero;
        }

        return closestEnemy;
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
    public void ApplySlow(Vector2 pos, float speed)
    {
        Enemy movement;
        foreach (Transform enemy in _enemyContainer.transform)
        {
            if (Vector2.Distance(enemy.position, pos) < 1.5f)
            {
                movement = enemy.GetComponent<Enemy>();
                movement.ChangeSpeed(speed);

            }
        }
    }

    //Bear trap script
    public bool BearTrap(Vector2 pos, float speed)
    {
        HealthBar health;
        Enemy current = null;
        
        
        foreach (Transform enemy in _enemyContainer.transform)
        {
            if (current == null || (Vector2.Distance(enemy.position, pos) < Vector2.Distance(current.transform.position, pos)))
            {
                current = enemy.GetComponent<Enemy>();
            }
        }

        if (current != null && Vector2.Distance(current.transform.position, pos) < .5f)
        {
            current.ChangeSpeed(speed);
            current.transform.position = new Vector2(pos.x, pos.y);
            //health = current.GetComponent<HealthBar>();
            //health.TakeDamage(50);
            return true;
        }

        return false;
    }

    //Sets the path for attendees
    private void SetAttendeePath()
    {
        //Set vampire attendee path
        _vampireAttendeePath = new List<GameObject>();
        _vampireAttendeePath.Add(_startNode);
        for (int i = 0; i < _vampireBoothContainer.transform.childCount; i++)
        {
            _vampireAttendeePath.Add(GetNearestBooth(_vampireAttendeePath[_vampireAttendeePath.Count - 1].transform.position, _vampireBoothContainer, _vampireAttendeePath));
        }
        _vampireAttendeePath.Add(_endNode);

        //Set ghost attendee path
        _ghostAttendeePath = new List<GameObject>();
        _ghostAttendeePath.Add(_startNode);
        for (int i = 0; i < _ghostBoothContainer.transform.childCount; i++)
        {
            _ghostAttendeePath.Add(GetNearestBooth(_ghostAttendeePath[_ghostAttendeePath.Count - 1].transform.position, _ghostBoothContainer, _ghostAttendeePath));
        }
        _ghostAttendeePath.Add(_endNode);
    }

    /*private void OnDrawGizmos()
    {
        if (_vampireAttendeePath != null && _vampireAttendeePath.Count > 1)
        {
            for(int i = 0; i < _vampireAttendeePath.Count - 1; i++)
            {
                Debug.DrawLine(_vampireAttendeePath[i].transform.position, _vampireAttendeePath[i + 1].transform.position);
            }
        }
    }*/

    //Called when attendee makes it to the end of the path
    private void AttendeePassed(GameObject attendee)
    {
        _select.AddMoney(25);
        _attendeesPassed++;
        Destroy(attendee);
        UpdateAttendeeText();
    }

    //Clears booths
    private void ClearBooths()
    {
        foreach(Transform booth in _vampireBoothContainer.transform)
        {
            Destroy(booth.gameObject);
        }
        foreach (Transform booth in _ghostBoothContainer.transform)
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
		_attendeeText.text = _attendeesPassed + " / " + _requiredAttendees;
		
        if(_attendeesPassed >= _requiredAttendees)
        {
			int width = _maxAttendeeBarSize;
			
            _attendeeText.color = Color.green;
            _attendeeBar.sizeDelta = new Vector2(width, _attendeeBar.sizeDelta.y);
			_attendeeBar.anchoredPosition = new Vector2((width / 2) + _attendeeBarInitialX, _attendeeBar.anchoredPosition.y);
			
        }
        else
        {
			int width = (int)(((float)_attendeesPassed / (float)_requiredAttendees) * _maxAttendeeBarSize);
			
            _attendeeText.color = Color.red;
			_attendeeBar.sizeDelta = new Vector2(width, _attendeeBar.sizeDelta.y);
			_attendeeBar.anchoredPosition = new Vector2((width / 2) + _attendeeBarInitialX, _attendeeBar.anchoredPosition.y);
        }
    }


    //Randomize spawns
    private void RandomizeSpawns()
    {
        float offset = 1.45f;
        //Randomize start node
        if (Random.Range(0, 2) == 0)
        {
            if (Random.Range(0, 2) == 0)
                _startNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.max.y + offset);
            else
                _startNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.min.y - offset);
        }
        else
        {
            if (Random.Range(0, 2) == 0)
                _startNode.transform.position = new Vector2(_placing.ScreenBounds.min.x - offset, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
            else
                _startNode.transform.position = new Vector2(_placing.ScreenBounds.max.x + offset, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
        }

        do
        {
            //Randomize end node
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, 2) == 0)
                    _endNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.max.y + offset);
                else
                    _endNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.min.y - offset);
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                    _endNode.transform.position = new Vector2(_placing.ScreenBounds.min.x - offset, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
                else
                    _endNode.transform.position = new Vector2(_placing.ScreenBounds.max.x + offset, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
            }
        }
        while (Vector2.SqrMagnitude(_startNode.transform.position - _endNode.transform.position) <= MIN_SPAWN_NODE_DISTANCE * MIN_SPAWN_NODE_DISTANCE);

        do
        {
            //Randomize enemy spawn node
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, 2) == 0)
                    _spawnNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.max.y + offset);
                else
                    _spawnNode.transform.position = new Vector2(Random.Range(_placing.ScreenBounds.min.x, _placing.ScreenBounds.max.x), _placing.ScreenBounds.min.y - offset);
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                    _spawnNode.transform.position = new Vector2(_placing.ScreenBounds.min.x - offset, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
                else
                    _spawnNode.transform.position = new Vector2(_placing.ScreenBounds.max.x + offset, Random.Range(_placing.ScreenBounds.min.y, _placing.ScreenBounds.max.y));
            }
        }
        while(Vector2.SqrMagnitude(_startNode.transform.position - _spawnNode.transform.position) <= MIN_SPAWN_NODE_DISTANCE * MIN_SPAWN_NODE_DISTANCE || Vector2.SqrMagnitude(_endNode.transform.position - _spawnNode.transform.position) <= MIN_SPAWN_NODE_DISTANCE * MIN_SPAWN_NODE_DISTANCE);
    }
}
