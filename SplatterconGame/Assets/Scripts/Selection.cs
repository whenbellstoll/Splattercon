using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum SelectionGroups { BOOTH, TRAP, SPELL }

public class Selection : MonoBehaviour
{

    [Header("Buttons")]
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

    [Header("Selection Containers")]
    [SerializeField]
    private GameObject _boothContainer;
    [SerializeField]
    private GameObject _trapContainer;
    [SerializeField]
    private GameObject _spellContainer;

    [Header("Amount Objects")]
    [SerializeField]
    private Text _amountText;
    [SerializeField]
    private GameObject _zeroMask;

    private List<SelectionGroup> _selectionGroups;

    private SelectionGroup _currentSelectedGroup;

    void Awake()
    {

        // Add clickk events for all buttons
        _boothButton.onClick.AddListener(BoothButton);
        _trapButton.onClick.AddListener(TrapButton);
        _spellButton.onClick.AddListener(SpellButton);
        _selectionButton.onClick.AddListener(SelectionButton);

        _selectionGroups = new List<SelectionGroup>();
        
        // Create a new selection group where the options are the children in the parent group.
        _selectionGroups.Add(new AmountSelectionGroup(SelectionGroups.BOOTH, _boothContainer, _amountText, _zeroMask));
        _selectionGroups.Add(new AmountSelectionGroup(SelectionGroups.TRAP, _trapContainer, _amountText, _zeroMask));
        _selectionGroups.Add(new SelectionGroup(SelectionGroups.SPELL, _spellContainer));


        // Start off with the booth selected
        UpdateSelection(SelectionGroups.BOOTH);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Detect scroll wheel movements
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            _currentSelectedGroup.Increment();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            _currentSelectedGroup.Decrement();
        }
    }


    // Update the current selection group we have selected (i.e. Booths, Traps, or Spells)
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


    // Check to see if the current type we input is of the AmountSelectionGroup so we can call methods on that object
    AmountSelectionGroup GetAmountSelection(SelectionGroups groupType)
    {
        foreach (SelectionGroup selectionGroup in _selectionGroups)
        {
            if (selectionGroup is AmountSelectionGroup && selectionGroup.GroupType == groupType)
            {
                AmountSelectionGroup currentAmountSelection = (AmountSelectionGroup)selectionGroup;
                return currentAmountSelection;
            }
        }

        throw new System.Exception("Selection group inputed is not of type AmountSelectionGroup");
    }


    /*
     *   ===== Below are all the methods we need to call from outside this class in the Game Manager or elsewhere. ======
     *                                                  |
     *                                                  |
     *                                                  V
    */


    // Get the current name of the object we have selected
    // !!! This is how we check to see what to place, and it will have the same name as the option in the Unity Heirachy !!!
    public string GetCurrentSelectionName()
    {
        return _currentSelectedGroup.SelectionName;
    }


    // Check to see if the currently selected group has every single option at zero
    // !!! This should not be used to check if we have any current booths yet !!!
    public bool AllZero()
    {
        if(_currentSelectedGroup is AmountSelectionGroup)
        {
            AmountSelectionGroup currentAmountSelection = (AmountSelectionGroup)_currentSelectedGroup;
            return currentAmountSelection.AllZero();
        }
        else
        {
            return false;
        }
    }


    // Check to see if the group we input has any remaining objects that can be placed
    // !!! This is very useful in checking to see if we have any remaining booths to place !!!
    public bool AllZero(SelectionGroups groupType)
    {
        foreach (SelectionGroup selectionGroup in _selectionGroups)
        {
            if (selectionGroup is AmountSelectionGroup && selectionGroup.GroupType == groupType)
            {
                AmountSelectionGroup currentAmountSelection = (AmountSelectionGroup)selectionGroup;
                return currentAmountSelection.AllZero();
            }
        }

        return false;
    }

    // Check to see if the option we have currently selected has any remaining objects we can place.
    // !!! Very useful in checking to see if we can place anymore traps, or booths !!!
    public bool IsZero()
    {
        if (_currentSelectedGroup is AmountSelectionGroup)
        {
            AmountSelectionGroup currentAmountSelection = (AmountSelectionGroup)_currentSelectedGroup;
            return currentAmountSelection.IsZero();
        }
        else
        {
            return false;
        }
    }

    // This version of SetAmount will take in a number and evenly divide the amounts between the options in the groupType as best it can.
    // For example, if we have 2 booths and we suply 3 for amount then the first booth would have 2, and the second would have 1
    // !!! Very useful in passing in a single number to determine how many booths we can place !!!
    public void SetAmount(SelectionGroups groupType, int amount)
    {
        AmountSelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        currentAmountSelection.SetAmount(amount);
        _currentSelectedGroup.UpdateSprites();
    }


    // This version of SetAmount takes in a list of amounts which will corespond to the options we have in our group. The list should be the same length of the number
    // of options represented in the heirachy. Applies to the supplied groupType
    // ex.    Traps 
    //            Bear Trap
    //            Lava Trap
    // If you supply SelectionGroups.TRAP, [3, 4] then Bear Trap would have 3, and Lava Trap would have 4

    public void SetAmount(SelectionGroups groupType, List<int> amounts)
    {
        AmountSelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        currentAmountSelection.SetAmount(amounts);
        _currentSelectedGroup.UpdateSprites();
    }


    // This version of SetAmount takes in a dictionary with the name of the option, and the amount we want to set it to. Applies to the supplied groupType
    // of options represented in the heirachy.
    // ex.    Traps 
    //            Bear Trap
    //            Lava Trap
    // If you supply SelectionGroups.TRAP, [Bear Trap => 2, Lava Trap => 5] then Bear Trap would have 2, and Lava Trap would have 5
    public void SetAmount(SelectionGroups groupType, Dictionary<string, int> amounts)
    {
        AmountSelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        currentAmountSelection.SetAmount(amounts);
        _currentSelectedGroup.UpdateSprites();
    }


    // Decrement the current selection by a value of 1
    // !!! Very useful if when we place an object !!!
    public void DecrementCurrentSelection()
    {
        if (_currentSelectedGroup is AmountSelectionGroup)
        {
            AmountSelectionGroup currentAmountSelection = (AmountSelectionGroup)_currentSelectedGroup;
            currentAmountSelection.DecrementAmount();
        }
        _currentSelectedGroup.UpdateSprites();
    }

    /*
     *  =====  End of methods called from elsewhere ======
     * 
    */

}


