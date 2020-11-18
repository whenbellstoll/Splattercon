using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate bool PlaceDel(Vector2 pos);

public class Placing : MonoBehaviour
{
    [SerializeField]
    private GameObject _placingPrefab;
    [SerializeField]
    private SpriteRenderer _previewSprite;
    [SerializeField]
    private GameObject _defaultPlacedObjectContainer;

    public PlaceDel ExtraPlacingRule;

    private float _gridSeperation = 1.0f;
    private bool _placing = false;
    public bool IsPlacing => _placing;

    private GameObject _placedObjectContainer;

    private Camera _mainCamera;
    private Bounds _screenBounds;
    public Bounds ScreenBounds => _screenBounds;
    private float _boundsMargin = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        //Get screen bounds
        _mainCamera = Camera.main;
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = _mainCamera.orthographicSize * 2;
        _screenBounds = new Bounds(
            new Vector3(_mainCamera.transform.position.x, _mainCamera.transform.position.y, 0),
            new Vector3(cameraHeight * screenAspect - 2 * _boundsMargin, cameraHeight - 2 * _boundsMargin, 0.1f));

        _previewSprite.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_placing)
        {
            Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            //Snap mouse pos to grid
            mousePos /= _gridSeperation;
            mousePos.x = Mathf.Round(mousePos.x);
            mousePos.y = Mathf.Round(mousePos.y);
            mousePos *= _gridSeperation;

            //Preview where object will be placed
            _previewSprite.transform.position = mousePos;

            //Check is object can be placed
            bool canPlace = CanPlace(mousePos);
            if (canPlace)
                _previewSprite.color = new Color(1, 1, 1, _previewSprite.color.a);
            else
                _previewSprite.color = new Color(1, 0, 0, _previewSprite.color.a);

            //If mouse button is clicked, place object and stop placing mode
            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                _previewSprite.gameObject.SetActive(false);
                Instantiate(_placingPrefab, mousePos, _placingPrefab.transform.rotation, _placedObjectContainer.transform);
                _placing = false;
            }

            //If esc is pressed exit placing mode
            if (Input.GetKeyDown(KeyCode.E))
            {
                _previewSprite.gameObject.SetActive(false);
                _placing = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartPlacing();
        }
    }

    //Starts placing mode
    public void StartPlacing()
    {
        _previewSprite.gameObject.SetActive(true);
        _previewSprite.sprite = _placingPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        _previewSprite.transform.localScale = _placingPrefab.GetComponentInChildren<SpriteRenderer>().transform.lossyScale;
        _placing = true;
        ExtraPlacingRule = null;
    }

    //Starts placing mode with given object
    public void StartPlacing(GameObject prefab)
    {
        _placingPrefab = prefab;
        _placedObjectContainer = _defaultPlacedObjectContainer;
        _previewSprite.gameObject.SetActive(true);
        _previewSprite.sprite = _placingPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        _previewSprite.transform.localScale = _placingPrefab.GetComponentInChildren<SpriteRenderer>().transform.lossyScale;
        _placing = true;
        ExtraPlacingRule = null;
    }

    //Starts placing mode with given object
    public void StartPlacing(GameObject prefab, GameObject container)
    {
        _placingPrefab = prefab;
        _placedObjectContainer = container;
        _previewSprite.gameObject.SetActive(true);
        _previewSprite.sprite = _placingPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        _previewSprite.transform.localScale = _placingPrefab.GetComponentInChildren<SpriteRenderer>().transform.lossyScale;
        _placing = true;
        ExtraPlacingRule = null;
    }

    //Starts placing mode with given object and placing rule
    public void StartPlacing(GameObject prefab, GameObject container, PlaceDel placingRule)
    {
        _placingPrefab = prefab;
        _placedObjectContainer = container;
        _previewSprite.gameObject.SetActive(true);
        _previewSprite.sprite = _placingPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        _previewSprite.transform.localScale = _placingPrefab.GetComponentInChildren<SpriteRenderer>().transform.lossyScale;
        _placing = true;
        ExtraPlacingRule = placingRule;
    }


    //Checks if object is valid for placing
    public bool CanPlace(Vector2 pos)
    {
        //Make sure another object is not in that space
        for (int i = 0; i < _placedObjectContainer.transform.childCount; i++)
        {
            if (pos == (Vector2)_placedObjectContainer.transform.GetChild(i).position)
                return false;
        }

        //Make sure mouse pos is in bounds
        if (!_screenBounds.Contains(pos))
        {
            return false;
        }

        //Add extra placing rules as necessary
        if (ExtraPlacingRule != null && ExtraPlacingRule(pos))
        {
            return false;
        }

        return true;
    }
}
