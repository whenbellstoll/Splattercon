using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

enum SelectionGroups { BOOTH, TRAP, SPELL }

public class Selection : MonoBehaviour
{

    [SerializeField]
    private Button _boothButton;
    [SerializeField]
    private Button _trapButton;
    [SerializeField]
    private Button _spellButton;
    [SerializeField]
    private Button _selectionButton;
    [SerializeField]
    private Button _playButton;

    [SerializeField]
    private GameObject _boothContainer;
    [SerializeField]
    private GameObject _trapContainer;
    [SerializeField]
    private GameObject _spellContainer;

    [SerializeField]
    private Text _amountText;
    [SerializeField]
    private GameObject _zeroMask;

    private List<SelectionGroup> _selectionGroups;

    private SelectionGroup _currentSelectedGroup;

    void Awake()
    {
        _boothButton.onClick.AddListener(BoothButton);
        _trapButton.onClick.AddListener(TrapButton);
        _spellButton.onClick.AddListener(SpellButton);
        _selectionButton.onClick.AddListener(SelectionButton);

        _selectionGroups = new List<SelectionGroup>();
        

        _selectionGroups.Add(new AmountSelectionGroup(SelectionGroups.BOOTH, _boothContainer, _amountText, _zeroMask));
        _selectionGroups.Add(new AmountSelectionGroup(SelectionGroups.TRAP, _trapContainer, _amountText, _zeroMask));
        _selectionGroups.Add(new SelectionGroup(SelectionGroups.SPELL, _spellContainer));

        UpdateSelection(SelectionGroups.BOOTH);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            _currentSelectedGroup.Increment();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            _currentSelectedGroup.Decrement();
        }
    }

    void UpdateSelection(SelectionGroups selectedGroup)
    {
        if (_currentSelectedGroup != null)
        {
            _currentSelectedGroup.HideAll();
        }

        foreach(SelectionGroup selectionGroup in _selectionGroups)
        {
            if (selectionGroup.GroupType == selectedGroup)
            {
                _currentSelectedGroup = selectionGroup;
                break;
            }
        }

        _currentSelectedGroup.UpdateSprites();
    }


    void BoothButton()
    {
        UpdateSelection(SelectionGroups.BOOTH);
    }

    void SpellButton()
    {
        UpdateSelection(SelectionGroups.SPELL);
    }

    void TrapButton()
    {
        UpdateSelection(SelectionGroups.TRAP);
    }

    void SelectionButton()
    {
        _currentSelectedGroup.Increment();
    }

    public string GetCurrentSelectionName()
    {
        return _currentSelectedGroup.SelectionName;
    }

}



class SelectionGroup
{

    private SelectionGroups _selectionGroup;

    protected List<GameObject> _selections;

    protected int _selectionIndex;

    public SelectionGroups GroupType
    {
        get { return _selectionGroup; }
    }

    public string SelectionName
    {
        get { return _selections[_selectionIndex].name; }
    }

    public SelectionGroup(SelectionGroups selectionGroup, GameObject parentObject)
    {

        _selectionGroup = selectionGroup;
        _selections = GetChildren(parentObject);
        _selectionIndex = 0;

        if(_selections.Count == 0)
        {
            throw new System.Exception("Selection Group " + selectionGroup + " has no child objects! Please add child objects to the group.");
        }

    }

    List<GameObject> GetChildren(GameObject parent)
    {
        List<GameObject> childrenObjects = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            childrenObjects.Add(child.gameObject);
        }

        return childrenObjects;
    }


    public virtual void UpdateSprites()
    {

        foreach (GameObject option in _selections)
        {
            option.SetActive(false);
        }

        _selections[_selectionIndex].SetActive(true);

    }

    public virtual void HideAll()
    {

        foreach (GameObject option in _selections)
        {
            option.SetActive(false);
        }

    }

    public void Increment()
    {
        _selectionIndex++;

        if(_selectionIndex >= _selections.Count)
        {
            _selectionIndex = 0;
        }

        UpdateSprites();

    }

    public void Decrement()
    {
        _selectionIndex--;

        if (_selectionIndex < 0)
        {
            _selectionIndex = _selections.Count - 1;
        }

        UpdateSprites();

    }

}



class AmountSelectionGroup : SelectionGroup
{

    private List<int> _selectionAmount;

    private Text _amountText;
    private GameObject _mask;

    public AmountSelectionGroup(SelectionGroups selectionGroup, GameObject parentObject, Text amountText, GameObject mask)
        : base(selectionGroup, parentObject)
    {

        _selectionAmount = new List<int>();
        _amountText = amountText;
        _mask = mask;

        SetAmount(0);

    }

    public override void UpdateSprites()
    {
        base.UpdateSprites();

        _amountText.gameObject.SetActive(true);

        Debug.Log("x" + _selectionAmount[_selectionIndex]);

        _amountText.text = "x" + _selectionAmount[_selectionIndex];

        if(_selectionAmount[_selectionIndex] == 0)
        {
            _mask.SetActive(true);
        }
        else
        {
            _mask.SetActive(false);
        }

    }

    public override void HideAll()
    {

        base.HideAll();

        _amountText.gameObject.SetActive(false);
        _mask.SetActive(false);

    }


    public void SetAmount(int amount)
    {

        int splitAmount = (int) Mathf.Ceil(amount / _selections.Count);

        _selectionAmount = new List<int>();

        for (int i = 0; i < _selections.Count; i++)
        {
            if(amount - splitAmount <= 0)
            {
                _selectionAmount.Add(amount);
            }
            else
            {
                amount -= splitAmount;
                _selectionAmount.Add(splitAmount);
            }
        }

        Debug.Log(_selectionAmount.Count);

    }

    public void SetAmount(List<int> amounts)
    {
        if(amounts.Count != _selections.Count)
        {
            throw new System.Exception("List supplied does not have the same number of entries as the number of options!");
        }

        _selectionAmount = new List<int>();

        for (int i = 0; i < amounts.Count; i++)
        {
            _selectionAmount.Add(amounts[i]);
        }

    }

    public void SetAmount(Dictionary<string, int> amounts)
    {

        foreach (KeyValuePair<string, int> amount in amounts)
        {

            for (int i = 0; i < _selections.Count; i++)
            {
                if (amount.Key == _selections[i].name)
                {
                    _selectionAmount[i] = amount.Value;
                }
            }

        }

    }

}