/// <summary>
/// Class which holds the different options for a certain selection group (i.e. Booths, Traps, or Spells)
/// </summary>
class SelectionGroup
{

    private SelectionGroups _selectionGroup;

    protected List<GameObject> _selections;

    protected int _selectionIndex;

    // Get the current group this object represents
    public SelectionGroups GroupType
    {
        get { return _selectionGroup; }
    }

    // Get the current name of the option we have selected
    public string SelectionName
    {
        get { return _selections[_selectionIndex].name; }
    }

    public SelectionGroup(SelectionGroups selectionGroup, GameObject parentObject)
    {

        _selectionGroup = selectionGroup;
        _selections = GetChildren(parentObject);
        _selectionIndex = 0;

        // If there are no children that means we have no options to choose from so throw an error for safety.
        if(_selections.Count == 0)
        {
            throw new System.Exception("Selection Group " + selectionGroup + " has no child objects! Please add child objects to the group.");
        }

    }

    // Get all the children from the parent, and create a list of all the potential options we can select from.
    List<GameObject> GetChildren(GameObject parent)
    {
        List<GameObject> childrenObjects = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            childrenObjects.Add(child.gameObject);
        }

        return childrenObjects;
    }


    // Update the current sprite that is on the selection button so we know what we are placing
    public virtual void UpdateSprites()
    {

        foreach (GameObject option in _selections)
        {
            option.SetActive(false);
        }

        _selections[_selectionIndex].SetActive(true);

    }

    // Hide all other possible selections. This is used when we are changing the selection groups
    public virtual void HideAll()
    {

        foreach (GameObject option in _selections)
        {
            option.SetActive(false);
        }

    }

    // Increment the current option we have selected, then draw the sprite with UpdateSprites
    public void Increment()
    {
        _selectionIndex++;

        if(_selectionIndex >= _selections.Count)
        {
            _selectionIndex = 0;
        }

        UpdateSprites();

    }

    // Decrement the current option we have selected, then draw the sprite with UpdateSprites.
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


/// <summary>
/// This is a special type of selection group where we can also set the amounts of the objects we can place (used for Booths, and Traps)
/// </summary>
class AmountSelectionGroup : SelectionGroup
{

    // This selection amount array should theoretically have the same amount of entries as the number of options we have
    private List<int> _selectionAmount;

    private Text _amountText; // The text for the amount we want to represent
    private GameObject _mask; // Opaque cover when we have none left

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

        // Update the current amount with the option we have selected
        _amountText.gameObject.SetActive(true);
        _amountText.text = "x" + _selectionAmount[_selectionIndex];

        // If we have none left draw it with a mask
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

        // Hide the mask, and text
        _amountText.gameObject.SetActive(false);
        _mask.SetActive(false);

    }


    // This version of SetAmount will take in a number and evenly divide the amounts between the options as best it can.
    // For example, if we have 2 booths and we suplly 3 for amount then the first booth would have 2, and the second would have 1
    public void SetAmount(int amount)
    {

        int splitAmount = (int) Mathf.Ceil((float)amount / _selections.Count);

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


    }


    // This version of SetAmount takes in a list of amounts which will corespond to the options we have in our group.
    // It is very important to note that the amounts will follow the same flow as the options do in the unity editor.
    // So the first index will correspond to the first GameObject in whataever selection container we are in
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


    // This version of SetAmount takes in a dictionary with the name of the option, and the amount we want to set it to.
    // The name of the option is the SAME exact name as it is in the Unity Heirachy panel.
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

    // Decrease the current amount we have selected.
    public void DecrementAmount()
    {
        _selectionAmount[_selectionIndex]--;
    }


    // Test to see if all the options in the group have zero left (this will be helpful in detecting if we have placed all the booths)
    public bool AllZero()
    {

        bool allZero = true;

        for (int i = 0; i < _selectionAmount.Count; i++)
        {
            if(_selectionAmount[i] > 0)
            {
                allZero = false;
            }
        }

        return allZero;
    }


    // Test to see if there are any things left to place (this will be useful when we want to see if we can place any more objects.)
    public bool IsZero()
    {
        if(_selectionAmount[_selectionIndex] <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}