using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum SelectionGroups { BOOTH, TRAP, SPELL }
public delegate void SelectionCallback();

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
    private Text _moneyText;
    [SerializeField]
    private GameObject _zeroMask;
    [SerializeField]
    private GameObject _playMask;

    [Header("Spell Recharge Bar")]
    [SerializeField]
    private float[] _rechargeRates;
    [SerializeField]
    private Image _rechargeBar;


    private List<SelectionGroup> _selectionGroups;
    private RechargeBar[] _rechargeBars;
    private SelectionGroup _currentSelectedGroup;
    private Money _money;
    
    private bool _playButtonClicked;
    private Vector3 _selectionButtonInitialPosition;
    public SelectionCallback OnSelectionChange;

    void Awake()
    {

        // Add clickk events for all buttons
        _boothButton.onClick.AddListener(BoothButton);
        _trapButton.onClick.AddListener(TrapButton);
        _spellButton.onClick.AddListener(SpellButton);
        _selectionButton.onClick.AddListener(SelectionButton);
        _playButton.onClick.AddListener(PlayButton);

        _selectionGroups = new List<SelectionGroup>();
        _money = new Money(_moneyText, 350);
        _selectionButtonInitialPosition = _selectionButton.transform.localPosition;

        _rechargeBars = new RechargeBar[_rechargeRates.Length];
        for (int i = 0; i < _rechargeRates.Length; i++)
        {
            _rechargeBars[i] = new RechargeBar(_rechargeBar, _rechargeRates[i]);
        }

        // Create a new selection group where the options are the children in the parent group.
        _selectionGroups.Add(new BoothGroup(_boothContainer, _money, _amountText, _zeroMask));
        _selectionGroups.Add(new TrapGroup(_trapContainer, _money, _amountText, _zeroMask));
        _selectionGroups.Add(new SpellGroup(_spellContainer, _money, _amountText, _zeroMask, _rechargeBars));


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
            if (_currentSelectedGroup.Increment())
                OnSelectionChange?.Invoke();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            if(_currentSelectedGroup.Decrement())
                OnSelectionChange?.Invoke();

        }

        if(_currentSelectedGroup.GroupType == SelectionGroups.SPELL)
        {
            _rechargeBar.gameObject.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            _currentSelectedGroup.UpdateSprites();
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

        OnSelectionChange?.Invoke();
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

    void PlayButton()
    {
        if (AllZero(SelectionGroups.BOOTH))
        {
            _playButtonClicked = true;
            UpdateSelection(SelectionGroups.SPELL);

        }
    }

    void SelectionButton()
    {
        _currentSelectedGroup.Increment();
        OnSelectionChange?.Invoke();
    }


    // Check to see if the current type we input is of the AmountSelectionGroup so we can call methods on that object
    SelectionGroup GetAmountSelection(SelectionGroups groupType)
    {
        foreach (SelectionGroup selectionGroup in _selectionGroups)
        {
            if (selectionGroup.GroupType == groupType)
            {
                return selectionGroup;
            }
        }

        throw new System.Exception("Could not find selection group inputed!");
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

    // Get the current group for the current selection we have.
    public SelectionGroups GetCurrentSelectionGroup()
    {
        return _currentSelectedGroup.GroupType;
    }


    // Check to see if the currently selected group has every single option at zero
    // !!! This should not be used to check if we have any current booths to place !!!
    public bool AllZero()
    { 
        return _currentSelectedGroup.AllZero();
    }


    // Check to see if the group we input has any remaining objects that can be placed
    // !!! This is very useful in checking to see if we have any remaining booths to place !!!
    public bool AllZero(SelectionGroups groupType)
    {
        SelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        return currentAmountSelection.AllZero();
    }

    // Check to see if the option we have currently selected has any remaining objects we can place.
    // !!! Very useful in checking to see if we can place anymore traps, or booths !!!
    public bool IsZero()
    {
        return _currentSelectedGroup.IsZero();
    }

    // This version of SetAmount will take in a number and evenly divide the amounts between the options in the groupType as best it can.
    // For example, if we have 2 booths and we suply 3 for amount then the first booth would have 2, and the second would have 1
    // !!! Very useful in passing in a single number to determine how many booths we can place !!!
    public void SetAmount(SelectionGroups groupType, int amount)
    {
        SelectionGroup currentAmountSelection = GetAmountSelection(groupType);
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
        SelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        currentAmountSelection.SetAmount(amounts);
        _currentSelectedGroup.UpdateSprites();
    }


    // This version of SetAmount takes in a the name of the option, and the amount we want to set it to. Applies to the supplied groupType
    // of options represented in the heirachy.
    // ex.    Traps 
    //            Bear Trap
    //            Lava Trap
    // If you supply (SelectionGroups.TRAP, "Bear Trap", 2) then Bear Trap would have 2
    public void SetAmount(SelectionGroups groupType, string objectName, int amount)
    {
        SelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        currentAmountSelection.SetAmount(objectName, amount);
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
        SelectionGroup currentAmountSelection = GetAmountSelection(groupType);
        currentAmountSelection.SetAmount(amounts);
        _currentSelectedGroup.UpdateSprites();
    }


    // Decrement the current selection by a value of 1, or purchasing with Money
    // !!! Very useful when we place an object !!!
    public void DecrementCurrentSelection()
    {
        _currentSelectedGroup.DecrementAmount();

        if(_currentSelectedGroup.IsZero() && !_currentSelectedGroup.AllZero())
        {
            while(_currentSelectedGroup.IsZero())
            {
                _currentSelectedGroup.Increment();
            }
            OnSelectionChange?.Invoke();
        }

        _currentSelectedGroup.UpdateSprites();
    }


    // Add money to the money counter
    public void AddMoney(int amount)
    {
        _money.AddMoney(amount);
    }


    // Subtract money from the money counter
    public void SubtractMoney(int amount) {
        _money.SubtractMoney(amount);
    }


    // Hide the UI buttons.
    public void HideButtons()
    {
        _boothButton.gameObject.SetActive(false);
        _trapButton.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(false);
        _spellButton.gameObject.SetActive(false);

        _selectionButton.transform.localPosition = _spellButton.transform.localPosition;

        _playButtonClicked = false;
    }

    // Show the UI Buttons
    public void ShowButtons()
    {
        _boothButton.gameObject.SetActive(true);
        _trapButton.gameObject.SetActive(true);
        _playButton.gameObject.SetActive(true);
        _spellButton.gameObject.SetActive(true);

        _selectionButton.transform.localPosition = _selectionButtonInitialPosition;
    }

    // Test to see if the play button was clicked
    public bool PlayButtonClicked()
    {
        return _playButtonClicked;
    }

    public void UpdatePlayMask()
    {
        if (AllZero(SelectionGroups.BOOTH))
        {
            _playMask.SetActive(false);
        }
        else
        {
            _playMask.SetActive(true);
        }
    }

    /*
     *  =====  End of methods called from elsewhere ======
     * 
    */

}


public class Money
{

    private Text _moneyText;
    private int _amount;

    public int Amount
    {
        get { return _amount; }

    }

    public Money(Text text, int staringAmount)
    {
        _moneyText = text;
        _amount = staringAmount;

        UpdateMoneyText();


    }

    public void UpdateMoneyText()
    {
        _moneyText.text = "$" + _amount;
    }

    public void AddMoney(int addMoney)
    {
        _amount += addMoney;
        UpdateMoneyText();
    }

    public void SubtractMoney(int subMoney)
    {
        _amount -= subMoney;
        UpdateMoneyText();
    }


}

public class RechargeBar
{

    private Image _rechargeBarImage;
    private float _rechargeRate;

    private float _rechargeTimer;

    public RechargeBar(Image rechargeBarImage, float rechargeRate)
    {

        _rechargeBarImage = rechargeBarImage;
        _rechargeRate = rechargeRate;

        _rechargeTimer = 0.0f;

    }

    public void UpdateRate()
    {

        _rechargeTimer -= Time.deltaTime;

        if(_rechargeTimer <= 0.0)
        {
            _rechargeTimer = 0.0f;
        }
    }

    public void UpdateLength()
    {
        SetLength();
    }

    public void SetLength()
    {

        _rechargeBarImage.fillAmount = (Mathf.Abs((_rechargeTimer - _rechargeRate) / _rechargeRate));

    }

    public void StartTimer()
    {
        _rechargeTimer = _rechargeRate;
    }

    public bool TimerEnded()
    {
        if(_rechargeTimer <= 0.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HideBar()
    {
        _rechargeBarImage.gameObject.SetActive(false);
    }

    public void ShowBar()
    {
        _rechargeBarImage.gameObject.SetActive(true);
    }

}



/// <summary>
/// Class which holds the different options for a certain selection group (i.e. Booths, Traps, or Spells)
/// </summary>
public abstract class SelectionGroup
{

    protected SelectionGroups _selectionGroup;

    // This selection amount array should theoretically have the same amount of entries as the number of options we have
    protected List<int> _selectionAmount;
    protected List<GameObject> _selections;

    protected int _selectionIndex;

    protected Text _amountText; // The text for the amount we want to represent
    protected Money _money;
    protected GameObject _mask; // Opaque cover when we have none left

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

    public int SelectionIndex
    {
        get { return _selectionIndex; }
    }

    public SelectionGroup(GameObject parentObject, Money money, Text amountText, GameObject mask)
    {

        _selectionGroup = SelectionGroups.BOOTH;
        _selections = GetChildren(parentObject);
        _selectionIndex = 0;

        // If there are no children that means we have no options to choose from so throw an error for safety.
        if (_selections.Count == 0)
        {
            throw new System.Exception("Selection Group " + _selectionGroup + " has no child objects! Please add child objects to the group.");
        }

        _selectionAmount = new List<int>();
        _amountText = amountText;
        _mask = mask;
        _money = money;

        SetAmount(0);

    }

    public abstract void DecrementAmount();
    public abstract bool AllZero();
    public abstract bool IsZero();

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
    public bool Increment()
    {
        if (_selections.Count == 0)
            return false;
        else
        {
            _selectionIndex++;

            if (_selectionIndex >= _selections.Count)
            {
                _selectionIndex = 0;
            }

            UpdateSprites();
            return true;
        }

    }

    // Decrement the current option we have selected, then draw the sprite with UpdateSprites.
    public bool Decrement()
    {
        if (_selections.Count == 0)
        {
            return false;
        }
        else
        {
            _selectionIndex--;

            if (_selectionIndex < 0)
            {
                _selectionIndex = _selections.Count - 1;
            }

            UpdateSprites();
            return true;
        }
    }


    // This version of SetAmount will take in a number and evenly divide the amounts between the options as best it can.
    // For example, if we have 2 booths and we suplly 3 for amount then the first booth would have 2, and the second would have 1
    public void SetAmount(int amount)
    {

        int splitAmount = (int)Mathf.Ceil((float)amount / _selections.Count);

        _selectionAmount = new List<int>();

        for (int i = 0; i < _selections.Count; i++)
        {
            if (amount - splitAmount <= 0)
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
        if (amounts.Count != _selections.Count)
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
    public void SetAmount(string objectName, int amount)
    {

        for (int i = 0; i < _selections.Count; i++)
        {
            if (objectName == _selections[i].name)
            {
                _selectionAmount[i] = amount;
            }
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



}


/// <summary>
/// This is a special type of selection group where we can also set the amounts of the objects we can place (used for Booths, and Traps)
/// </summary>
public class BoothGroup : SelectionGroup
{


    public BoothGroup(GameObject parentObject, Money money, Text amountText, GameObject mask)
        : base(parentObject, money, amountText, mask)
    {

        _selectionGroup = SelectionGroups.BOOTH;
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

    // Decrease the current amount we have selected.
    public override void DecrementAmount()
    {
        _selectionAmount[_selectionIndex]--;
        if (_selectionAmount[_selectionIndex] < 0)
            _selectionAmount[_selectionIndex] = 0;
    }


    // Test to see if all the options in the group have zero left (this will be helpful in detecting if we have placed all the booths)
    public override bool AllZero()
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
    public override bool IsZero()
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


/// <summary>
/// This is a special type of selection group where we can also set the amounts of the objects we can place (used for Booths, and Traps)
/// </summary>
public class TrapGroup : SelectionGroup
{


    public TrapGroup(GameObject parentObject, Money money, Text amountText, GameObject mask)
        : base(parentObject, money, amountText, mask)
    {

        _selectionGroup = SelectionGroups.TRAP;
    }

    public override void UpdateSprites()
    {
        base.UpdateSprites();

        // Update the current amount with the option we have selected
        _amountText.gameObject.SetActive(true);
        _amountText.text = "$" + _selectionAmount[_selectionIndex];

        // If we have none left draw it with a mask
        if (_selectionAmount[_selectionIndex] > _money.Amount)
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

    public override void DecrementAmount()
    {

        int currentAmount = _selectionAmount[_selectionIndex];

        // If we have none left draw it with a mask
        if (currentAmount <= _money.Amount)
        {
            _money.SubtractMoney(currentAmount);
        }
    }

    public override bool AllZero()
    {
        bool allZero = true;

        for (int i = 0; i < _selectionAmount.Count; i++)
        {
            if (_selectionAmount[i] <= _money.Amount)
            {
                allZero = false;
            }
        }

        return allZero;
    }

    public override bool IsZero()
    {
        if (_selectionAmount[_selectionIndex] > _money.Amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}



/// <summary>
/// This is a special type of selection group where we can also set the amounts of the objects we can place (used for Booths, and Traps)
/// </summary>
public class SpellGroup : SelectionGroup
{


    private RechargeBar[] _rechargeBars;

    public SpellGroup(GameObject parentObject, Money money, Text amountText, GameObject mask, RechargeBar[] rechargeBars)
        : base(parentObject, money, amountText, mask)
    {

        _selectionGroup = SelectionGroups.SPELL;

        _rechargeBars = rechargeBars;
    }

    public override void UpdateSprites()
    {
        base.UpdateSprites();

        // Update the current amount with the option we have selected
        _amountText.gameObject.SetActive(true);
        _amountText.text = "$" + _selectionAmount[_selectionIndex];

        // If we have none left draw it with a mask
        if (IsZero())
        {
            _mask.SetActive(true);
        }
        else
        {
            _mask.SetActive(false);
        }

        UpdateRates();

        if (_rechargeBars[_selectionIndex].TimerEnded())
        {
            _rechargeBars[_selectionIndex].HideBar();
            Cursor.visible = true;
        }
        else
        {
            _rechargeBars[_selectionIndex].ShowBar();
            _rechargeBars[_selectionIndex].UpdateLength();
            Cursor.visible = false;
        }


    }

    public void UpdateRates()
    {

        // Update all recharge bar rats
        for (int i = 0; i < _rechargeBars.Length; i++)
        {
            _rechargeBars[i].UpdateRate();
        }

    }

    public override void HideAll()
    {

        base.HideAll();

        // Hide the mask, and text
        _amountText.gameObject.SetActive(false);
        _mask.SetActive(false);

        _rechargeBars[_selectionIndex].HideBar();
        Cursor.visible = true;

    }



    public override void DecrementAmount()
    {

        int currentAmount = _selectionAmount[_selectionIndex];

        // If we have none left draw it with a mask
        if (currentAmount <= _money.Amount && _rechargeBars[_selectionIndex].TimerEnded())
        {
            _money.SubtractMoney(currentAmount);
            _rechargeBars[_selectionIndex].StartTimer();
        }
    }

    public override bool AllZero()
    {
        bool allZero = true;

        for (int i = 0; i < _selectionAmount.Count; i++)
        {
            if (_selectionAmount[i] <= _money.Amount && _rechargeBars[_selectionIndex].TimerEnded())
            {
                allZero = false;
                break;
            }
        }

        return allZero;
    }

    public override bool IsZero()
    {
        if (_selectionAmount[_selectionIndex] > _money.Amount || !_rechargeBars[_selectionIndex].TimerEnded())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}