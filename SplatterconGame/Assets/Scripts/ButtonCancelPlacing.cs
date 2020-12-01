using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCancelPlacing : MonoBehaviour
{


    [SerializeField]
    private Placing _placing;
    [SerializeField]
    private Selection _select;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StopPlacing()
    {
        if(_select.GetCurrentSelectionGroup() == SelectionGroups.BOOTH || _select.GetCurrentSelectionGroup() == SelectionGroups.TRAP) _placing.CancelPlacing();
    }

    public void StartPlacing()
    {
        if (_select.GetCurrentSelectionGroup() == SelectionGroups.BOOTH || _select.GetCurrentSelectionGroup() == SelectionGroups.TRAP) _placing.ResumePlacing();
    }
}
