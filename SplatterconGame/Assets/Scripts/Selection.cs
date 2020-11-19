using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

enum SelectionOptions { BOOTH, TRAP, SPELL }
public class SelectionScript : MonoBehaviour
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

    private SelectionOptions currentSelection;

    void Awake()
    {
        _boothButton.onClick.AddListener(BoothButton);
        _trapButton.onClick.AddListener(TrapButton);
        _spellButton.onClick.AddListener(SpellButton);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSelection(SelectionOptions selectionOptions)
    {

    }


    void BoothButton()
    {
        UpdateSelection(SelectionOptions.BOOTH);
    }

    void SpellButton()
    {
        UpdateSelection(SelectionOptions.TRAP);
    }

    void TrapButton()
    {
        UpdateSelection(SelectionOptions.SPELL);
    }

    void SelectionButton()
    {

    }


}
