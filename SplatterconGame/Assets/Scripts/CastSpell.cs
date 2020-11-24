using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CastSpell : MonoBehaviour
{
    private Camera _mainCamera;

    private GameObject current;

    private Vector2 mousePos;

    public GameObject spellOne;
    public GameObject spellTwo;
    


    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        current = spellOne;
    }

    public void Cast(string selectedSpell)
    {
        switch (selectedSpell)
        {
            case "Fire Spell":
                current = spellOne;
                break;
            case "Snow Spell":
                current = spellTwo;
                break;
            default:
                current = spellOne;
                break;
        }
        mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Destroy(Instantiate(current, mousePos, Quaternion.identity), 3.0f);
    }

}
